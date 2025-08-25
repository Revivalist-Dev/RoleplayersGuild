# Recommended VS Code Extensions for RoleplayersGuild Development

This document provides a curated list of Visual Studio Code extensions that are highly recommended to improve the development workflow for this project, based on its specific technology stack.

## C# & .NET Core Development

These extensions transform VS Code into a powerful C# IDE.

1.  **C# Dev Kit** (Publisher: Microsoft)
    -   **Why:** This is the modern, essential extension pack for .NET development in VS Code. It provides a rich editing experience, a Solution Explorer for managing your `.sln` and `.csproj` files, integrated unit testing (for xUnit), and a better debugging experience. **This is the most important extension to install for backend work.**

2.  **NuGet Gallery** (Publisher: patcx)
    -   **Why:** Provides a simple UI for browsing and installing NuGet packages directly within VS Code, which is often faster and more convenient than manually editing the `.csproj` file or using the command line.

## React, TypeScript & Frontend Development

These extensions streamline the development of your `Site.Client` React islands.

3.  **ESLint** (Publisher: Microsoft)
    -   **Why:** Integrates ESLint directly into the editor. It will highlight and provide quick-fixes for linting errors in your TypeScript and React code as you type, ensuring code quality and consistency.

4.  **Prettier - Code formatter** (Publisher: Prettier)
    -   **Why:** Automatically formats your code on save according to the Prettier rules defined in your project. This completely automates code formatting, ensuring a consistent style across the entire frontend codebase.

5.  **Stylelint** (Publisher: Stylelint)
    -   **Why:** A linter for your SASS/SCSS files. It helps you avoid errors and enforce consistent conventions in your stylesheets, just like ESLint does for your TypeScript.

## Database Development

6.  **PostgreSQL** (Publisher: Microsoft)
    -   **Why:** Allows you to connect directly to your PostgreSQL database (both local and remote) from within VS Code. You can browse tables, write and execute SQL queries, and view results without needing to switch to a separate database client like pgAdmin or DBeaver.

## Docker & Containerization

7.  **Docker** (Publisher: Microsoft)
    -   **Why:** This is an essential extension for your containerized setup. It provides syntax highlighting and IntelliSense for your `Dockerfile` and `docker-compose.yml` files, and adds a UI for managing your containers, images, and volumes directly from the VS Code sidebar.

## General Productivity & Code Quality

8.  **GitLens — Git supercharged** (Publisher: GitKraken)
    -   **Why:** Massively enhances the built-in Git capabilities of VS Code. It provides inline `git blame` annotations (so you can see who last changed a line of code), a rich commit history viewer, and powerful tools for comparing branches and commits.

9.  **EditorConfig for VS Code** (Publisher: EditorConfig)
    -   **Why:** Helps maintain consistent coding styles (like indentation and line endings) across different editors and IDEs by reading the `.editorconfig` file. This is a foundational tool for team-based projects.

10. **Markdown All in One** (Publisher: Yu Zhang)
    -   **Why:** As we're creating a lot of documentation in Markdown, this extension provides a huge quality-of-life boost with keyboard shortcuts for formatting, a live preview, and the ability to automatically generate a table of contents.