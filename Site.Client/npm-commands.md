# 🚀 Managing Your Project

These commands are for the day-to-day tasks of running and building your application. They are shortcuts for the scripts defined in your package.json.

    npm run dev

        Starts the Vite development server with hot-reloading. This is what you'll use while actively coding your React components.

    npm run build

        Checks your code for TypeScript errors and then builds the optimized, production-ready files into the ../wwwroot/react-dist folder for your C# application to use.

    npm run storybook

        Starts the Storybook development server. Use this to build, view, and test your components in isolation.

    npm run lint

        Analyzes your code with ESLint to find and report potential errors or style issues.

## 📦 Managing Dependencies (Packages)

Use these commands to add, update, or remove libraries from your project.

### Installation

    npm install

        Installs all dependencies listed in your package.json. Run this after cloning a project or when a teammate adds a new package.

    npm install <package-name>

        Adds a new package as a production dependency (e.g., npm install dayjs).

    npm install <package-name> -D

        Adds a new package as a development dependency (e.g., a testing tool). The -D is a shortcut for --save-dev.

### Updating

    npm outdated

        Checks for newer versions of your installed packages and shows you a list of what's available to update.

    npm update

        Updates your packages to the latest versions allowed by the rules in your package.json.

### Removing

    npm uninstall <package-name>

        Removes a package from your node_modules folder and updates your package.json.

## 🛠️ Maintenance & Security

These commands help you keep your project healthy and secure.

    npm audit

        Scans your project for known security vulnerabilities in your dependencies and provides a report.

    npm audit fix

        Attempts to automatically fix any reported vulnerabilities by updating packages to secure versions.

    npm list --depth=0

        Shows you a clean, top-level list of all the packages you have installed.