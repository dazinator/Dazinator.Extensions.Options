using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Dazinator.Extensions.Options.Tests.Integration
{
    using System.Threading;
    using Dazinator.Extensions.Options.Tests.Utils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    public class ConfigureUponRequestActionTests
    {

        [Theory]
        [InlineData("foo", "foo-v2", "bar", "bar-v2")] // each 
        [InlineData("foo", "foo", "foo", "foo", "foo")]
        public void ServiceCollection_ConfigureUponRequest_FromAction_InvokedOncePerNamedOptions(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
              {
                  services.AddOptions();
                  services.ConfigureUponRequest<TestOptions>().From((sp, name, options) =>
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
        public void OptionsBuilder_ConfigureUponRequest_ThrowsWhenInvokedOnNamedOptions(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
            {
                Assert.Throws<InvalidOperationException>(() => services.AddOptions<TestOptions>("foo") // note: we AddOptions with a "name" argument, and then attempt to use dynamic names, should get exception.
                            .ConfigureUponRequest().From((sp, name, options) =>
                            {
                                Interlocked.Increment(ref invocationCount);
                                options.Name = name;
                            }));

            });

            foreach (var name in names)
            {
                var options = optionsSnapshot.Get(name);
                Assert.NotNull(options);
                Assert.Null(options.Name);
            }
        }


        [Theory]
        [InlineData("foo", "foo-v2", "bar", "bar-v2")] // each 
        [InlineData("foo", "foo", "foo", "foo", "foo")]
        public void OptionsBuilde_ConfigureUponRequest_InvokedOncePerNamedOptions(params string[] names)
        {
            var invocationCount = 0;

            var optionsSnapshot = TestHelper.CreateTestSubject<IOptionsSnapshot<TestOptions>>(out var testServices, (services) =>
            {
                // Add named options configuration AFTER other configuration
                services.AddOptions<TestOptions>() // note: we AddOptions without any "name" argument, but the below is invoked style per each name requested.
                            .ConfigureUponRequest().From((sp, name, options) =>
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
        public void ConfigureUponRequest_Action_InvokedAgainAfterChangeTokenTriggered()
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
                services.ConfigureUponRequest<TestOptions>().From((sp, name, options) =>
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
