namespace Microsoft.Extensions.Options
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Dazinator.Extensions.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ConfigureNamedOptionsExtensions
    {
        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first trime it is requested, using an Action delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureUponRequest<TOptions>(this IServiceCollection services, Action<string, TOptions> configureAction)
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
        public static IServiceCollection ConfigureUponRequest<TOptions>(this IServiceCollection services, Action<IServiceProvider?, string, TOptions> configureAction)
           where TOptions : class
        {
            services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(sp, configureAction));
            services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return services;
        }

        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first trime it is requeste, using a binding to IConfiguration.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureUponRequest<TOptions>(this IServiceCollection services, Func<string, IConfiguration> getConfig, Action<string, BinderOptions>? configureBinder = null)
           where TOptions : class
        {
            ConfigureUponRequest<TOptions>(services, (sp, name, options) =>
            {
                var configToBind = getConfig(name);
                Action<BinderOptions>? configureBinding = configureBinder == null ? null : (bindingOptions) => configureBinder?.Invoke(name, bindingOptions);
                configToBind.Bind(options, configureBinding);

                var changeTokenSourceManager = sp.GetRequiredService<DynamicOptionsConfigurationChangeTokenSourceManager<TOptions>>();
                changeTokenSourceManager.EnsureRegistered(name, configToBind);
            });

            services.AddSingleton<DynamicOptionsMonitor<TOptions>>();

            services.AddSingleton<IOptionsMonitor<TOptions>>(sp => sp.GetRequiredService<DynamicOptionsMonitor<TOptions>>());
            //  services.AddSingleton<IOptionsMonitor<TOptions>, DynamicOptionsMonitor<TOptions>>();
            services.AddSingleton<DynamicOptionsConfigurationChangeTokenSourceManager<TOptions>>();

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
        public static OptionsBuilder<TOptions> ConfigureUponRequest<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, Action<string, TOptions> configureAction)
          where TOptions : class
        {
            optionsBuilder.Services.ConfigureUponRequest(configureAction);
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
        public static OptionsBuilder<TOptions> ConfigureUponRequest<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, Action<IServiceProvider?, string, TOptions> configureAction)
          where TOptions : class
        {
            optionsBuilder.Services.ConfigureUponRequest(configureAction);
            return optionsBuilder;
        }


        /// <summary>
        /// Configure a named options instance, lazily, i.e. the first time it is requeste,using IConfiguration.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        /// <remarks>Convenience method to allow fluent api building when working with <see cref="OptionsBuilder<>"/>.</remarks>
        public static OptionsBuilder<TOptions> BindConfigurationUponRequest<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, Func<string, IConfiguration> getConfig, Action<string, BinderOptions>? configureBinder = null)
          where TOptions : class
        {
            optionsBuilder.Services.ConfigureUponRequest<TOptions>(getConfig, configureBinder);
            return optionsBuilder;
        }

    }
}
