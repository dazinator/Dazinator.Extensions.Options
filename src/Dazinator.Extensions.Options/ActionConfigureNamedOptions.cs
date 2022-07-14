namespace Dazinator.Extensions.Options
{
    using Microsoft.Extensions.Options;

    public class ActionConfigureNamedOptions<TOptions> : IConfigureNamedOptions<TOptions>
        where TOptions : class
    {
        private readonly ConfigureActionOptions<TOptions> _configureActionOptions;

        public ActionConfigureNamedOptions(ConfigureActionOptions<TOptions> configureActionOptions) => _configureActionOptions = configureActionOptions;

        public void Configure(string name, TOptions options) => _configureActionOptions.Invoke(name, options);

        // This won't be called, but is required for the interface
        public void Configure(TOptions options) => Configure(Options.DefaultName, options);
    }

}
