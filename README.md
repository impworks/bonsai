# Bonsai

A family wiki engine (in Russian).

### Features

* Pages with Markdown text
* Media files: photos, video, PDF documents
* Person tags on photos
* Relations support: validation, inferrence
* Fact storage
* Access control

### Screenshots

#### Front-end:

<a href="https://user-images.githubusercontent.com/604496/46574247-037d4f00-c9a9-11e8-8585-0d574dda2600.png"><img src="https://user-images.githubusercontent.com/604496/46574252-1859e280-c9a9-11e8-821f-daeaaac7de3f.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574259-2c054900-c9a9-11e8-8ecc-ca542053f665.png"><img src="https://user-images.githubusercontent.com/604496/46574288-9a4a0b80-c9a9-11e8-8373-2a7d3e00289c.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574262-31629380-c9a9-11e8-9ea6-18fbe63f239f.png"><img src="https://user-images.githubusercontent.com/604496/46574291-9f0ebf80-c9a9-11e8-8656-8a54dd2f2be7.png" /></a>

#### Admin panel:

<a href="https://user-images.githubusercontent.com/604496/46574266-3f181900-c9a9-11e8-828d-9d9a5db25acb.png"><img src="https://user-images.githubusercontent.com/604496/46574292-a209b000-c9a9-11e8-8193-cd99fc1f5f91.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574268-43443680-c9a9-11e8-974f-f8a60fbeaa74.png"><img src="https://user-images.githubusercontent.com/604496/46574297-a504a080-c9a9-11e8-8612-d3e5cd1592a4.png" /></a>

### Installation

1. Install [PostgreSQL server](https://www.openscg.com/bigsql/postgresql/installers.jsp/) (9.6+)
2. Install [ElasticSearch 5.6.x](https://www.elastic.co/downloads/past-releases) (6.0 is not supported yet)
3. Install [Russian Morphology](https://github.com/imotov/elasticsearch-analysis-morphology) for ElasticSearch.
4. Download [ffmpeg shared binaries](https://ffmpeg.zeranoe.com/builds/) for your system and extract the archive's contents into `External/ffmpeg` folder in the solution root (must contain both `ffmpeg` and `ffprobe` executables).
5. Create a [Facebook Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-2.1&tabs=aspnetcore2x)
6. Create a [Google Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins) or comment out the `UseGoogle` block in `Startup.cs`
7. Create a file called `appsettings.Development.json` and save the auth secrets/connection strings/etc in it:

    ```
    {
      "ConnectionStrings": {
        "Database": "Server=127.0.0.1;Port=5432;Database=bonsai;User Id=<login>;Password=<password>;Persist Security Info=true"
      },
      "Auth": {
        "Facebook": {
          "AppId": "...",
          "AppSecret": "..." 
        },
        "Google": {
          "ClientId": "...",
          "ClientSecret": "..." 
        } 
      } 
    }
    ```
8. Create the database:

    ```
    dotnet ef database update
    ```
9. Run
