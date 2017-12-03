using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace NetCoreStack.Jobs
{
    public static class BackgroundJobServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseNetCoreStackJobServer(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            ThrowIfNotConfigured(app);

            var services = app.ApplicationServices;
            var lifetime = services.GetRequiredService<IApplicationLifetime>();
            var loggerFactory = services.GetService<ILoggerFactory>();
            var options = services.GetService<JobBuilderOptions>();
            var jobStorage = services.GetService<IJobStorage>();
            var server = new ProcessServer(services, options, jobStorage, loggerFactory);

            lifetime.ApplicationStopping.Register(() => server.SendStop());
            lifetime.ApplicationStopped.Register(() => server.Dispose());

            return app;
        }

        private static void ThrowIfNotConfigured(IApplicationBuilder app)
        {
            var configuration = app.ApplicationServices.GetService<BackgroundJobServerMarkerService>();
            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddBackgroundJobServer' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }
        }
    }
}
