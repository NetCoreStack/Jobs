using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreStack.Jobs.TestHosting.Jobs;

namespace NetCoreStack.Jobs.TestHosting
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddNetCoreStackJobServer(setup =>
            {
                setup.Register<JobWorker>(Cron.Minutely());
                setup.Register<SampleJob>(Cron.Minutely());
                setup.Register<SampleJob2>(Cron.MinuteInterval(2));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseNetCoreStackJobServer();
        }
    }
}
