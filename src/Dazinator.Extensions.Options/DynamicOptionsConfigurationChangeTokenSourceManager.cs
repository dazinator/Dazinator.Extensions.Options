namespace Microsoft.Extensions.Options
{
    using System.Collections.Generic;
    using Dazinator.Extensions.Options;
    using Microsoft.Extensions.Configuration;

    public class DynamicOptionsConfigurationChangeTokenSourceManager<TOptions>
     where TOptions : class
    {
        private readonly DynamicOptionsMonitor<TOptions> _monitor;
        private readonly Dictionary<string, List<IConfiguration>> _registeredOptionsNames = new Dictionary<string, List<IConfiguration>>();

        public DynamicOptionsConfigurationChangeTokenSourceManager(DynamicOptionsMonitor<TOptions> monitor)
        {
            _monitor = monitor;
        }
        public void EnsureRegistered(string name, IConfiguration config)
        {

            List<IConfiguration> boundConfigs;
            if (!_registeredOptionsNames.ContainsKey(name))
            {
                boundConfigs = new List<IConfiguration>();
                _registeredOptionsNames.Add(name, boundConfigs);
            }
            else
            {
                boundConfigs = _registeredOptionsNames[name];
            }

            // A named options could be bound to multiple config sections.
            // we only create and add a new IOptionsChangeTokenSource if the config being bound to is a new one.
            if (!boundConfigs.Contains(config))
            {
                var newChangeTokenSource = new ConfigurationChangeTokenSource<TOptions>(name, config);
                _monitor.RegisterSource(newChangeTokenSource);
            }

            return;
        }

    }
}
