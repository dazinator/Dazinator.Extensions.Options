namespace Dazinator.Extensions.Options.Tests.Integration
{
    using System.Threading;
    using Dazinator.Extensions.Options.Tests.Utils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    public class ActionBoundOptionsIntegrationTests
    {

        [Theory]
        [InlineData("foo", "foo-v2", "bar", "bar-v2")] // each 
        [InlineData("foo", "foo", "foo", "foo", "foo")]
        public void OptionsSnapshot_GetNamedOptions_InvokesConfigureActionOncePerName(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
              {
                  services.AddOptions();
                  services.Configure<TestOptions>((sp, name, options) =>
                  {
                      Interlocked.Increment(ref invocationCount);
                      options.Name = name;
                  });

              });

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
        public void Can_Configure_Using_OptionsBuilder(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
            {
                // Add named options configuration AFTER other configuration
                services.AddOptions<TestOptions>()
                            .Configure((sp, name, options) =>
                            {
                                Interlocked.Increment(ref invocationCount);
                                options.Name = name;
                            });

            });

            foreach (var name in names)
            {
                var options = optionsSnapshot.Get(name);
                Assert.NotNull(options);
                Assert.Equal(name, options.Name);
            }

            var distinctNamedCount = names.Distinct().Count();
            Assert.Equal(distinctNamedCount, invocationCount);
        }

        [Fact]
        public void OptionsMonitor_GetNamedOptions_AfterChangeTokenTriggered_InvokesConfigureActionAgain()
        {
            var invocationCount = 0;
            using var cancelTokenSourceToBeCancelled = new CancellationTokenSource();
            using var cancelTokenSourceNext = new CancellationTokenSource();

            var optionsMonitor = TestHelper.CreateTestSubject<IOptionsMonitor<TestOptions>>(out var testServices, (services) =>
            {
                services.AddOptions();
                // Add named options configuration AFTER other configuration
                var isFirstToken = true;
                var testChangeTokenSource = new TestChangeTokenSource("foo", () =>
                {
                    if (isFirstToken)
                    {
                        isFirstToken = false;
                        return cancelTokenSourceToBeCancelled.Token;
                    }
                    return cancelTokenSourceNext.Token;
                });

                services.AddSingleton<IOptionsChangeTokenSource<TestOptions>>(testChangeTokenSource);
                services.Configure<TestOptions>("bar", options =>
                {

                });
                services.Configure<TestOptions>((sp, name, options) =>
                {
                    Interlocked.Increment(ref invocationCount);
                    options.Name = name;
                });

            });

            var name = "foo";

            var options = optionsMonitor.Get(name);
            Assert.NotNull(options);
            Assert.Equal(name, options.Name);
            Assert.Equal(1, invocationCount);


            // trigger change token source
            cancelTokenSourceToBeCancelled.Cancel();
            options = optionsMonitor.Get(name);
            Assert.NotNull(options);
            Assert.Equal(name, options.Name);
            Assert.Equal(2, invocationCount);
        }
    }

    internal class TestOptions
    {
        public string? Name { get; set; }
    }

    public class TestChangeTokenSource : IOptionsChangeTokenSource<TestOptions>
    {
        private readonly Func<CancellationToken> _getNetCancellationToken;

        public TestChangeTokenSource(string optionsName, Func<CancellationToken> getNetCancellationToken)
        {
            _getNetCancellationToken = getNetCancellationToken;
            Name = optionsName;
        }

        public string Name { get; set; }

        public IChangeToken GetChangeToken()
        {
            var ct = _getNetCancellationToken.Invoke();
            return new CancellationChangeToken(ct);
        }
    }


}
