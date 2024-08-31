# Barebones Todo Minimal API - ASP.NET Core 8

A minimal todo list api in C# with ASP.NET Core 8. 

## Features
+ Builds on the bare minimum `WebApplication.CreateEmptyBuilder`
+ Implements a simple cookie based user authentication / session using raw http
headers, backed by a session table in the database.
+ Uses sqlite database and raw sql queries. `Microsoft.Data.Sqlite` is the only
required package dependency.
+ Implements keyset/cursor based paginnation for the todo/list route 

The shell scripts in misc are intended to be run in a zsh shell on linux. They
include convenience functions for building / executing as well as testing the
routes using curl. Feel free to ignore them if they don't match your use case.

https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async
