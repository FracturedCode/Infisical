# Net.FracturedCode.Infisical

A C# client library for Infisical's API generated from its OpenApi spec.

This is an alternative to the
[first party Infisical SDK](https://github.com/Infisical/sdk) with modern C# and
more endpoints.

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

TODO create the helpers

Only helpers for [Infisical Universal Auth](https://infisical.com/docs/documentation/platform/identities/universal-auth)
have been implemented, but other authentication methods can be implemented 
if you're clever enough. It all comes down to manipulating the `HttpClient` 
before injecting it into the generated clients.

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

In your host setup:
```csharp
builder.AddInfisicalClients();
```

## Build yourself

```
dotnet run --project ./Pipline/Pipeline.csproj
```

The artifact will be in `./Infisical/bin/Release`

## Project Structure
TODO
### Pipeline

### AppHost

### Infisical

### Tests

## License

[LGPLv3](./LICENSE)

[LGPLv3 overview](https://choosealicense.com/licenses/lgpl-3.0/#)

[LGPLv3 more detailed information](https://fossa.com/blog/open-source-software-licenses-101-lgpl-license/)

TL;DR You can use this library as you see fit, but any modifications must be 
made publicly available and licensed under LGPLv3 or GPLv3 or later. 
Modifications do not include making your own C# DI extensions, for instance, as 
long as your code is using the unmodified library as an installed NuGet 
package.