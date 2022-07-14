## Purpose

## Dynamic Named HttpClients

The typical usage pattern for `IHttpClientFactory` is to register your named `HttpClient`s on startup, and this means configuring them. 
This means the configuration such as the handlers, and the base address, cannot then be easily changed AFTER the http client has subsequently been used in the app.

This library provides a simple mechanism for you to supply new http clients at runtime.

To do this, you just request a http client from the `IHttpClientFactory` using a new name, and also supply a delegate that can be used to configure the http client if it hasn't been intiialised previously.
Once a http client has been built with the specific name, it can't be reconfigured. Instead you have to supply a new name.


Todo: example

cs
```

```
