# Hand-off Document: Site Styling Architecture

This document provides a comprehensive overview of the current architecture for handling stylesheets in the RoleplayersGuild project. It details the modern, Vite-centric workflow, outlines the changes made during our recent refactoring, and identifies key points of interaction and potential sources of error.

---

## 1. Current Architecture: Vite-Powered HMR

The site's styling is now managed entirely by the Vite development server, providing a seamless development experience with Hot Module Replacement (HMR) and an optimized, automated build process for production.

-   **Source Files**: All SCSS source files are located in `Site.Assets/styles/scss/`.
-   **Main Entry Point**: The file `site.scss` serves as the primary entry point for all globally loaded styles.
-   **Development Workflow**:
    1.  The `main.tsx` file imports `site.scss`.
    2.  The Vite dev server processes this import, compiles the SCSS in memory, and injects the resulting CSS into the page via HMR.
    3.  Any changes to `.scss` files are detected by Vite and instantly injected into the browser, without requiring a page reload.
-   **Production Workflow**:
    1.  The `vite build` command processes the `site.scss` import in `main.tsx`, and bundles all styles into a final, minified, and hashed `site.css` file.
    2.  The `ViteManifestService` reads the generated `manifest.json` to inject a link to this final, versioned CSS file.

---

## 2. Key Changes Made

During our debugging session, we made several critical changes to fix layout issues and modernize the asset pipeline.

1.  **Hybrid Asset Loading Strategy**: We refactored the system to use a hybrid approach. `main.tsx` is the entry point for all *global* assets, while component-specific scripts are now loaded directly within their respective Razor views.
    -   **Reason**: This improves modularity and ensures that pages only load the scripts they actually need, which is more efficient than a single global bundle.

2.  **Corrected Font Loading**: We configured NGINX to correctly proxy font requests to the `roleplayersguild` service and updated the `site.scss` file to use the correct, root-relative paths.
    -   **Reason**: This ensures that the browser can find and load the Bootstrap Icons font files, which are served from the mounted `Site.Assets` directory.

3.  **Modernized Sass Syntax**: We updated `site.scss` to use the modern `@use` syntax with explicit file extensions.
    -   **Reason**: This resolves deprecation warnings and removes ambiguity in the import process.

---

## 3. Key Files and Potential Points of Error

This is a checklist of the critical files that control the styling system. If a style-related issue occurs, the problem is almost certainly located in one of these files.

1.  **`Site.Assets/styles/scss/site.scss`**
    -   **Role**: The main entry point for all global styles.
    -   **Potential Error**: An incorrect `@use` or `@forward` rule, or an incorrect font path variable, can break the entire site's styling.

2.  **`Site.Client/src/main.tsx`**
    -   **Role**: Imports the `site.scss` file, making it part of the Vite asset graph.
    -   **Potential Error**: If the import statement for `site.scss` is removed, no styles will be loaded.

3.  **`Site.Client/vite.config.ts`**
    -   **Role**: Configures the Vite dev server and production build.
    -   **Potential Error**: An incorrect `server.hmr.clientPort` setting will break the WebSocket connection, and HMR will fail.

4.  **`deployment/nginx/default.conf.template`**
    -   **Role**: Routes requests for static assets to the correct service.
    -   **Potential Error**: Incorrect or missing `location` blocks for `/fonts/` or `/images/` will result in 404 errors for those assets.

By understanding the roles of these four key files, any future styling issue can be diagnosed and resolved efficiently.