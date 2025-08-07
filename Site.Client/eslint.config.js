import globals from "globals";
import js from "@eslint/js";
import tseslint from "typescript-eslint";
import reactPlugin from "eslint-plugin-react";
import hooksPlugin from "eslint-plugin-react-hooks";
import refreshPlugin from "eslint-plugin-react-refresh";

export default [
  // 1. Global ignores
  {
    ignores: ["dist/**"],
  },

  // 2. Base configurations for all JS/TS files
  js.configs.recommended,
  ...tseslint.configs.recommended,

  // 3. React-specific configuration
  {
    files: ["**/*.{js,jsx,ts,tsx}"],
    plugins: {
      react: reactPlugin,
      "react-hooks": hooksPlugin,
      "react-refresh": refreshPlugin,
    },
    languageOptions: {
      globals: {
        ...globals.browser,
      },
      parserOptions: {
        ecmaFeatures: { jsx: true },
      },
    },
    settings: {
      react: {
        // Tells eslint-plugin-react to automatically detect the version of React to use.
        version: "detect",
      },
    },
    rules: {
      // Standard React rules
      ...reactPlugin.configs.recommended.rules,
      ...reactPlugin.configs["jsx-runtime"].rules,
      // React Hooks rules
      ...hooksPlugin.configs.recommended.rules,
      // React Refresh rule
      "react-refresh/only-export-components": "warn",
      // Your custom rule (adapted for typescript-eslint)
      "no-unused-vars": "off",
      "@typescript-eslint/no-unused-vars": ["error", { "varsIgnorePattern": "^_" }],
    },
  },
])
