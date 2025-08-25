# AI Developer Handoff Document

This document summarizes the key findings and critical configurations discovered during the recent debugging session. The local development environment has several interconnected services, and understanding their interactions is crucial for effective troubleshooting.

## 1. Core Architecture (Docker Compose)

The local environment is orchestrated by `docker-compose.yml` and consists of several key services:

-   **`nginx`**: The reverse proxy and single entry point for all browser traffic at `http://localhost:8080`. It routes requests to the appropriate backend service.
-   **`roleplayersguild`**: The main ASP.NET Core backend. It serves Razor Pages, handles business logic, and contains the Web API.
-   **`site-client-dev`**: The Vite development server. It serves all JavaScript/TypeScript, handles Hot Module Replacement (HMR), and processes SCSS files.
-   **`minio`**: An S3-compatible object storage service used for local development to simulate AWS S3 for image uploads.

## 2. Vite & ASP.NET Core Integration

This is the most complex part of the system.

-   **Request Flow**: The ASP.NET Core backend renders the initial HTML. It uses the `ViteManifestService.cs` to inject `<script>` tags into the page. These script tags point to the Vite dev server.
-   **Critical Configuration**:
    -   `appsettings.json` -> `Vite.DevServerProxy`: This **must** be set to `/vite-dev`. This relative path is used by the backend to create the script URLs.
    -   `vite.config.ts` -> `server.hmr.clientPort`: This **must** be set to the public-facing NGINX port (e.g., 8080). This tells the Vite client in the browser how to connect back for HMR.
    -   `nginx/default.conf.template`: Must contain a `location /vite-dev/` block that proxies all requests on that path to the `site-client-dev` service.

**Failure in this area results in React components not loading (the "infinite loading" issue).**

## 3. Image Handling (Minio S3)

-   **Upload Flow**: The React frontend uploads an image to the `CharactersApiController`, which uses the `ImageService` to save the file to the Minio bucket.
-   **Serving Flow**: The `UrlProcessingService` generates a URL for the image. In development, this URL is a relative path (e.g., `/images/UserFiles/...`). The browser requests this path, and NGINX proxies the request to the `minio` service to retrieve the image.
-   **Critical Configuration**:
    -   `UrlProcessingService.cs`: Must be environment-aware. It checks `_env.IsDevelopment()` to decide whether to generate a local relative path or a production CDN URL.
    -   `nginx/default.conf.template`: Requires a specific `location /images/UserFiles/` block that proxies to the `minio` service. This rule must appear *before* the more general `/images/` rule.
    -   `docker-compose.yml`: The `nginx` service must receive the `${S3_PORT}` environment variable to correctly build its configuration.

**Failure in this area results in 404 or Access Denied errors for user-uploaded images.**

## 4. User Activity Tracking (Heartbeat)

-   **Mechanism**: This is **not** a WebSocket. It is a standard `fetch` POST request sent every 15 minutes by `user-activity-heartbeat.ts`.
-   **Endpoint**: `POST /api/userapi/activity`, handled by `UserApiController`.
-   **Critical Bug Pattern**: The database queries that write and read the `LastAction` timestamp are sensitive to time zone handling in PostgreSQL.
    -   **Correct**: `SET "LastAction" = NOW()`
    -   **Incorrect**: `SET "LastAction" = NOW() AT TIME ZONE 'UTC'`
    -   This bug was found and fixed in both `UserDataService.cs` (for writing) and `CharacterDataService.cs` (for reading). Be wary of it if other activity-related queries are created.

## 5. React Component Loading

-   **Mechanism**: The `script-loader.ts` script runs on page load. It searches the DOM for elements with a `data-component="ComponentName"` attribute.
-   **Props**: Component props are passed via a `data-props='{...}'` attribute containing a JSON string.
-   **Common Failure**: If a React component does not load, the first step is to verify that its placeholder `<div>` in the corresponding `.cshtml` file has the correct `data-component` and `data-props` attributes.

## 6. ASP.NET Core Middleware

-   **CORS**: The `app.UseCors()` middleware call in `Program.cs` **must** be placed after `app.UseRouting()` and before the endpoint mapping calls (e.g., `app.MapDefaultControllerRoute()`). Incorrect placement will cause a 500 error on API endpoints that have CORS metadata.