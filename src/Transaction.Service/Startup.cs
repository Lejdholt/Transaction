using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace Transaction.Service
{
 
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing((builder) => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(this.Configuration.GetValue<string>("Service:Name")))
                .AddSource(nameof(ObservableReceiveActor), nameof(AkkaService), nameof(KafkaActor))
                .AddAspNetCoreInstrumentation()
                .AddJaegerExporter(jaegerOptions =>
                {
                    jaegerOptions.AgentHost = this.Configuration.GetValue<string>("Jaeger:Host");
                    jaegerOptions.AgentPort = this.Configuration.GetValue<int>("Jaeger:Port");
                }));

            services.AddSingleton<AkkaService>();
            services.AddHostedService<AkkaService>(p => p.GetService<AkkaService>());



            services.AddSwaggerGen();
            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting(); 
            app.UseHttpMetrics();

            app.UseEndpoints(e =>
            {
                e.MapControllers();

                e.MapMetrics();
            });
        }
    }
}
