namespace Microsoft.Extensions.Options
{
    using Dazinator.Extensions.Options;
    using Microsoft.Extensions.DependencyInjection;

    public static class ActionConfigureNamedOptionsExtensions
    {
        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first trime it is requested, using an Action delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<string, TOptions> configureAction)
            where TOptions : class
        {
            services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(null, (s, n, o) => configureAction?.Invoke(n, o)));
            services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return services;
        }

        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first trime it is requeste,  using an Action delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<IServiceProvider?, string, TOptions> configureAction)
           where TOptions : class
        {
            services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(sp, configureAction));
            services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return services;
        }

        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first time it is requeste,using an Action delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        /// <remarks>Convenience method to allow fluent api building when working with <see cref="OptionsBuilder<>"/>.</remarks>
        public static OptionsBuilder<TOptions> Configure<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, Action<string, TOptions> configureAction)
          where TOptions : class
        {

            optionsBuilder.Services.Configure(configureAction);
            return optionsBuilder;
        }

        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first time it is requeste,using an Action delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        /// <remarks>Convenience method to allow fluent api building when working with <see cref="OptionsBuilder<>"/>.</remarks>
        public static OptionsBuilder<TOptions> Configure<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, Action<IServiceProvider?, string, TOptions> configureAction)
          where TOptions : class
        {

            optionsBuilder.Services.Configure(configureAction);
            return optionsBuilder;
        }

    }
}
