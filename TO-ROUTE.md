# RoleplayersGuild Routing and Asset Serving

This document explains the request lifecycle and asset serving strategy for the RoleplayersGuild platform in a local development environment. The architecture consists of two main projects:

1.  **`RoleplayersGuild`**: The main ASP.NET Core Razor Pages website.
2.  **`Site.Client`**: A Vite-based React project for interactive components ("islands").

---

## Core Concepts

### The NGINX Reverse Proxy

Think of the `nginx` service as the main receptionist for the application. It is the single entry point for all traffic from the browser. Its job is to route requests to the correct backend service.

-   **Vite Dev Server (`/vite-dev/{...}`)**: Routed directly to the `site-client-dev` Vite dev server for HMR and module loading.
-   **Static Assets (`/fonts/{...}`, `/images/{...}`)**: Routed to the main `roleplayersguild` application service, which has access to the mounted `Site.Assets` directory.
-   **All Other Requests (`/{...}`)**: Routed to the main `roleplayersguild` application service.

### React "Islands" Architecture

We do not have a full Single-Page Application (SPA). Instead, the `RoleplayersGuild` website is a traditional server-rendered site using Razor Pages. We embed small, interactive React components (like a character editor or a dynamic chat window) into these Razor Pages. These are the "islands" of interactivity.

-   **`Site.Client/src/main.tsx`**: The main entry point for *global* JavaScript and SCSS.
-   **Component-Specific Scripts**: Scripts that are only used by a single component are now loaded directly within that component's Razor view for better modularity.

### The Vite Configuration (`vite.config.ts`)

This file is the command center for the `Site.Client` project. It tells the Vite build tool how to compile, bundle, and serve the frontend assets.

-   **`base`**: This is set to `/vite-dev/`. This unified path is used by NGINX to identify requests that should be routed to the Vite dev server.
-   **`plugins`**: Includes the `copyAssets` plugin, which copies fonts and images from `Site.Assets` into the final build directory (`dist/assets`).
-   **`server.hmr.clientPort`**: This is the crucial setting for proxying. It is set to the public-facing port (`8080`) to ensure the HMR client connects back to the correct address through NGINX.

---

## Asset Serving Strategy

### Development Environment (`docker-compose up`)

In development, the goal is speed and features like Hot Module Replacement (HMR).

1.  The **`nginx`** service listens on the host machine's port `8080`.
2.  A Razor Page in the `RoleplayersGuild` project calls the `ViteManifestService` to generate `<script>` tags pointing to the Vite dev server (e.g., `http://localhost:8080/vite-dev/src/main.tsx`).
3.  The browser requests these assets, which hits **NGINX**. NGINX sees the `/vite-dev/` prefix and proxies the request directly to the **Vite dev server**.
4.  The `main.tsx` file imports the `site.scss` file. Vite processes this import and injects the compiled CSS into the page.
5.  The compiled CSS contains URLs pointing to font files (e.g., `/fonts/bootstrap-icons.woff2`). The browser requests these assets.
6.  **NGINX** sees the `/fonts/` prefix and proxies the request to the **`roleplayersguild`** service, which serves the file from the mounted `Site.Assets` directory.

### Production Environment (`docker-compose --profile prod up`)

In production, assets are compiled, optimized, and served statically for maximum performance.

1.  The `vite build` command is executed. The `copyAssets` plugin copies fonts and images to `dist/assets`. Vite bundles all JavaScript and CSS into hashed files in `dist/assets`.
2.  The `ViteManifestService` reads the `manifest.json` file to inject the correct, cache-busted asset URLs into the HTML.
3.  The `RoleplayersGuild` application serves these static assets from its `wwwroot` directory.

#### The Role of `ViteManifestService`

The `IViteManifestService` is the crucial bridge between the Vite build output and the ASP.NET Core Razor Pages.

-   **In a Production Environment**: It reads `manifest.json` to find the final, hashed asset filenames.
-   **In a Development Environment**: It generates script tags that point directly to the Vite dev server's entry points (e.g., `main.tsx` or a component-specific script like `UserNavHorizontal.ts`). It does *not* render any `<link>` tags for stylesheets, as this is now handled by the Vite HMR client.

---

## Common Pitfalls & Debugging Checklist

MIME type errors, 404s for static assets, or WebSocket errors during development are almost always caused by a misconfiguration in the chain from the C# backend to the NGINX proxy.

Follow this checklist to debug routing issues:

1.  **Check the Browser's Network Tab**:
    -   **What is the URL being requested?** For JS modules, it should start with `/vite-dev/`. For static assets like fonts and images, it should start with `/fonts/` or `/images/`.
    -   If the prefixes are incorrect, the problem is likely in the `site.scss` file (for font paths) or the `ViteManifestService` (for script paths).

2.  **Verify the `RoleplayersGuild` Application**:
    -   **`_Layout.cshtml`**: Ensure the call to `@await ViteAssets.RenderViteScripts()` is present and correct.
    -   **`ViteManifestService.cs`**: Ensure it is only configured to load `main.tsx`.

3.  **Verify the Proxy Layer & HMR**:
    -   **`vite.config.ts`**: Is `server.hmr.clientPort` set to the correct public-facing port (e.g., `8080`)?
    -   **`nginx/default.conf.template`**: Are the `location` blocks for `/vite-dev/`, `/fonts/`, and `/images/` present and correctly proxying to the appropriate services?

If all of these are correct, a full, clean rebuild (`docker-compose down -v` followed by `docker-compose up --build`) will resolve most issues.