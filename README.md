# Bonsai

Фамильный вики-движок и фотоальбом.

### [Демо: попробовать в действии](https://bonsai.kirillorlov.pro)

## Возможности

* Страницы с разметкой Markdown
* Медиа-файлы: фото, видео, планируется поддержка документов PDF
* Отметки людей на фото
* Родственные связи (с проверками и автоматическим выводом)
* Факты (дата рождения, пол, группа крови, владение языками, хобби, и так далее)
* Контроль доступа по ролям: администратор, редактор, читатель, гость
* История правок: для любой страницы или медиа-файла хранится история с diff'ами и возможностью отката к предыдущей версии

## Скриншоты

#### Публичные страницы:

<a href="https://user-images.githubusercontent.com/604496/46574247-037d4f00-c9a9-11e8-8585-0d574dda2600.png"><img src="https://user-images.githubusercontent.com/604496/46574252-1859e280-c9a9-11e8-821f-daeaaac7de3f.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574259-2c054900-c9a9-11e8-8ecc-ca542053f665.png"><img src="https://user-images.githubusercontent.com/604496/46574288-9a4a0b80-c9a9-11e8-8373-2a7d3e00289c.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574262-31629380-c9a9-11e8-9ea6-18fbe63f239f.png"><img src="https://user-images.githubusercontent.com/604496/46574291-9f0ebf80-c9a9-11e8-8656-8a54dd2f2be7.png" /></a>

#### Панель администратора:

<a href="https://user-images.githubusercontent.com/604496/46574266-3f181900-c9a9-11e8-828d-9d9a5db25acb.png"><img src="https://user-images.githubusercontent.com/604496/46574292-a209b000-c9a9-11e8-8193-cd99fc1f5f91.png" /></a>
<a href="https://user-images.githubusercontent.com/604496/46574268-43443680-c9a9-11e8-974f-f8a60fbeaa74.png"><img src="https://user-images.githubusercontent.com/604496/46574297-a504a080-c9a9-11e8-8612-d3e5cd1592a4.png" /></a>

## Установка с помощью Docker
1. Скачайте файл [docker-compose.lite.yml](docker-compose.lite.yml).

2. _Опционально_: 

    Настройте доступ по HTTPS для дополнительной безопасности.

    Вы можете использовать любые доступные сервисы, например [Cloudflare Tunnel](https://github.com/impworks/bonsai/wiki/%D0%9D%D0%B0%D1%81%D1%82%D1%80%D0%BE%D0%B9%D0%BA%D0%B0-%D1%82%D1%83%D0%BD%D0%BD%D0%B5%D0%BB%D1%8F-Cloudflare) (бесплатно, несложно, нужен домен), Synology DDNS (бесплатно, просто, нужен Synology NAS), или другие.

    Это трудоемкий шаг, поэтому если вы просто хотите попробовать Bonsai своими руками локально - его и следующий можно пропустить или отложить.

3. _Опционально_:

    Создайте [приложение авторизации Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0) (или Yandex, Вконтакте).

    Отредактируйте файл `docker-compose.lite.yml`:

    * Впишите данные для авторизации Google в поля `Auth__Google__ClientId` и `Auth__Google__ClientSecret`
    * Задайте настройку `Auth__AllowPasswordAuth=false`, если хотите отключить менее безопасную авторизацию по паролю

4. Запустите все контейнеры с помощью `docker compose`:
   ```
   docker-compose -f docker-compose.lite.yml up -d
   ```
5. После старта Bonsai будет доступен на порту `8080`.

## Разработка (на Windows)

Для участия в разработке понадобится:

* [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0): основной рантайм для Bonsai

1. Установите [NodeJS 14](https://nodejs.org/en/)
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

    Создайте [приложение авторизации Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0) (или Yandex, Вконтакте).

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
Для этого вам потребуется создать приложение авторизации на сайте [Google](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0), [ВКонтакте](https://vk.com/editapp?act=create) или в [Яндексе](https://oauth.yandex.ru/client/new), как написано в инструкции.
Можно подключить несколько авторизационных приложений одновременно - пользователи смогут выбирать из них то, которое им больше по душе.

Также вы можете создать учетную запись с авторизацией по логину и паролю. Она пригодится в двух случаях:

* Быстро попробовать Bonsai в действии (установка без создания приложений значительно быстрее)
* Дать доступ родственникам, которые не зарегистрированы в соцсетях

Несколько фактов об авторизации, которые стоит иметь в виду:

* У одной учетной записи может быть только один способ авторизации: или пароль, или Google, или Вконтакте, и т.д.
* После создания учетной записи поменять тип авторизации нельзя.
* Учетные записи с авторизацией по паролю автоматически блокируются, если пароль был введен неверно слишком много раз подряд.
* Пароль может сменить только администратор вручную. Если у вас только одна учетная запись администратора и вы забыли от нее пароль - восстановить доступ можно только с помощью манипуляций с базой данных!