# Site.Client NPM Commands and Tooling

This document provides an overview of the available `npm` scripts and the automated tooling configured for the `Site.Client` project.

## Available Scripts

You can run any of these scripts from the `Site.Client` directory using the `npm run <script_name>` command.

| Script            | Description                                                                                                                                                      |
| :---------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `npm run dev`     | Starts the Vite development server with Hot Module Replacement (HMR). This is the primary command you will use during development.                               |
| `npm run build`   | Compiles, bundles, and minifies all front-end assets (TypeScript, React, SASS, etc.) for production. The output is placed in the `wwwroot/react-dist` directory. |
| `npm run check`   | Runs `depcheck` to analyze your `package.json` and identify any unused or missing dependencies.                                                                  |
| `npm run lint`    | Runs ESLint to statically analyze your code for potential errors and style issues.                                                                               |
| `npm run preview` | Starts a local server to preview the production build from the `wwwroot/react-dist` directory.                                                                   |

## Automated Tooling

This project is equipped with a professional-grade tooling setup to automate code formatting and quality checks.

### Prettier

- **Purpose:** An opinionated code formatter that ensures a consistent code style across the entire project.
- **Configuration:** Rules are defined in the `.prettierrc.json` file.
- **Usage:** Prettier is run automatically on every commit. You can also run it manually with `npx prettier --write .`.

### Husky & lint-staged

- **Purpose:** This combination automates code quality checks before any code is committed to the repository.
- **Configuration:** The `lint-staged` configuration is in `package.json`, and the Husky hook is in the `.husky` directory.
- **Usage:** When you run `git commit`, Husky will trigger `lint-staged`, which will then run ESLint and Prettier on all the files you have staged for commit. This process is completely automatic. If there are any linting errors, the commit will be aborted, allowing you to fix the issues before trying again.
