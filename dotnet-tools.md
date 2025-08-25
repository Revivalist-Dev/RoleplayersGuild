# .NET Tooling and NuGet Packages

This document provides an overview of the key .NET tools and NuGet packages used across the backend projects (`RoleplayersGuild`, `RPGateway`, etc.) to ensure code quality, consistency, and performance.

## Code Quality & Style

### `StyleCop.Analyzers`

- **Purpose:** A set of Roslyn analyzers that enforce C# style and consistency rules directly in the IDE and during the build process.
- **Why it's used:** It guarantees that all C# code in the repository adheres to a common, industry-standard style guide. This covers everything from code layout and naming conventions to XML documentation, ensuring the codebase is clean, readable, and maintainable. Configuration is managed in the `stylecop.json` file.

## API Gateway

### `Ocelot`

- **Purpose:** A lightweight, open-source API Gateway for .NET applications.
- **Why it's used:** It provides a single, unified entry point for all API requests, routing them to the appropriate downstream services (`RoleplayersGuild`, `Site.Client` dev server, etc.). It is configured via the `ocelot.json` and `ocelot.Development.json` files.

## Web Server

### `Kestrel`

- **Purpose:** The default, cross-platform web server for ASP.NET Core.
- **Why it's used:** It is a lightweight, high-performance web server that is tightly integrated with the ASP.NET Core framework. It is used to host the `RoleplayersGuild` and `RPGateway` applications.

## Configuration

### Strongly-Typed Configuration (Options Pattern)

- **Purpose:** Binds application settings from `appsettings.json` to strongly-typed C# objects.
- **Why it's used:** This approach, also known as `IConfiguration` binding, replaces the use of "magic strings" (e.g., `_configuration["Section:Key"]`) with injectable, strongly-typed classes. This provides compile-time safety, enables IntelliSense, and makes configuration easier to manage and test. All configuration models are located in the `Project.Configuration` directory and registered in `Program.cs`.

## API & Documentation

### `Swashbuckle.AspNetCore` (Swagger)

- **Purpose:** A tool for automatically generating OpenAPI (formerly Swagger) documentation for ASP.NET Core web APIs.
- **Why it's used:** Swashbuckle inspects the API controllers and generates a `swagger.json` file. It also provides the Swagger UI, an interactive web interface where developers can explore, understand, and test API endpoints directly from their browser. This is invaluable for frontend development and for ensuring API consistency. The Swagger UI is enabled only in the Development environment and is accessible at the `/swagger` endpoint.

## Database

### `FluentMigrator`

- **Purpose:** A database migration framework for .NET that allows schema changes to be defined and versioned in C# code.
- **Why it's used:** It provides a robust, automated, and database-agnostic way to manage the evolution of the database schema. Instead of writing and managing `.sql` scripts manually, migrations are defined as versioned C# classes. This makes schema changes trackable in source control, easily repeatable across different environments (development, testing, production), and simplifies the deployment process. Migrations are located in the `Site.Database/Migrations` folder and are run automatically on application startup.

## Monitoring & Health

### `AspNetCore.HealthChecks`

- **Purpose:** A library that provides a system for reporting the health of an application and its dependencies.
- **Why it's used:** Health checks are essential for monitoring the status of a running application. We have configured a health check endpoint at `/healthz` that verifies the application's ability to connect to the PostgreSQL database. This endpoint can be used by external monitoring services or container orchestrators (like Docker or Kubernetes) to determine if the application is running correctly and to automatically restart it if it becomes unhealthy.

## Object-to-Object Mapping

### `Mapster`

- **Purpose:** A high-performance library for object-to-object mapping.
- **Why it's used:** It automates the process of converting data between different C# objects (e.g., from a database entity to an API data transfer object). This significantly reduces boilerplate code, prevents manual mapping errors, and keeps business logic clean. It is configured in `Program.cs` and can be used by injecting the `IMapper` interface into any service.

## Database Migrations CLI

### `FluentMigrator.DotNet.Cli` (`dotnet-fm`)

- **Purpose:** A .NET local tool for managing FluentMigrator database migrations from the command line.
- **Why it's used:** While migrations run automatically on application startup, this tool provides fine-grained control during development. It allows you to run, revert, list, and validate migrations without needing to start the web server. It is installed via the local tool manifest (`.config/dotnet-tools.json`).
- **To Restore Tools:** Run `dotnet tool restore`.
- **Common Commands:**
    - `dotnet fm list migrations -p "RoleplayersGuild.csproj" -c "DefaultConnection" --preview`: See all migrations and their status.
    - `dotnet fm migrate up -p "RoleplayersGuild.csproj" -c "DefaultConnection" --preview`: Apply all pending migrations.
    - `dotnet fm migrate down 202508130001 -p "RoleplayersGuild.csproj" -c "DefaultConnection" --preview`: Rollback a specific migration.