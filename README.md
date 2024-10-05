# Net.FracturedCode.Infisical

A C# client library for Infisical's API generated from its OpenApi spec.

This is an alternative to the
[first party Infisical SDK](https://github.com/Infisical/sdk) with modern C# and
more endpoints.

This is licensed under LGPL which may be more permissive than you realize.
Please read [this blurb](#license).

## Why

As compared to the first party SDK, this project has:

- **More features:** Only a few API endpoints are available via the Infisical 
  SDK. This library contains every endpoint.
- **No binary blobs:** This project is all C#.
- **Best practice:** Uses `HttpClient`.
- **Dependency injection:** Includes extensions for easy DI setup.

## Installation

```
dotnet add package Net.FracturedCode.Infisical
```

## Usage

Only helpers for [Infisical Universal Auth](https://infisical.com/docs/documentation/platform/identities/universal-auth)
have been implemented, but other authentication methods can be implemented
if you're clever enough. It all comes down to manipulating the `HttpClient`
before injecting it into the generated clients. You can use the project as 
reference for how to create your own.

### Config

Add this configuration:
```json
{
  "Infisical": {
    "Url": "https://app.infisical.com", // Your infisical server here, if applicable
    "ClientId": "insertMyClientId",
    "ClientSecret": "insertMyClientSecret"
  }
}
```

**Do not** simply plop your client secret in your `appsettings.json` and commit 
it! Read up on [.NET Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
if you're not sure what to do here. Often [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
or [environment variables](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#environment-variable-configuration-provider)
are a good idea for serving secrets to a C# project.

### Dependency injection

In your host setup:
```csharp
// the lambda that calls AddStandardResilience() is optional, it is used to draw
// attention to the fact that you can customize the HttpClient for the Infisical clients
builder.Services.AddInfisicalClients(b => b.AddStandardResilience());
```

### Consumption

TODO expand

## Build yourself

```
dotnet run --project ./Pipline/Pipeline.csproj
```

The artifact will be in `./Infisical/bin/Release`

## Cool things in this project

Allow me to nerd out.

### Nuget packaging
A great debugging and with
[sourcelink](https://github.com/dotnet/sourcelink),
deterministic build,
and the symbol package with all sources embedded published to nuget.

I had to do some research to figure out what all the options were really doing.
There is some comments in [Infisical.csproj](Infisical/Infisical.csproj)
that you may find useful.

### C# [Modular Pipelines](https://github.com/thomhurst/ModularPipelines)

Writing pipelines in a programming language has many benefits.

Here are the most important benefits in my view:

- Local pipeline debugging. This tightens the feedback loop and 
  therefore increases the pace of iteration.
  It is difficult to oversell how valuable that is.
  This is possible in some yaml pipeline frameworks but, ime, painful.
- The benefits to using the C# programming language as opposed to yaml:
  - strong typing
  - IDE integrations; syntax highlighting, autocomplete, debugging.
  - accessibility; some developers are often uncomfortable or not interested in 
    learning yaml pipelines and devops. C# provides something they're 
    familiar with.
- Focus your efforts in .NET instead of having to maintain another 
  technology in your repo.
- Helps avoid scripting languages. Bash syntax can be a pain. Ever ran a 
  pipeline 10 times until you got a string escape sequence or interpolation 
  just right? Yeah.

### Source generators

There is some metaprogramming going on to avoid reflection. Really just for 
fun, but this could be leveraged in the future for `NativeAot` compatability.

### .NET Aspire



## License

[LGPLv3](./LICENSE)

[LGPLv3 overview](https://choosealicense.com/licenses/lgpl-3.0/#)

[LGPLv3 more detailed information](https://fossa.com/blog/open-source-software-licenses-101-lgpl-license/)

TL;DR You can use this library as you see fit, but any modifications must be 
made publicly available and licensed under LGPLv3 or GPLv3 or later. 
Modifications do not include making your own C# DI extensions, for instance, as 
long as your code is using the unmodified library as an installed NuGet 
package.