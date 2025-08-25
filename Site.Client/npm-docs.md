# Front-End Package Documentation (`Site.Client`)

This document provides an overview of the key NPM packages used in the `Site.Client` project, explaining their purpose and why they were chosen.

## Core Frameworks

### `react` & `react-dom`

- **Purpose:** The core libraries for building the user interface with React components. `react` contains the logic for creating components, while `react-dom` is responsible for rendering them in the browser.

### `vite`

- **Purpose:** A next-generation front-end tooling system that serves as our development server and build tool. It provides extremely fast Hot Module Replacement (HMR) for a superior development experience and bundles all assets (TypeScript, SASS, images) for production.

### `typescript`

- **Purpose:** A superset of JavaScript that adds static types. It helps catch errors early in development, improves code quality, and makes the codebase easier to read and maintain.

## State Management & Data Fetching

### `@tanstack/react-query`

- **Purpose:** A powerful library for managing server state. It handles fetching, caching, synchronizing, and updating data from the API.
- **Why it's used:** It replaces manual `axios` calls with a robust, hook-based system (`useQuery`, `useMutation`) that simplifies data fetching logic, reduces boilerplate, and provides advanced features like caching, automatic refetching, and loading/error state management out of the box.

### `react-hook-form`

- **Purpose:** A performant and flexible library for building forms.
- **Why it's used:** Ideal for complex forms like the Character Editor. It minimizes re-renders for better performance and simplifies state management, validation, and submission handling.

### `zustand`

- **Purpose:** A small, fast, and scalable state-management solution using a hook-based API.
- **Why it's used:** While React Query is perfect for managing *server state*, Zustand is used for managing global *client-side UI state*. It provides a simple way to share state (e.g., theme settings, modal visibility) across different "React Islands" or components that are not directly connected, without causing unnecessary re-renders or requiring complex boilerplate like Context providers.

### `clsx`

- **Purpose:** A tiny (239B) utility for conditionally constructing `className` strings.
- **Why it's used:** It provides a clean and readable way to apply CSS classes based on component state or props, avoiding messy template literals.

### `axios`

- **Purpose:** A promise-based HTTP client for making API requests to the `RPGateway`. It is the underlying fetcher used by TanStack Query.

## Code Quality & Linting

### `@tanstack/eslint-plugin-query`

- **Purpose:** An official ESLint plugin for TanStack Query.
- **Why it's used:** Enforces best practices for TanStack Query to prevent common mistakes, such as forgetting to `await` calls or using incorrect query keys.

## UI & Styling

### `bootstrap` & `bootstrap-icons`

- **Purpose:** The primary CSS framework for the site's overall layout and styling. `bootstrap-icons` provides a set of high-quality SVG icons.

### `sass`

- **Purpose:** A CSS preprocessor that adds features like variables, nesting, and mixins to CSS, making stylesheets more maintainable and organized.

## Layout

### `packery` & `imagesloaded`

- **Purpose:** Used for creating dynamic, grid-based layouts (like a photo gallery) where items can be intelligently packed together. `imagesloaded` ensures that layouts are calculated only after all images have been loaded.
