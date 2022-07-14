namespace Dazinator.Extensions.Options.Tests.Utils
{
    using Microsoft.Extensions.DependencyInjection;

    public static class TestHelper
    {
        public static TTestSubject CreateTestSubject<TTestSubject>(out IServiceProvider testServices,
            Action<IServiceCollection>? configureTestServices = null)
            where TTestSubject : class
        {
            IServiceCollection services = new ServiceCollection();
            configureTestServices?.Invoke(services);
            var sp = services.BuildServiceProvider();
            testServices = sp;
            var factory = sp.GetRequiredService<TTestSubject>();
            return factory;
        }

    }
}
