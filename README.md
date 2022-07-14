## Features

Provides additional capabilities for `Microsoft.Extensions.Options`.

### Configure dynamically named options

### The problem
The standard `Microsoft.Extensions.Options` functionality allows you to register "named" options in startup code.
However, you must know all of the names of the options that will be requested - ahead of time - i.e at the point of registration, e.g:

```cs
 services.Configure<TestOptions>("foo", options =>
                {                   
                    
                });

```

This means you can then request options with this name at runtime, and the configuration delegates you have supplied will configure the instance accordingly - great.
For many this will be good enough.

However in an advanced scenario, suppose you wish to supply dynamic names.
For example, at runtime you need to request a new name like "bar-v2" that wasn't registered in advance.
In this case you'd like to have a way to intercept and configure this new named options.

This library provides such a mechanism, via some overloads that allow you to supply an `Action` delegate that will be invoked, with the name supplied as an argument:

```cs
services.Configure<TestOptions>((sp, name, options) =>
                {
                  // name is the name that has been requested.
                    
                });

```

You can now request whatever named options you like at runtime, and the method above will be invoked to configure these instances how you please.

The primary use case for this feature is `IHttpClientFactory`, which uses named `HttpClientFactoryOptions` behind the scenes.
By supplying different names you can kind of "cache bust" and force a new HttpClient to be built that uses the latest configuration.

The default `IHttpClientFactory` provided by Microsoft, builds and pools handlers for a given `name`d http client, and uses the `HttpClientFactoryOptions` registered with the same name to do this.
So if you've configured a http client named "foo" and you later use that http client via the factory - it will be built according to the 'HttpClientFactoryOptions` with the same name - if you want to change this confiugration, you can't.
Therefore if you want reconfigure a named http client at runtime (for example, change it's configured `BaseAddress`, or `Handlers`), the simplest way to acheive this is to request the http client with a different name - i.e perhaps with a version identifier appended which can be incremented.
This forces the `IHttpClientFactory` to miss its cache, and build a new http client which can be based on the latest configuration.
```

services.Configure<HttpClientFactoryOptions>((sp, name, options) =>
                {
                  // name is the httpclient name that has been requested.
                  var httpClientName = SplitOnDashAndTakeFirstSegment(name);
                    // todo: load latest config for httpClientName e.g "foo".
                });


// ...
IHttpClientFactory httpClientFactory = GetHttpClientFactory();
var fooClientv1 = httpClientFactory.CreateClient("foo-v1");

// later you update the config / settings for "foo-v1" so that the latest config settings are now found with the name "foo-v2"
var fooClientv2 = httpClientFactory.CreateClient("foo-v2");

```

