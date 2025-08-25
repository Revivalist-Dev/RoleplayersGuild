# Razor Pages Best Practices

This document outlines the best practices and conventions for developing with ASP.NET Core Razor Pages in the `RoleplayersGuild` project.

---

## 1. Project Structure

-   **File Location**: All Razor Pages must be located within the `/Site.Directory/` folder.
-   **Colocation**: The C# PageModel (`.cshtml.cs`) file should always be colocated with its corresponding Razor view (`.cshtml`) file.
-   **Feature Folders**: Group related pages into "feature folders" (e.g., `/Account-Panel/`, `/Community/Characters/`) to keep the project organized.

## 2. PageModels (`.cshtml.cs`)

-   **Constructor Injection**: Always use constructor injection to receive services (e.g., `IUserService`, `ICharacterDataService`). Do not use property injection or service locators.
-   **`[BindProperty]`**: Use the `[BindProperty]` attribute on public properties to bind form data on POST requests. For security, add `SupportsGet = true` only when you explicitly need to bind data from the query string on a GET request.
-   **Handler Methods**: Use named handler methods for clarity (e.g., `OnPostCreateAsync`, `OnGetDeleteAsync`). Avoid using generic `OnPostAsync` or `OnGetAsync` when a page handles multiple distinct actions.
-   **Return Types**: Always use `async Task<IActionResult>` for handler methods. Return specific results like `Page()`, `RedirectToPage()`, `NotFound()`, or `JsonResult()` to clearly express the outcome.

## 3. Views (`.cshtml`)

-   **Dependency Injection**: Use the `@inject` directive at the top of a view to inject services that are needed for rendering logic (e.g., `@inject IViteManifestService ViteAssets`).
-   **ViewData vs. Properties**: Prefer strongly-typed properties on your PageModel over using the `ViewData` dictionary. Use `ViewData` only for simple, view-specific data that doesn't belong on the model, such as the page title (`ViewData["Title"] = "My Page";`).
-   **Partial Views vs. View Components**:
    -   Use a **Partial View** (`<partial name="_MyPartial" />`) for simple, reusable chunks of Razor markup that do not require their own logic.
    -   Use a **View Component** (`@await Component.InvokeAsync("MyComponent")`) for more complex, self-contained UI elements that require their own data fetching and logic (e.g., the site header, user navigation).

## 4. Integrating Client-Side Interactivity

Our project enhances server-rendered Razor Pages with client-side interactivity using both standalone TypeScript modules and self-contained React components ("Islands").

### Loading Component-Specific Scripts

To keep the application modular and efficient, JavaScript that is specific to a single component should be loaded directly within that component's Razor view. This ensures that the browser only downloads the scripts it needs for the components that are actually on the page.

-   **`IViteManifestService`**: Use the `@inject` directive to get an instance of the `IViteManifestService`.
-   **`RenderViteScripts()`**: Call this method at the bottom of your component's `.cshtml` file, passing the path to the component's specific TypeScript entry point.

**Example (`UserNavHorizontal/Default.cshtml`):**

```csharp
@model RoleplayersGuild.Site.Model.UserNavViewModel
@inject RoleplayersGuild.Site.Services.IViteManifestService ViteAssets

@* ... component HTML ... *@

@* Load the specific scripts for this component *@
@await ViteAssets.RenderViteScripts(
    "src/Site.Scripts/UserNavHorizontal.ts",
    "src/Site.Scripts/UserNavHorizontalToggle.ts"
)
```

-   **Multiple Scripts**: You can pass multiple script paths to the `RenderViteScripts` method if a component requires more than one entry point.

### Loading Global Scripts

Global scripts that are required on every page (such as analytics, anti-forgery token handling, and the main stylesheet) are loaded once in the main `_Layout.cshtml` file via the `main.tsx` bundle. Only add scripts to `main.tsx` if they are truly universal.