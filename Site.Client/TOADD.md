# Frontend Package & Integration Roadmap (`Site.Client`)

This document outlines potential npm packages that could be added to the `Site.Client` project to enhance UI, state management, testing, and overall developer experience.

## UI & Component Libraries

1.  **`@headlessui/react`**: A set of completely unstyled, fully accessible UI components (like modals, dropdowns) designed to integrate beautifully with your existing styles.
2.  **`framer-motion`**: A powerful and simple animation library for creating fluid and delightful user interfaces.
3.  **`react-select`**: A flexible and feature-rich replacement for the native select input, with support for multi-select, async search, and tagging.
4.  **`react-hot-toast`**: An elegant and customizable library for creating toast notifications.
5.  **`react-beautiful-dnd`**: A popular and accessible library for creating drag-and-drop interfaces.
6.  **`recharts`**: A composable charting library built on React components for creating beautiful data visualizations.
7.  **`react-modal`**: A simple and accessible modal dialog component.
8.  **`react-tooltip`**: A versatile and customizable tooltip component.
9.  **`downshift`**: A set of primitive hooks for building powerful and accessible autocomplete, combobox, and select components.
10. **`tiptap`**: A headless, framework-agnostic rich-text editor that would be a powerful foundation for a modern character sheet or post editor.

## State Management & Data Validation

11. **`jotai`**: An atomic state management library. It's a simple and flexible alternative to Zustand for managing state in a more granular way.
12. **`valtio`**: A proxy-based state management library that provides a simple, mutable-style API while ensuring React's rendering stays optimized.
13. **`immer`**: A utility for working with immutable state in a more convenient way. It's often used with `useState` or `useReducer` to simplify complex state updates.
14. **`zod`**: A TypeScript-first schema declaration and validation library. Perfect for validating form data, API responses, and ensuring data consistency throughout the application.
15. **`graphql-request`**: A minimal and lightweight GraphQL client, perfect for simple interactions with a GraphQL API.

## Testing

16. **`vitest`**: A testing framework designed specifically for Vite projects. It's incredibly fast and has a Jest-compatible API.
17. **`@testing-library/react`**: The standard for writing user-centric tests for React components that are robust and maintainable.
18. **`@testing-library/jest-dom`**: Adds custom Jest matchers for asserting on DOM state (e.g., `toBeInTheDocument()`).
19. **`msw` (Mock Service Worker)**: An API mocking library that intercepts network requests at the network level, allowing you to test your application against a mock API in a highly realistic way.
20. **`playwright`**: A modern and powerful end-to-end testing framework for testing your application's behavior in a real browser.

## Utilities & Developer Experience

21. **`date-fns`**: A modern, lightweight, and tree-shakable library for date manipulation.
22. **`lodash-es`**: The modern, ES module version of the popular Lodash utility library, ensuring you only bundle the functions you use.
23. **`@tanstack/react-query-devtools`**: The official devtools for TanStack Query, providing invaluable insight into your queries, mutations, and cache.
24. **`storybook`**: A tool for building, viewing, and testing your UI components in isolation, creating a living component encyclopedia.
25. **`@welldone-software/why-did-you-render`**: A library that monkey-patches React to notify you about potentially avoidable re-renders.
26. **`i18next` & `react-i18next`**: The standard libraries for adding internationalization (i18n) to your application, allowing you to support multiple languages.
27. **`react-helmet-async`**: A component to manage your document head, allowing you to set page titles, meta tags, etc., on a per-component basis.
28. **`class-variance-authority`**: A utility for creating type-safe and composable component variants, excellent for a design system.
29. **`tailwind-merge`**: A utility function to intelligently merge Tailwind CSS classes without style conflicts.
30. **`yjs`**: A high-performance CRDT (Conflict-free Replicated Data Type) library, which is the foundation for building real-time collaborative features like a shared text editor.