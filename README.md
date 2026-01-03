# Bonsai

Фамильный вики-движок и фотоальбом на нескольких языках.

### [Демо: попробовать в действии](https://bonsai.kirillorlov.pro)

## Возможности

* Страницы с разметкой Markdown
* Медиа-файлы: фото, видео, PDF
* Отметки людей на фото
* Родственные связи (с проверками и автоматическим выводом)
* Факты (дата рождения, пол, группа крови, владение языками, хобби, и так далее)
* Контроль доступа по ролям: администратор, редактор, читатель, гость
* История правок: для любой страницы или медиа-файла хранится история с diff'ами и возможностью отката к предыдущей версии
* Поддержка ИИ-ассистентов через <a href="https://github.com/impworks/bonsai/wiki/MCP%E2%80%90%D1%81%D0%B5%D1%80%D0%B2%D0%B5%D1%80">MCP-сервер</a>

## Скриншоты

#### Публичные страницы:

<a href="https://github.com/impworks/bonsai/assets/604496/151719f8-3396-4d6a-b24a-ba0a525cf04b"><img src="https://github.com/impworks/bonsai/assets/604496/921baf97-ad71-4c7f-b9b5-2d0c9d8f069f" /></a>
<a href="https://github.com/impworks/bonsai/assets/604496/da6794b1-a8ca-4128-939b-7b55ea60e5b1"><img src="https://github.com/impworks/bonsai/assets/604496/93defa8b-c1e3-4e57-a59f-492643da54b4" /></a>
<a href="https://github.com/impworks/bonsai/assets/604496/56b90464-9055-4028-a060-84f490015894"><img src="https://github.com/impworks/bonsai/assets/604496/cc45cdd2-0fdb-42c2-96ba-14d7fbaf637f" /></a>
<a href="https://github.com/impworks/bonsai/assets/604496/631f4ab8-cde5-4359-b0f5-3bc47edb3856"><img src="https://github.com/impworks/bonsai/assets/604496/0b31dd07-c604-46da-a2d0-9c0b8cc57260" /></a>

#### Панель администратора:

<a href="https://github.com/impworks/bonsai/assets/604496/dad5420d-14bc-4fa9-93f4-75b530d8ee69"><img src="https://github.com/impworks/bonsai/assets/604496/0d337339-8116-4b4a-9640-33d66778e827" /></a>
<a href="https://github.com/impworks/bonsai/assets/604496/8423d10f-79fd-45bf-8cd5-bcc69cd11607"><img src="https://github.com/impworks/bonsai/assets/604496/6dce3e19-0f58-422e-a4d7-a1ddc423ba1d" /></a>
<a href="https://github.com/impworks/bonsai/assets/604496/9b7e8166-38f9-48fc-a529-4bd99a1a2a35"><img src="https://github.com/impworks/bonsai/assets/604496/29ded387-8b30-48c9-9f31-0d3a2151f3c9" /></a>

## Установка с помощью Docker
1. Скачайте файл [docker-compose.lite.yml](docker-compose.lite.yml).

2. _Опционально_: 

    Настройте доступ по HTTPS для дополнительной безопасности.

    Вы можете использовать любые доступные сервисы, например [Cloudflare Tunnel](https://github.com/impworks/bonsai/wiki/%D0%9D%D0%B0%D1%81%D1%82%D1%80%D0%BE%D0%B9%D0%BA%D0%B0-%D1%82%D1%83%D0%BD%D0%BD%D0%B5%D0%BB%D1%8F-Cloudflare) (бесплатно, несложно, нужен домен), Synology DDNS (бесплатно, просто, нужен Synology NAS), или другие.

    Это трудоемкий шаг, поэтому если вы просто хотите попробовать Bonsai своими руками локально - его и следующий можно пропустить или отложить.

3. _Опционально_:

    Создайте [приложение авторизации Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-9.0) (или Yandex, Вконтакте).

    Отредактируйте файл `docker-compose.lite.yml`:

    * Впишите данные для авторизации Google в поля `Auth__Google__ClientId` и `Auth__Google__ClientSecret`
    * Задайте настройку `Auth__AllowPasswordAuth=false`, если хотите отключить менее безопасную авторизацию по паролю

4. _Опционально_:

   Если вы хотите запустить Bonsai на языке, отличном от русского, поменяйте локаль в `docker-compose.lite.yml`:
   Вместо `Locale=ru-RU` можно использовать `en-US`.

5. Запустите все контейнеры с помощью `docker compose`:
   ```
   docker-compose -f docker-compose.lite.yml up -d
   ```
6. После старта Bonsai будет доступен на порту `8080`.

## Разработка (на Windows)

Для участия в разработке понадобится:

* [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0): основной рантайм для Bonsai

1. Установите [NodeJS](https://nodejs.org/en/)
2. Скачайте [shared-сборку ffmpeg](https://www.ffmpeg.org/download.html) для вашей операционной системы и извлеките данные в папку `External/ffmpeg` в корне проекта (необходимы исполняемые файлы `ffmpeg` и `ffprobe`).
3. Создайте файл `appsettings.Development.json`, пропишите строку подключения к БД:

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

5. _Опционально, но рекомендуемо_:

    Создайте [приложение авторизации Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-10.0) (или Yandex, Вконтакте).

    Впишите данные для авторизации в файл `appsettings.Development.json` и установите свойство `AllowPasswordAuth` в значение `false`:

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
    
6. Создайте базу данных:

    ```
    dotnet ef database update
    ```
7. Запустите сборку стилей и скриптов:

    ```
    npm install
    npm run build
    ```
8. Запустите приложение (из Visual Studio или через `dotnet run`).

## Безопасность

### Резервные копии данных

Если вам ценна информация, которую вы заносите в Bonsai, обязательно **НАСТРОЙТЕ РЕЗЕРВНОЕ КОПИРОВАНИЕ**.

Копировать необходимо следующие данные:

* Базу данных (десятки мегабайт)
* Загруженные медиа-файлы в папке `wwwroot/media` (могут быть гигабайты)

В комплектации по-умолчанию вам достаточно скопировать две папки, используя любые доступные средства (копирование на дополнительные носители, загрузка в облако, и так далее).
При использовании БД PostgreSQL потребуются дополнительные действия по выгрузке содержимого БД.
Выбор наиболее уместного подхода, с учетом вашего бюджета и объема данных, остается за вами.

### Способы авторизации

Bonsai поддерживает 2 метода авторизации: OAuth с использованием внешних сайтов и авторизация по паролю.

OAuth является предпочтительным: он проще для пользователей, более безопасный и универсальный. **Если можете, используйте его!**
Для этого вам потребуется создать приложение авторизации на сайте [Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-10.0), [ВКонтакте](https://vk.com/editapp?act=create) или в [Яндексе](https://oauth.yandex.ru/client/new), как написано в инструкции.
Можно подключить несколько авторизационных приложений одновременно - пользователи смогут выбирать из них то, которое им больше по душе.

Также вы можете создать учетную запись с авторизацией по логину и паролю. Она пригодится в двух случаях:

* Быстро попробовать Bonsai в действии (установка без создания приложений значительно быстрее)
* Дать доступ родственникам, которые не зарегистрированы в соцсетях

Несколько фактов об авторизации, которые стоит иметь в виду:

* У одной учетной записи может быть только один способ авторизации: или пароль, или Google, или Вконтакте, и т.д.
* После создания учетной записи поменять тип авторизации нельзя.
* Учетные записи с авторизацией по паролю автоматически блокируются, если пароль был введен неверно слишком много раз подряд.
* Пароль может сменить только администратор вручную. Если у вас только одна учетная запись администратора и вы забыли от нее пароль - восстановить доступ можно только с помощью манипуляций с базой данных!
