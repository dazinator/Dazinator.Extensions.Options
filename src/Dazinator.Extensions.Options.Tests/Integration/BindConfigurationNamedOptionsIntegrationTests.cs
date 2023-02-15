namespace Dazinator.Extensions.Options.Tests.Integration
{
    using System.Threading;
    using Dazinator.Extensions.Options.Tests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class ConfigurationBoundOptionsIntegrationTests
    {

        [Theory]
        [InlineData("foo", "foo-v2", "bar", "bar-v2")] // each 
        [InlineData("foo", "foo", "foo", "foo", "foo")]
        public void OptionsSnapshot_GetNamedOptions_ReturnsOptionsBoundFromConfigDynamically(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
              {
                  // Add named options configuration AFTER other configuration
                  services.AddOptions();

                  var configBuilder = new ConfigurationBuilder();

                  var inMemoryConfigValues = new Dictionary<string, string>();
                  foreach (var name in names)
                  {
                      inMemoryConfigValues.TryAdd($"{name}:{nameof(TestOptions.Name)}", name);
                  }

                  configBuilder.AddInMemoryCollection(inMemoryConfigValues);
                  IConfiguration config = configBuilder.Build();

                  services.Configure<TestOptions>((name) =>
                  {
                      // dynamically bind named options to config section.
                      Interlocked.Increment(ref invocationCount);
                      return config.GetSection(name);
                  });

              });

            // var names = new List<string>() { "foo", "foo-v2", "bar", "bar-v2" };
            foreach (var name in names)
            {
                var options = optionsSnapshot.Get(name);
                Assert.NotNull(options);
                Assert.Equal(name, options.Name);
            }

            var distinctNamedCount = names.Distinct().Count();
            Assert.Equal(distinctNamedCount, invocationCount);
        }

        [Theory]
        [InlineData("foo", "foo-v2", "bar", "bar-v2")] // each 
        [InlineData("foo", "foo", "foo", "foo", "foo")]
        public void OptionsSnapshot_GetNamedOptions_WhenConfiguredWithOptionsMonitor_ReturnsOptionsBoundFromConfigDynamically(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
            {

                var configBuilder = new ConfigurationBuilder();

                var inMemoryConfigValues = new Dictionary<string, string>();
                foreach (var name in names)
                {
                    inMemoryConfigValues.TryAdd($"{name}:{nameof(TestOptions.Name)}", name);
                }

                configBuilder.AddInMemoryCollection(inMemoryConfigValues);
                IConfiguration config = configBuilder.Build();

                // Add named options configuration AFTER other configuration
                services.AddOptions<TestOptions>()
                        .Configure<TestOptions>((name) =>
                        {
                            // dynamically bind named options to config section based on options name requested at runtime.
                            Interlocked.Increment(ref invocationCount);
                            return config.GetSection(name);
                        });


            });

            // var names = new List<string>() { "foo", "foo-v2", "bar", "bar-v2" };
            foreach (var name in names)
            {
                var options = optionsSnapshot.Get(name);
                Assert.NotNull(options);
                Assert.Equal(name, options.Name);
            }

            var distinctNamedCount = names.Distinct().Count();
            Assert.Equal(distinctNamedCount, invocationCount);
        }

    }


}
