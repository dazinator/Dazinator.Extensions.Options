namespace Dazinator.Extensions.Options
{
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// The native IOptionsMonitor<T> implementation which is present by default via .AddOptions() relies on having all the IChangeTokenSources for named options
    /// registered up-front as services as they are injected into it's constructor and it is a singleton. 
    /// This implementation is different in that it allows new <see cref="IOptionsChangeTokenSource{TOptions}"/>'s to be introduced, allowing for new named options to be lazily introduced at runtime
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    public class DynamicOptionsMonitor<TOptions> :
        IOptionsMonitor<TOptions>,
        IDisposable
        where TOptions : class
    {
        private readonly IOptionsMonitorCache<TOptions> _cache;
        private readonly IOptionsFactory<TOptions> _factory;
        private List<IDisposable> _registrations = new List<IDisposable>();
        internal event Action<TOptions, string>? _onChange;
        private object _registrationsLock = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">The factory to use to create options.</param>
        /// <param name="sources">The sources used to listen for changes to the options instance.</param>
        /// <param name="cache">The cache used to store options.</param>
        public DynamicOptionsMonitor(IOptionsFactory<TOptions> factory, IEnumerable<IOptionsChangeTokenSource<TOptions>> sources, IOptionsMonitorCache<TOptions> cache)
        {
            _factory = factory;
            _cache = cache;



            // The default DI container uses arrays under the covers. Take advantage of this knowledge
            // by checking for an array and enumerate over that, so we don't need to allocate an enumerator.
            if (sources is IOptionsChangeTokenSource<TOptions>[] sourcesArray)
            {
                foreach (IOptionsChangeTokenSource<TOptions> source in sourcesArray)
                {
                    RegisterSource(source);
                }
            }
            else
            {
                foreach (IOptionsChangeTokenSource<TOptions> source in sources)
                {
                    RegisterSource(source);
                }
            }
        }

        public void RegisterSource(IOptionsChangeTokenSource<TOptions> source)
        {
            // the lock is used in case Dispose (which enumerates and clears the list) is called concurrently.
            lock (_registrationsLock)
            {
                IDisposable registration = ChangeToken.OnChange(
                    source.GetChangeToken,
                    InvokeChanged,
                    source.Name);

                _registrations.Add(registration);
            }

        }

        private void InvokeChanged(string? name)
        {
            name ??= Options.DefaultName;
            _cache.TryRemove(name);
            TOptions options = Get(name);
            _onChange?.Invoke(options, name);
        }

        /// <summary>
        /// The present value of the options, equivalent to <c>Get(Options.DefaultName)</c>.
        /// </summary>
        /// <exception cref="OptionsValidationException">One or more <see cref="IValidateOptions{TOptions}"/> return failed <see cref="ValidateOptionsResult"/> when validating the <typeparamref name="TOptions"/> instance been created.</exception>
        /// <exception cref="MissingMethodException">The <typeparamref name="TOptions"/> does not have a public parameterless constructor or <typeparamref name="TOptions"/> is <see langword="abstract"/>.</exception>
        public TOptions CurrentValue
        {
            get => Get(Options.DefaultName);
        }

        /// <summary>
        /// Returns a configured <typeparamref name="TOptions"/> instance with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the <typeparamref name="TOptions"/> instance, if <see langword="null"/> <see cref="Options.DefaultName"/> is used.</param>
        /// <returns>The <typeparamref name="TOptions"/> instance that matches the given <paramref name="name"/>.</returns>
        /// <exception cref="OptionsValidationException">One or more <see cref="IValidateOptions{TOptions}"/> return failed <see cref="ValidateOptionsResult"/> when validating the <typeparamref name="TOptions"/> instance been created.</exception>
        /// <exception cref="MissingMethodException">The <typeparamref name="TOptions"/> does not have a public parameterless constructor or <typeparamref name="TOptions"/> is <see langword="abstract"/>.</exception>
        public virtual TOptions Get(string? name)
        {
            // copying captured variables to locals avoids allocating a closure if we don't enter the if
            var localName = name ?? Options.DefaultName;
            var localFactory = _factory;
            return _cache.GetOrAdd(localName, () => localFactory.Create(localName));
        }

        /// <summary>
        /// Registers a listener to be called whenever <typeparamref name="TOptions"/> changes.
        /// </summary>
        /// <param name="listener">The action to be invoked when <typeparamref name="TOptions"/> has changed.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            _onChange += disposable.OnChange;
            return disposable;
        }

        /// <summary>
        /// Removes all change registration subscriptions.
        /// </summary>
        public void Dispose()
        {
            // the lock is used in case RegisterSource (which adds to the list) is called concurrently.
            lock (_registrationsLock)
            {
                // Remove all subscriptions to the change tokens
                foreach (IDisposable registration in _registrations)
                {
                    registration.Dispose();
                }

                _registrations.Clear();
            }

        }

        internal sealed class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TOptions, string> _listener;
            private readonly DynamicOptionsMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(DynamicOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnChange(TOptions options, string name) => _listener.Invoke(options, name);

            public void Dispose() => _monitor._onChange -= OnChange;
        }
    }

}
