namespace Dazinator.Extensions.Options
{
    public class ConfigureActionOptions<TOptions>
        where TOptions : class
    {
        protected Action<IServiceProvider?, string, TOptions> ConfigureAction { get; }

        public IServiceProvider? ServiceProvider { get; set; }

        public ConfigureActionOptions(IServiceProvider? serviceProvider, Action<IServiceProvider?, string, TOptions> configureAction)
        {
            ServiceProvider = serviceProvider;
            ConfigureAction = configureAction;
        }

        public void Invoke(string name, TOptions options) => ConfigureAction?.Invoke(ServiceProvider, name, options);
    }

}
