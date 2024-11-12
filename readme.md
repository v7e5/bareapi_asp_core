# Barebones Todo Minimal API - ASP.NET Core 8 (SQlite)

A sample minimal todo list api in C# with ASP.NET Core 8. This project uses
SQlite for database. Check out
[/v7e5/bareapi_asp_core_mssql](https://github.com/v7e5/bareapi_asp_core_mssql)
for a version that uses SQLServer/MSSQL.

## Features
+ Builds on the bare minimum `WebApplication.CreateEmptyBuilder`
+ Implements a simple cookie based user authentication / session using raw http
headers, backed by a session table in the database.
+ Avoids the complexity and ceremony of EFCore/ORMs in favor of raw sql queries/ADO.NET.
+ Uses sqlite for database. `Microsoft.Data.Sqlite` is the only required package dependency. 
+ Implements keyset/cursor based pagination for the todo/list route 

**Note**: I've used synchronous ADO.NET methods instead of 
the preferred asynchronous variants for reasons detailed here. 
https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async

The shell scripts in misc are intended to be run in a zsh shell on linux. They
include convenience functions for building / executing as well as testing the
routes using curl. Feel free to ignore them if they don't match your use case.

## Endpoints

### Auth

#### login
POST: `/login`

Example request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "username": string (required),
  "password": string (required)
}
EOL
)" \
'http://0.0.0.0:8000/login'

```

#### logout
POST: `/logout`

Example request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
'http://0.0.0.0:8000/logout'

```

### Todos

#### todo: list
POST: `/todo/list`

This route implements keyset pagination. It is controlled by three optional
parameters: `cursor_init`, `cursor_prev` and `cursor_next`. Initial 
request without any parameters returns a JSON response something like this:
```json
{
  "data": [ // array of todos
    {todo object}
  ],
  "cursor_init": // index of first row (return with every next/prev request),
  "cursor_prev": //index of last page, use to go back (is null for first page)
  "cursor_next": //index for next page, use to go forward,
}
```
Include one of either `cursor_next` or `cursor_prev` along with `cursor_init`.
To go forward, include the `cursor_next` param in your request, which you
will have recieved from an earlier request. To go back, send `cursor_prev`.
The value for `cursor_init` does not change for a given query. It is used as an
anchor to determine if there are any rows while going backwards.

Example request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (optional),
  "task": string (optional),
  "done": bool (optional),
  "due_from": unix timestamp (long, optional),
  "due_to": unix timestamp (long, optional),
  "categories": [array of category ids (long, optional)],
  "cursor_init": long (optional),
  "cursor_next": long (optional),
  "cursor_prev": long (optional)
}
EOL
)" \
'http://0.0.0.0:8000/todo/list'

```

#### todo: create
POST: `/todo/create`

```json
{
  "task": string (required),
  "done": bool (optional),
  "due": unix timestamp (long, optional),
  "categories": [array of category ids (long, optional)]
}
```

#### todo: update
POST: `/todo/update`

```json
{
  "id": long (required),
  "task": string,
  "done": bool,
  "due": unix timestamp,
  "categories": [array of category ids]
}
```

#### todo: delete
POST: `/todo/delete`

```json
{
  "id": long (required)
}
```

### Users

#### user: create
POST: `/user/create`

```json
{
  "username": string (required),
  "password": string (required)
}
```

#### user: delete
POST: `/user/delete`

```json
{
  "id": long (required)
}
```
#### user: list
POST: `/user/list`

```json
{
  "id": long (optional),
  "username": string (optional)
}
```

### Categories

#### category: create
POST: `/category/create`

```json
[
  {"name":"bug","color":"d73a4a"},
  {"name":"duplicate","color":"cfd3d7"}
]
```

#### category: list
POST: `/category/list`

```json
{
  "id": long (optional),
  "name": string (optional),
  "color": string (optional)
}
```

#### category: update
POST: `/category/update`

```json
{
  "id": long (required),
  "name": string,
  "color": string
}
```

#### category: delete
POST: `/category/delete`

```json
{
  "id": long (required)
}
```
