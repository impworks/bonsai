# Bonsai

A family wiki engine (in Russian).

### Features

* Pages with Markdown text
* Media files: photos, video, PDF documents
* Person tags on photos
* Relations support: validation, inferrence
* Fact storage
* Access control

### Installation

1. Install PostgreSQL server (9.6+)
2. Install ElasticSearch (5.6.x, 6.0 is not supported yet)
3. Install [Russian Morphology](https://github.com/imotov/elasticsearch-analysis-morphology) for ElasticSearch.
4. Download [ffmpeg shared binaries](https://ffmpeg.zeranoe.com/builds/) for your system and extract the archive's contents into `External/ffmpeg` folder in the solution root (must contain both `ffmpeg` and `ffprobe` executables).
5. Create a [Facebook Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-2.1&tabs=aspnetcore2x)
6. Create a [Google Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins) or comment out the `UseGoogle` block in `Startup.cs`
7. Add secrets via PMC:

    ```bash
    dotnet user-secrets set Auth:Facebook:AppId <id>
    dotnet user-secrets set Auth:Facebook:AppSecret <secret>

    dotnet user-secrets set Auth:Google:ClientId <id>
    dotnet user-secrets set Auth:Google:ClientSecret <secret>
    ```
8. Set connection strings in `appconfig.json`
9. Create the database:

    ```
    dotnet ef database update
    ```
10. Run