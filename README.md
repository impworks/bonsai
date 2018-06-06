# Bonsai

A family wiki engine (in Russian).

### Features

* Pages with Markdown text
* Media files: photos, video, 360 spheres (?)
* Person tags on photos
* Relations support: validation, inferrence
* Fact storage
* Access control

### Installation

1. Install PostgreSQL server (9.6+)
2. Install ElasticSearch (5.6.x, 6.0 is not supported yet)
3. Install [Russian Morphology](https://github.com/imotov/elasticsearch-analysis-morphology) for ElasticSearch
4. Create a [Facebook Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-2.1&tabs=aspnetcore2x)
5. Create a [Google Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins) or comment out the `UseGoogle` block in `Startup.cs`
6. Add secrets via PMC:

    ```bash
    dotnet user-secrets set Auth:Facebook:AppId <id>
    dotnet user-secrets set Auth:Facebook:AppSecret <secret>

    dotnet user-secrets set Auth:Google:ClientId <id>
    dotnet user-secrets set Auth:Google:ClientSecret <secret>
    ```
7. Set connection strings in `appconfig.json`
8. Create the database:

    ```
    dotnet ef database update
    ```
9. Run