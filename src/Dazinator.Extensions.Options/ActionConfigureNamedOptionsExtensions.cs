namespace Microsoft.Extensions.Options
{
    using Dazinator.Extensions.Options;
    using Microsoft.Extensions.DependencyInjection;  

    public static class ActionConfigureNamedOptionsExtensions
    {
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<string, TOptions> configureAction)
            where TOptions : class
        {
            services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(null, (s, n, o) => configureAction?.Invoke(n, o)));
            services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return services;
        }

        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<IServiceProvider?, string, TOptions> configureAction)
           where TOptions : class
        {
            services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(sp, configureAction));
            services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return services;
        }
    }
}
