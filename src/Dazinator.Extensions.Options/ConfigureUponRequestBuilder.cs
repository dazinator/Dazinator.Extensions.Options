namespace Microsoft.Extensions.Options
{
    using Dazinator.Extensions.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Exists for syntactic sugar purposes. An instance of this can be exposed and allow a consumer to configure the options in a focused way, whilst constraining the api's to only those that make sense for this library, i.e if using AddOptions() theere is a much wider surface area of possibilities for confiugring options,
    /// this alternative only exposes configure methods that support configuring the options instannce at request time as opposed to registration time.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TInner"></typeparam>
    public class ConfigureUponRequestBuilder<TOptions, TInner>
         where TOptions : class
    {
        private readonly IServiceCollection _services;
        private readonly TInner _inner;

        public ConfigureUponRequestBuilder(IServiceCollection services, TInner inner)
        {
            _services = services;
            _inner = inner;
        }


        public TInner From(Action<string, TOptions> configureAction)
        {
            _services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(null, (s, n, o) => configureAction?.Invoke(n, o)));
            _services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return _inner;
        }

        public TInner From(Action<IServiceProvider?, string, TOptions> configureAction)
        {
            _services.AddSingleton(sp => new ConfigureActionOptions<TOptions>(sp, configureAction));
            _services.AddSingleton<IConfigureOptions<TOptions>, ActionConfigureNamedOptions<TOptions>>();
            return _inner;
        }

        public TInner From(Func<string, IConfiguration> getConfig, Action<string, BinderOptions>? configureBinder = null)
        {
            From((sp, name, options) =>
            {
                var configToBind = getConfig(name);
                Action<BinderOptions>? configureBinding = configureBinder == null ? null : (bindingOptions) => configureBinder?.Invoke(name, bindingOptions);
                configToBind.Bind(options, configureBinding);

                var changeTokenSourceManager = sp.GetRequiredService<DynamicOptionsConfigurationChangeTokenSourceManager<TOptions>>();
                changeTokenSourceManager.EnsureRegistered(name, configToBind);
            });

            _services.AddSingleton<DynamicOptionsMonitor<TOptions>>();
            _services.AddSingleton<IOptionsMonitor<TOptions>>(sp => sp.GetRequiredService<DynamicOptionsMonitor<TOptions>>());
            _services.AddSingleton<DynamicOptionsConfigurationChangeTokenSourceManager<TOptions>>();
            return _inner;

        }

    }

}
