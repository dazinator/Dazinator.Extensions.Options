## Features

Provides additional capabilities for `Microsoft.Extensions.Options`.

### Configure dynamically named options

Additional `Configure` api's are provided for configuring `Microsoft.Extensions.Options` `Options` so that you can dynamically configure an options when it is first requested, rather than at the point of registration.

e.g the "out of the box" behaviour is to register various named options like this, specifying the name at point of registration:-

```cs
 services.Configure<TestOptions>("foo", options =>
                {                   
                    
                });

```

For many this will be good enough and you do not need this library.

However in an advanced scenario, suppose you wish to request new names at runtime.
In this case you'd like to have a way to intercept and configure new named options at the point they are requested for the first time.

This library provides such a mechanism, via some overloads for `Configure` that allow you to supply familiar constructs such as an `Action` to confiugre the options instance, or an `IConfiguration` at the point of request as opposed to registration, where the options name is provided to you as an argument. 

```cs
services.Configure<TestOptions>((sp, name, options) =>
                {
                  // name is the name that has been requested.
                    
                });

```

You can now request whatever named options you like at runtime, and the method above will be invoked to configure these instances how you please.


## IConfiguration example

An additional `Configure` api is provided so that you can dynamically configure options from IConfiguration, i.e based on the options name requested at runtime. 

```cs

   IConfiguration config = GetConfiguration();

   services.Configure<TestOptions>((name) =>
   {
       // dynamically configure the options by returning an IConfiguration it should be bound form here,
       // you could select a config section based on the options name requested at runtime for exampl.
       return config.GetSection(name);
   }
```

## Use Cases (Background Info)

The primary use case for dynamic configuration of options is `IHttpClientFactory` scenarios.
`IHttpClientFactory` uses named `HttpClientFactoryOptions` behind the scenes to configure HttpClients.

By supplying different names you can kind of "cache bust" and force a new HttpClient to be built that will apply new configuration.

The default `IHttpClientFactory` provided by Microsoft, builds and pools handlers for a given `name`d http client, and uses the `HttpClientFactoryOptions` registered with the same name to do this.
So if you've configured a http client named "foo" and you later use that http client via the factory - it will be built according to the `HttpClientFactoryOptions` with the same name - if you want to change this confiugration, you can't.
Therefore if you want reconfigure a named http client at runtime (for example, change it's configured `BaseAddress`, or `Handlers`), the simplest way to acheive this is to request the http client with a different name - i.e perhaps with a version identifier appended which can be incremented.
This forces the `IHttpClientFactory` to miss its cache, and build a new http client which can be based on the latest configuration.

```cs
services.Configure<HttpClientFactoryOptions>((sp, name, options) =>
                {
                  // name is the httpclient name that has been requested.
                  var httpClientName = SplitOnDashAndTakeFirstSegment(name);
                    // todo: load latest config for httpClientName e.g "foo".
                });


// Note: the "-v1", "-v2" acts as a kind of "cache busting" mechanism, to ensure that IHttpClientFactory will build a new http client
// in conjunction with the Configure method above, that ensures we can still configure the `HttpClientFactoryOptions` based on the latest settings we have for this client.
IHttpClientFactory httpClientFactory = GetHttpClientFactory();
var fooClientv1 = httpClientFactory.CreateClient("foo-v1");

// later you update the config / settings for "foo-v1" so that the latest config settings are now found with the name "foo-v2"
var fooClientv2 = httpClientFactory.CreateClient("foo-v2");

```

