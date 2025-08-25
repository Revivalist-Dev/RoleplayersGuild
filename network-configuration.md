# Network Configuration Guide

This document provides a comprehensive overview of the networking and service architecture for the RoleplayersGuild application in a local development environment.

## Table of Contents
- [Network Configuration Guide](#network-configuration-guide)
  - [Table of Contents](#table-of-contents)
  - [Architecture Overview](#architecture-overview)
  - [Service Responsibilities](#service-responsibilities)
    - [NGINX](#nginx)
    - [RoleplayersGuild](#roleplayersguild)
    - [Site.Client](#siteclient)
  - [Request Flow](#request-flow)
    - [1. Initial Page Load (e.g., `/Dashboard`)](#1-initial-page-load-eg-dashboard)
    - [2. Frontend Asset Load (e.g., `/src/main.tsx`)](#2-frontend-asset-load-eg-srcmaintsx)
  - [Key Configuration Files](#key-configuration-files)
  - [Troubleshooting History \& Resolutions](#troubleshooting-history--resolutions)
    - [**Asset Pipeline Refactor (August 2025)**](#asset-pipeline-refactor-august-2025)

---

## Architecture Overview

The local development environment runs within a Docker network and consists of a reverse proxy that directs traffic to the appropriate backend service.

1.  **NGINX**: The single entry point for all browser traffic, listening on `localhost:8080`. It acts as the main reverse proxy.
2.  **RoleplayersGuild (ASP.NET Core Application)**: The main backend application that serves Razor Pages, API logic, and static images.
3.  **Site.Client (Vite Dev Server)**: A Node.js service that serves frontend assets (JS/TS/CSS) during development on port `5173`.

## Service Responsibilities

### NGINX
- **Role**: Edge Proxy.
- **Entry Point**: `localhost:8080`.
- **Responsibilities**:
    - Inspects the URL of each incoming request.
    - Forwards requests starting with `/react-dist/` to the `site-client-dev` service.
    - Forwards all other traffic to the `roleplayersguild` service.
    - Handles WebSocket upgrade headers, which are essential for Vite's Hot Module Replacement (HMR).

### RoleplayersGuild
-   **Role**: Backend Application Server.
-   **Responsibilities**:
    -   Serves the main HTML structure via Razor Pages.
    -   Handles all business logic and API requests (e.g., database interactions, user authentication).
    -   Serves static images directly from the `Site.Client/public` directory.
    -   Injects the necessary `<script>` and `<link>` tags into the Razor Pages to load the frontend assets from the Vite server.

### Site.Client
-   **Role**: Frontend Development Server.
-   **Responsibilities**:
    -   Serves all hot-reloading assets: JavaScript/TypeScript (React) and CSS/SCSS.
    -   Provides a WebSocket server for Hot Module Replacement, allowing for instant updates in the browser when frontend code changes.

## Request Flow

### 1. Initial Page Load (e.g., `/Dashboard`)
1.  **Browser** -> `http://localhost:8080/Dashboard`
2.  **NGINX** receives the request. The path does not start with `/react-dist/`, so it forwards the request to the `roleplayersguild` service.
3.  **RoleplayersGuild** receives the request, renders the `Dashboard.cshtml` Razor Page, and returns the HTML to the browser. This HTML contains tags like `<script type="module" src="/react-dist/src/main.tsx"></script>` and `<link rel="stylesheet" href="/react-dist/src/Site.Styles/scss/site.scss">`.

### 2. Frontend Asset Load (e.g., `/src/main.tsx`)
1.  **Browser** parses the HTML and requests the script at `http://localhost:8080/react-dist/src/main.tsx`.
2.  **NGINX** receives the request. The path starts with `/react-dist/`, so it forwards the request to the `site-client-dev:5173` service.
3.  **Site.Client (Vite)** receives the request and serves the `main.tsx` file.

## Key Configuration Files

-   **`docker-compose.yml`**: Defines the services, their networking, and the commands to run them.
-   **`deployment/nginx/default.conf.template`**: Configures the routing rules for the NGINX reverse proxy.
-   **`Site.Client/vite.config.ts`**: Configures the Vite development server.
-   **`RoleplayersGuild/Program.cs`**: Configures the main backend application, including its static file serving rules.
-   **`RoleplayersGuild/Site.Services/ViteManifestService.cs`**: Generates the `<script>` and `<link>` tags in the Razor Pages.

## Troubleshooting History & Resolutions

This section details a history of resolved issues, which can be valuable for diagnosing future problems.

---
### **Asset Pipeline Refactor (August 2025)**

A series of cascading issues related to asset serving and hot reload reliability led to a full-scale refactoring of the development environment.

1.  **Problem**: Browser getting `404 Not Found` for images and CSS, and `NS_ERROR_CONNECTION_REFUSED` for a .NET-injected WebSocket script.
    -   **Root Cause**: An overly complex architecture where NGINX was responsible for serving static files it didn't have access to, and a conflict between .NET's browser refresh and Vite's HMR.
    -   **Resolution**:
        1.  Simplified the architecture by making the `RoleplayersGuild` app responsible for serving its own static images from `Site.Client/public`.
        2.  Disabled the .NET browser refresh feature (`DOTNET_WATCH_SUPPRESS_BROWSER_REFRESH=true`) to eliminate the conflict.
        3.  Refactored the `ViteManifestService` to be the single source of truth for injecting asset tags, ensuring it correctly points to the Vite dev server for CSS and JS.

2.  **Problem**: Stylesheet being loaded with the wrong MIME type (`text/javascript`).
    -   **Root Cause**: The `ViteManifestService` was being called incorrectly from a Razor view, causing it to generate a `<link>` tag pointing to a JavaScript file (`main.tsx`).
    -   **Resolution**: Made the `RenderViteStyles` method in the C# service more robust. It now ignores any parameters passed to it and always uses a hardcoded, correct path to the stylesheet entry point (`site.scss`), making it immune to incorrect calls.

3.  **Problem**: .NET Hot Reload (`dotnet watch`) was not detecting file changes inside the Docker container.
    -   **Root Cause**: The default file watching mechanism relies on OS notifications that are often unreliable when passed into a Docker container from the host.
    -   **Resolution**: Enabled polling mode for the file watcher by setting the `DOTNET_USE_POLLING_FILE_WATCHER=true` environment variable in `docker-compose.yml`. This makes `dotnet watch` actively check for changes every few seconds, ensuring reliable hot reloads.