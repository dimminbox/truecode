# TrueCode

Набор сервисов на .NET 8 и PostgreSQL: регистрация и вход пользователей, хранение и выдача курсов валют ЦБ РФ, единая точка входа.

## Из чего состоит проект

| Проект | Порт | Что делает |
|---|---|---|
| `TrueCode.Users.Api` | 5000 | Регистрация, вход, обновление и отзыв токенов. Выдаёт JWT (RS256) и публикует ключи в `/.well-known/jwks.json`. Эндпоинт `register` защищён заголовком `token`. |
| `TrueCode.Finance.Api` | 5001 | Отдаёт курсы валют: `GET /currencies` (постранично) и `GET /currencies/{code}`. Требует JWT, выданный `Users.Api`. |
| `TrueCode.Finance.Updater` | нет (фоновый воркер) | По расписанию скачивает курсы с сайта ЦБ, разбирает XML и сохраняет их в базу. Несколько экземпляров не конфликтуют благодаря advisory lock в PostgreSQL. |
| `TrueCode.PublicGateway` | 5002 | Публичный шлюз на Ocelot. Проксирует запросы к `Users.Api` и `Finance.Api`, на `/` отдаёт страницу со ссылками на Scalar `/scalar/users` и `/scalar/finance`. |

Общий код лежит в `TrueCode.SharedKernel` (базовые типы домена, доступ к БД, `Result<T>`). Схему базы создаёт отдельное консольное приложение `TrueCode.DbMigrations`. Тесты находятся в `tests/`.

## Что нужно

- .NET SDK 8 или новее
- Docker (для локального PostgreSQL)

## 1. Поднять PostgreSQL

Из корня репозитория:

```bash
docker compose -f infra/docker-compose.yml up -d
```

По умолчанию создаётся база `truecode` с пользователем `postgres` и паролем `postgres` на порту `5432`. Значения можно переопределить переменными окружения `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` и `POSTGRES_PORT`.

Проверить, что база готова:

```bash
docker compose -f infra/docker-compose.yml ps
```

## 2. Применить миграции

Миграции лежат в проекте `TrueCode.DbMigrations` и написаны на [FluentMigrator](https://fluentmigrator.github.io/).

Из корня репозитория:

```bash
dotnet run --project src/TrueCode.DbMigrations
```

Все ещё не применённые миграции выполнятся по порядку версий. Применённые FluentMigrator пропускает, их список хранится в таблице `VersionInfo`. Повторный запуск безопасен.

## Как обновлять миграции

1. Создайте файл в `src/TrueCode.DbMigrations/Migrations/` с именем вида `M<версия>_<Описание>.cs`.
2. Унаследуйтесь от `Migration`, добавьте атрибут `[Migration(<версия>)]` и реализуйте `Up()` и `Down()`.
3. Версия должна быть больше всех существующих. Удобный формат: `ггггммддччмм`, например `202611011200`.
4. Запустите миграции командой из шага 2.

Не меняйте уже применённые миграции: если схему нужно исправить, добавьте новую миграцию.

## Строка подключения

По умолчанию берётся из `src/TrueCode.DbMigrations/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=truecode;Username=postgres;Password=postgres"
  }
}
```

Чтобы указать другую базу, не меняя файл, задайте переменную окружения (она переопределяет `appsettings.json`):

```bash
ConnectionStrings__Postgres="Host=db.example.com;Port=5432;Database=truecode;Username=app;Password=secret" \
  dotnet run --project src/TrueCode.DbMigrations
```

## Проверить результат

```bash
docker exec -it truecode-postgres psql -U postgres -d truecode -c '\dt'
```

Должны быть таблицы `currency`, `users`, `user_auth_history` и служебная `VersionInfo`.

## Откат

Откатить миграции до нужной версии можно командой `down`:

```bash
dotnet run --project src/TrueCode.DbMigrations -- down 202610091005
```

Выполнятся методы `Down()` всех миграций с версией больше указанной, в обратном порядке. Версия, которую вы указали, останется применённой.

Откат необратимо удаляет данные, если в `Down()` есть удаление таблицы или колонки. Миграции, которые пересоздают таблицу, возвращают только структуру, а не данные.

Для локальной базы самый простой способ начать с нуля это пересоздать её вместе с томом:

```bash
docker compose -f infra/docker-compose.yml down -v
docker compose -f infra/docker-compose.yml up -d
dotnet run --project src/TrueCode.DbMigrations
```

Флаг `-v` удаляет том с данными, поэтому все данные в локальной базе будут потеряны.
