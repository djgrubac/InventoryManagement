# Inventory_Management

[![CI](https://github.com/djgrubac/inventory-management-api/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/djgrubac/inventory-management-api/actions/workflows/ci.yml)

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/Inventory_Management) version 8.0.6.

## Build

Run `dotnet build -tl` to build the solution.

## Run

To run the web application:

```bash
cd .\src\Web\
dotnet watch run
```

Navigate to https://localhost:5001. The application will automatically reload if you change any of the source files.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::8.0.6
```

## Test

The solution contains unit tests (`tests/Application.UnitTests`) and integration tests (`tests/Application.IntegrationTests`). The integration tests run against an in-memory SQLite database, so no Postgres instance is needed.

To run all tests:
```bash
dotnet test
```

## CI

GitHub Actions ([.github/workflows/ci.yml](../.github/workflows/ci.yml), at the repo root) runs on every pull request to `master` and every push to `master`. It:

1. Restores and builds the solution in Release (warnings are treated as errors).
2. Runs all unit and integration tests and uploads the test results and coverage as the `test-results` artifact.
3. Fails if the EF Core model has changes without a migration. To run the same check locally:

```bash
dotnet tool restore
dotnet ef migrations has-pending-model-changes --project src/Infrastructure --startup-project src/Web
```

## Help
To learn more about the template go to the [project website](https://github.com/jasontaylordev/CleanArchitecture). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.