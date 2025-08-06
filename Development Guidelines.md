Best Practices for RoleplayersGuild.com Development

This document outlines a modern, hybrid development strategy for RoleplayersGuild.com. The core principle is to leverage the strengths of both your server-side and client-side technologies: use ASP.NET Core (C#) for structure and data and React/TypeScript for interactivity.
## 1. Core Principles: Backend vs. Frontend

Think of your application as having two distinct parts that work together.
Backend (C# / ASP.NET Core)

The backend is the foundation of your site. It runs on your server and is responsible for:

    Business Logic: Handling core operations, calculations, and rules.

    Database Interaction: Securely querying and saving data using Dapper.

    Authentication & Authorization: Managing user logins and permissions.

    Initial Page Rendering: Serving the initial HTML shell for your Razor pages.

    API Endpoints: Providing data to the frontend as clean, structured JSON.

Your C# code should be the single source of truth for your data and security.
Frontend (TypeScript / React)

The frontend is the interactive experience for the user. It runs in their web browser and is responsible for:

    Dynamic UI: Updating the page without a full reload (e.g., submitting a form, showing a modal, filtering a list).

    State Management: Keeping track of UI state, like which tab is active or what's in a text field.

    User Interaction: Responding to clicks, keyboard input, and other user events.

All new client-side code should be written in TypeScript for type safety, consistency, and improved maintainability.
## 2. The "React Island" Architecture

For complex, interactive sections of your site like the "My-Characters" edit page, the best approach is the "React Island" model.

The concept is simple:

    The Razor Page is the "Host": Your .cshtml file becomes a minimal shell. Its only jobs are to render the main layout and a single <div> element that will act as the root for your React application.

    Pass the Initial ID: The Razor page passes the necessary ID (like the CharacterId) to the div using a data-* attribute.

    <!-- In Edit.cshtml -->
    <div id="character-editor-root" data-character-id="@Model.Input.CharacterId">
        <!-- A loading spinner can go here -->
    </div>

    React Takes Over: A self-contained React application, built and compiled separately, is "mounted" onto that div. From that point on, React controls everything inside the "island."

    Data via API: The React app makes an initial fetch or axios call to a C# API endpoint (e.g., /api/characters/@Model.Input.CharacterId) to get all the data it needs to render the editor. All subsequent saves and updates also go through these API endpoints.

Benefits of this approach:

    Robustness: Eliminates the fragile system of loading HTML partials via AJAX. State is managed cleanly within React, preventing "infinite loading" bugs.

    Decoupling: Your C# backend and React frontend are completely separate. The backend provides data, and the frontend displays it. They can be developed and tested independently.

    Better UX: The user gets a faster, smoother, app-like experience without full page reloads.

## 3. Recommended Tooling & Workflow
Backend (No Changes)

    Language: C#

    Framework: ASP.NET Core Razor Pages

    Data Access: Dapper

Frontend (The New Standard)

    Framework: React for complex, stateful "islands" of interactivity.

    Language: TypeScript for all new client-side code.

    Build Tool: Vite is the recommended tool for creating and managing your React/TypeScript projects. It's incredibly fast and simple to configure.

    Project Structure: Maintain a separate folder for your client-side code (e.g., Site.Client).

    Data Fetching: Use a standard library like Axios to make API calls from your React components to your C# API controllers.

## 4. Practical Steps for a New Interactive Feature

    Define the API First: In C#, create a new ApiController (e.g., StoriesApiController.cs). Define [HttpGet], [HttpPost], [HttpPut] endpoints that return IActionResult with JSON data (Ok(data)).

    Create the Razor Host Page: Create a minimal .cshtml page. Its only purpose is to render the root <div> with the necessary data- attributes and the <script> tags pointing to your compiled frontend assets.

    Build the React Component: In your Site.Client project, build the interactive UI as a React component. Manage all state, data fetching, and user events within React.

    Build and Integrate: Run npm run build in your Site.Client directory. This will generate optimized .js and .css files. Configure your ASP.NET Program.cs to serve these static files from the build output directory (e.g., wwwroot/client/dist).

By following this clear separation of concerns, you create a more stable and scalable application that leverages the best of both the .NET and JavaScript ecosystems.