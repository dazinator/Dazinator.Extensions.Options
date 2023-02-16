namespace Microsoft.Extensions.Options
{
    using Microsoft.Extensions.DependencyInjection;

    public static class ConfigureNamedOptionsExtensions
    {

        /// <summary>
        /// Configure an options instance with a dynamic name provided at point of request.
        /// This allows new options names to be requested at runtime without having to specify options names at point of registration on startup.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static ConfigureUponRequestBuilder<TOptions, IServiceCollection> ConfigureUponRequest<TOptions>(this IServiceCollection services)
            where TOptions : class
        {
            var builder = new ConfigureUponRequestBuilder<TOptions, IServiceCollection>(services, services);
            return builder;

        }

        /// <summary>
        /// Configure an options instance with a dynamic name provided at point of request.
        /// This allows new options names to be requested at runtime without having to specify options names at point of registration on startup.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        /// <remarks>Convenience method to allow fluent api building when working with <see cref="OptionsBuilder<>"/>.</remarks>
        public static ConfigureUponRequestBuilder<TOptions, OptionsBuilder<TOptions>> ConfigureUponRequest<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
          where TOptions : class
        {
            if (!string.IsNullOrEmpty(optionsBuilder.Name))
            {
                throw new InvalidOperationException("Should not call ConfigureUponRequest on an OptionsBuilder that is not for the default options instance. This api is intended to set up dynamic named options so configuring it for a explicitly named options instance does not make sense.");
            }
            var builder = new ConfigureUponRequestBuilder<TOptions, OptionsBuilder<TOptions>>(optionsBuilder.Services, optionsBuilder);
            return builder;
        }
    }
}
