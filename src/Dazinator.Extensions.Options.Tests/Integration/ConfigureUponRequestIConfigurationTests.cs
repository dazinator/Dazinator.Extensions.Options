namespace Dazinator.Extensions.Options.Tests.Integration
{
    using System.Threading;
    using Dazinator.Extensions.Options.Tests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class ConfigureUponRequestIConfigurationTests
    {

        [Theory]
        [InlineData("foo", "foo-v2", "bar", "bar-v2")] // each 
        [InlineData("foo", "foo", "foo", "foo", "foo")]
        public void ServiceCollection_ConfigureUponRequest_FromIConfiguration_InvokedOncePerNamedOptions(params string[] names)
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

                  services.ConfigureUponRequest<TestOptions>().From((name) =>
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
        public void OptionsBuilder_ConfigureUponRequest_InvokedOncePerNamedOptions(params string[] names)
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
                        .ConfigureUponRequest<TestOptions>().From((name) =>
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
