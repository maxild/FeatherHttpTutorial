Tutorial for featherhttp https://github.com/featherhttp/tutorial

## Run The React frontend

```bash
$ npm start
```

## Run the Blazor frontend

```bash
$ dotnet watch run
```

## Run the aspnetcore backend (server)

```bash
$ dotnet watch run
```

## Run the Blazor frontend and the AspNetCore backend simultanously

```bash
$ dotnet watch run --project src/Server/Server.csproj
```

The client Blazor WebAssembly app is published into the `/bin/Release/net5.0/publish/wwwroot`
folder of the server app, along with any other static web assets of the server app.
The two apps are deployed together.

This is the CLI command that does setup such a project (actually 3 projects)

```bash
$ dotnet new blazorwasm --hosted -o blazorwasmhosted
```
