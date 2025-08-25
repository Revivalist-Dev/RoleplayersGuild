# Docker Compose Commands

This document outlines the commands for running the RoleplayersGuild application stack using Docker Compose. The setup uses profiles to separate services for different environments.

## Core Concepts

- **Profiles:** We use Docker Compose profiles to define subsets of services. This allows us to run the application in different configurations (e.g., `dev` vs. `prod`) without needing separate `docker-compose` files.
- **Default Services:** The core backend services (`roleplayersguild`, `rpgateway`, `minio`) are defined without a profile, meaning they will start by default with any `docker-compose up` command.

---

## Development Environment

This is the standard mode for local development. It runs the core backend services and the Vite development server for the `Site.Client` front-end, which provides Hot Module Replacement (HMR) for instant feedback on code changes.

**Command:**
```shell
docker-compose --profile dev up
```

**Services Started:**
- `roleplayersguild` (ASP.NET Core Backend)
- `rpgateway` (Ocelot API Gateway)
- `minio` (Local S3-compatible storage)
- `createbuckets` (MinIO utility)
- `site-client-dev` (Vite Dev Server)

---

## Production Environment Simulation

This mode simulates a production environment by building and serving the optimized, static front-end assets with Nginx. This is useful for testing the final build output before deployment.

**Command:**
```shell
docker-compose --profile prod up
```

**Services Started:**
- `roleplayersguild` (ASP.NET Core Backend)
- `rpgateway` (Ocelot API Gateway)
- `minio` (Local S3-compatible storage)
- `site-client-prod` (Nginx serving static front-end assets)

---

## Other Useful Commands

- **Run in Detached Mode:** Add the `-d` flag to run the containers in the background.
  ```shell
  docker-compose --profile dev up -d
  ```

- **Force a Rebuild:** If you make changes to a `Dockerfile`, use the `--build` flag to force Docker to rebuild the images.
  ```shell
  docker-compose --profile dev up --build
  ```

- **Stop Services:** To stop and remove the running containers.
  ```shell
  docker-compose down
  ```

- **View Logs:** To view the logs from all running containers.
  ```shell
  docker-compose logs -f
  ```
  Or for a specific service:
  ```shell
  docker-compose logs -f roleplayersguild