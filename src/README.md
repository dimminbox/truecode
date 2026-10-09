# Запуск миграций БД

Миграции лежат в проекте `TrueCode.DbMigrations` и написаны на [FluentMigrator](https://fluentmigrator.github.io/). Они создают таблицы `currency`, `users` и `user_auth_history` в PostgreSQL.

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

Из корня репозитория:

```bash
dotnet run --project src/TrueCode.DbMigrations
```

Все ещё не применённые миграции выполнятся по порядку версий. Применённые FluentMigrator пропускает, их список хранится в таблице `VersionInfo`. Повторный запуск безопасен.

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

## Миграции

| Версия | Файл | Что делает |
|---|---|---|
| `202610091000` | `M202610091000_CreateCurrencyTable.cs` | таблица `currency` |
| `202610091001` | `M202610091001_CreateUsersTable.cs` | таблица `users` |
| `202610091002` | `M202610091002_CreateUserAuthHistoryTable.cs` | таблица `user_auth_history` |
| `202610091003` | `M202610091003_RecreateCurrencyTableWithGuidId.cs` | `currency.id` становится `uuid` вместо `int` (таблица пересоздаётся) |
| `202610091004` | `M202610091004_AddCodeToCurrencyTable.cs` | в `currency` добавляется уникальная колонка `code` (3 буквы, например `USD`) |
| `202610091005` | `M202610091005_AddUpdatedAtToCurrencyTable.cs` | в `currency` добавляется колонка `updated_at` (время последнего изменения курса) |
| `202610091006` | `M202610091006_RecreateCurrencyTableWithIntId.cs` | `currency.id` снова `int` с автоинкрементом, как в задании (таблица пересоздаётся) |

## Добавить новую миграцию

1. Создайте файл в `src/TrueCode.DbMigrations/Migrations/` с именем вида `M<версия>_<Описание>.cs`.
2. Унаследуйтесь от `Migration`, добавьте атрибут `[Migration(<версия>)]` и реализуйте `Up()` и `Down()`.
3. Версия должна быть больше всех существующих. Удобный формат: `ггггммддччмм`, например `202611011200`.
4. Запустите миграции командой из шага 2.

Не меняйте уже применённые миграции: если схему нужно исправить, добавьте новую миграцию.

## Проверить результат

```bash
docker exec -it truecode-postgres psql -U postgres -d truecode -c '\dt'
```

Должны быть таблицы `currency`, `users`, `user_auth_history` и служебная `VersionInfo`.

## Откат

Автоматического отката через `dotnet run` нет: приложение выполняет только `MigrateUp()`. Методы `Down()` в миграциях нужны для ручного отката или для тестов. Для локальной базы самый простой откат это пересоздать её вместе с томом:

```bash
docker compose -f infra/docker-compose.yml down -v
docker compose -f infra/docker-compose.yml up -d
dotnet run --project src/TrueCode.DbMigrations
```

Флаг `-v` удаляет том с данными, поэтому все данные в локальной базе будут потеряны.
