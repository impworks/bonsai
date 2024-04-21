# Bonsai

A family wiki and photoalbum engine.

### [Live demo](https://bonsai.kirillorlov.pro)

## Features

* Pages with Markdown text
* Media files: photos, videos (PDF documents will be supported later)
* Person tags on photos
* Relations: validation, inferrence
* Fact storage (birthday, gender, blood type, languages, hobbies, etc.)
* Access control: editor, reader and guest roles
* Changesets: browse changes to any page/media, see diffs, easily revert if necessary

## Screenshots

#### Front-end:

<a href="https://user-images.githubusercontent.com/604496/46574247-037d4f00-c9a9-11e8-8585-0d574dda2600.png"><img src="https://user-images.githubusercontent.com/604496/46574252-1859e280-c9a9-11e8-821f-daeaaac7de3f.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574259-2c054900-c9a9-11e8-8ecc-ca542053f665.png"><img src="https://user-images.githubusercontent.com/604496/46574288-9a4a0b80-c9a9-11e8-8373-2a7d3e00289c.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574262-31629380-c9a9-11e8-9ea6-18fbe63f239f.png"><img src="https://user-images.githubusercontent.com/604496/46574291-9f0ebf80-c9a9-11e8-8656-8a54dd2f2be7.png" /></a>

#### Admin panel:

<a href="https://user-images.githubusercontent.com/604496/46574266-3f181900-c9a9-11e8-828d-9d9a5db25acb.png"><img src="https://user-images.githubusercontent.com/604496/46574292-a209b000-c9a9-11e8-8193-cd99fc1f5f91.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574268-43443680-c9a9-11e8-974f-f8a60fbeaa74.png"><img src="https://user-images.githubusercontent.com/604496/46574297-a504a080-c9a9-11e8-8612-d3e5cd1592a4.png" /></a>

## Installation via Docker
1. Download the [docker-compose.lite.yml](docker-compose.lite.yml).

2. Set the locale to English:

   Modify `docker-compose.lite.yml`:
   Change `Locale=ru-RU` to `Locale=en-US`.

3. _Optional_: 

    Configure your Bonsai instance to use HTTPS for better security.

    You can use any vendor-specific options: e.g. Cloudflare Tunnel (free, fairly easy to configure, but requires a domain), Synology DDNS (free, very easy, requires a Synology NAS device), etc.

    This requires a bit of work, so if you just want to give Bonsai a quick spin - feel free to skip or postpone this one and the next.

4. _Optional_:

    Create a [Google Authorization app](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0) (or Yandex / VK.com).

    Modify `docker-compose.lite.yml`:

    * Save Google authorization credentials to `Auth__Google__ClientId` and `Auth__Google__ClientSecret` config properties
    * Set `Auth__AllowPasswordAuth=false` if you want to disable the less-secure password authorization

5. Bring everything up using `docker compose`:
   ```
   docker-compose -f docker-compose.lite.yml up -d
   ```
6. After everything is brought up Bonsai will listen on port `8080`.

## Development (on Windows)

For development, you will need the following:

* [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0): the main runtime for Bonsai

1. Install [NodeJS 14](https://nodejs.org/en/)
2. Download [ffmpeg shared binaries](https://www.ffmpeg.org/download.html) for your system and extract the archive's contents into `External/ffmpeg` folder in the solution root (must contain both `ffmpeg` and `ffprobe` executables).
3. Create a file called `appsettings.Development.json`, add the connection string:

    ```
    {
      "ConnectionStrings": {
        "EmbeddedDatabase": "Data Source=App_Data/bonsai.db",
        "UseEmbeddedDatabase": true
      },
      "Auth": {
        "AllowPasswordAuth": true
      } 
    }
    ```

5. _Optional, but suggested_:

    Create a [Google Authorization App](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0) (or Yandex / VK.com).

    Add the retrieved authorization credentials to the `appsettings.Development.json` and set `AllowPasswordAuth` to `false`:

    ```
    {
        "Auth": {
            "AllowPasswordAuth": false,
            "Google": {
              "ClientId": "<...>",
              "ClientSecret": "<...>" 
            },
            "Yandex": {
              "ClientId": "<...>",
              "ClientSecret": "<...>" 
            },
            "Vkontakte": {
              "ClientId": "<...>",
              "ClientSecret": "<...>" 
            }
        }
    }
    ```
    
6. Create the database:

    ```
    dotnet ef database update
    ```
7. Build the styles and scripts:

    ```
    npm install
    npm run build
    ```
8. Run the app (as Visual Studio project or using `dotnet run`).

## Security considerations

### Data backup

If you value the data that you store in Bonsai, make sure that you **SET UP BACKUPS**.

You will need to back up the following:

* Database (approximately tens of megabytes)
* Uploaded media in `wwwroot/media` (may contain gigabytes of data)

There are many options available, free and paid: uploading to a cloud storage, copying to external drives, etc. Please consider your usage/budget and select a combination that works best for you.

### Authorization methods

Bonsai features two authorization methods: OAuth and password authorization.

OAuth is the preferred method: it's easier to use for end users, more secure and versatile. **Please use the OAuth method if you can!**
For OAuth, you will need to create an authorization app on [Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0), [Vkontakte](https://vk.com/editapp?act=create), or [Yandex](https://oauth.yandex.ru/client/new) as described in the installation steps.
You can enable multiple authorization apps at the same time: users will pick the one they prefer.

As a fallback, you can also create an account with classic login/password authorization. It can be used for two purposes:

* Playing around with Bonsai (easier to set up: no auth app and https configuration required)
* Giving access to elder family members who don't have a social network account

Please keep the following facts in mind:

* Any user account can only have one authorization method: password, or Facebook, or Google, etc.
* It is not possible to change the authorization type for an account once it has been created.
* Password-based accounts can be locked out if there are too many consecutive failed login attempts.
* Account password can only be reset by an administrator manually. If you only have one admin account, it is password-based, and the password is lost - there's no way to regain access besides direct database manipulation!