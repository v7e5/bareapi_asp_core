# Barebones Todo Minimal API - ASP.NET Core 8

A sample minimal todo list api in C# with ASP.NET Core 8. 

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

Sample request:
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

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "task": string (required),
  "done": bool (optional),
  "due": unix timestamp (long, optional),
  "categories": [array of category ids (long, optional)]
}
EOL
)" \
'http://0.0.0.0:8000/todo/create'
```

#### todo: update
POST: `/todo/update`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (required),
  "task": string,
  "done": bool,
  "due": unix timestamp,
  "categories": [array of category ids]
}
EOL
)" \
'http://0.0.0.0:8000/todo/update'
```

#### todo: delete
POST: `/todo/delete`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (required)
}
EOL
)" \
'http://0.0.0.0:8000/todo/delete'
```

### Auth

#### login
POST: `/login`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "username": string (required),
  "passwd": string (required)
}
EOL
)" \
'http://0.0.0.0:8000/login'

```

#### logout
POST: `/logout`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
'http://0.0.0.0:8000/logout'

```

### Users

#### user: create
POST: `/user/create`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "username": string (required),
  "passwd": string (required)
}
EOL
)" \
'http://0.0.0.0:8000/user/create'
```

#### user: delete
POST: `/user/delete`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (required)
}
EOL
)" \
'http://0.0.0.0:8000/user/delete'
```
#### user: list
POST: `/user/list`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (optional),
  "username": string (optional)
}
EOL
)" \
'http://0.0.0.0:8000/user/list'
```

### Categories

#### category: create
POST: `/category/create`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
[
  {"name":"bug","color":"d73a4a"},
  {"name":"duplicate","color":"cfd3d7"}
]
EOL
)" \
'http://0.0.0.0:8000/category/create'
```

#### category: list
POST: `/category/list`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (optional),
  "name": string (optional),
  "color": string (optional)
}
EOL
)" \
'http://0.0.0.0:8000/category/list'
```

#### category: update
POST: `/category/update`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (required),
  "name": string,
  "color": string
}
EOL
)" \
'http://0.0.0.0:8000/category/update'
```

#### category: delete
POST: `/category/delete`

Sample request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "id": long (required)
}
EOL
)" \
'http://0.0.0.0:8000/category/delete'
```

