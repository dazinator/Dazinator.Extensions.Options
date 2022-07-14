## Features

Provides additional capabilities for `Microsoft.Extensions.Options`.

### Configure dynamically named options

### The problem
The standard `Microsoft.Extensions.Options` functionality allows you to register "named" options in startup code.
However, you must know all of the names of the options that will be requested - ahead of time - i.e at the point of registration however.

e.g

```cs

 services.Configure<TestOptions>("bar", options =>
                {                   
                    
                });

```

This means you can then request options named "bar" and your registered delegates will take effect.

However if you request a name like "bar-v2" that wasn't registered in advance, then you'll have no way to intercept and configure this named options.

This library provides such a mechanism, via some overloads that allow you to supply an `Action` delegate that will be invoked, with the name supplied at runtime, allowing you to configure any names that appear dynamically whilst the app is running:

```cs
services.Configure<TestOptions>((sp, name, options) =>
                {
                  // name is the name that has been requested.
                    
                });

```

You can now request whatever named options you like at runtime, and the method above will be invoked to configure these instances.
This allows you to configure dynamically named options i.e without having to register all the possible names in advance.
