using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;

namespace NetCoreStack.Jobs
{
    public static class BackgroundJobServerServiceCollectionExtensions
    {
        private static readonly MethodInfo registryDelegate = 
            typeof(BackgroundJobServerServiceCollectionExtensions).GetTypeInfo().GetDeclaredMethod(nameof(BackgroundJobServerServiceCollectionExtensions.RegisterJob));

        public static IServiceCollection AddNetCoreStackJobServer(this IServiceCollection services, Action<JobBuilderOptions> setup)
        {
            services.AddMemoryCache();
            services.AddOptions();

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            services.AddSingleton<BackgroundJobServerMarkerService>();

            var opts = new JobBuilderOptions();
            setup?.Invoke(opts);
            foreach (var item in opts.JobList)
            {
                var type = item.Type;
                var genericRegistry = registryDelegate.MakeGenericMethod(type);
                genericRegistry.Invoke(null, new object[] { services, type });
            }

            services.AddSingleton(opts);

            var assemblies = opts.JobList.Select(j => j.GetType().Assembly).Distinct().ToList();
            var assemblyOptions = new AssemblyOptions(assemblies);
            services.AddSingleton(assemblyOptions);

            services.TryAdd(ServiceDescriptor.Singleton<IJobStorage, DefaultJobStorage>());

            return services;
        }

        public static IServiceCollection AddNetCoreStackJobServer(this IServiceCollection services, 
            Action<JobBuilderOptions> setup, 
            Action<RedisStorageOptions> redisSetup)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            if (redisSetup == null)
            {
                throw new ArgumentNullException(nameof(redisSetup));
            }

            AddNetCoreStackJobServer(services, setup);
            services.Configure(redisSetup);
            services.AddSingleton<IJobStorage, RedisStorage>();

            return services;
        }

        internal static void RegisterJob<TJob>(IServiceCollection services, Type type) where TJob : IJob
        {
            services.AddTransient(typeof(TJob), type);
        }
    }
}
