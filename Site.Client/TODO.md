# Frontend Package TODO List (`Site.Client`)

This document serves as a checklist for evaluating and potentially integrating new npm packages to enhance the `Site.Client` project.

## UI & Component Libraries

-   [ ] **`@headlessui/react`**: Evaluate for building new, fully accessible UI components like modals and dropdowns.
-   [ ] **`framer-motion`**: Consider for adding complex animations or page transitions.
-   [ ] **`react-select`**: Evaluate as a replacement for native `<select>` elements where multi-select or async search is needed.
-   [ ] **`react-hot-toast`**: Implement for user-facing notifications (e.g., "Profile saved successfully").
-   [ ] **`react-beautiful-dnd`**: Consider for features requiring drag-and-drop functionality.
-   [ ] **`recharts`**: Evaluate if any data visualization or charting features are planned.
-   [ ] **`react-modal`**: Use for simple, accessible modal dialogs.
-   [ ] **`react-tooltip`**: Implement for adding tooltips to icons or complex UI elements.
-   [ ] **`downshift`**: Use as a primitive for building custom, accessible autocomplete or combobox components.
-   [ ] **`tiptap`**: Investigate as a potential foundation for a new rich-text editor.

## State Management & Data Validation

-   [ ] **`jotai`**: Explore as an alternative to Zustand for highly granular, atomic state management.
-   [ ] **`valtio`**: Consider for state-heavy components where a mutable-style API might simplify logic.
-   [ ] **`immer`**: Integrate with `useState` or `useReducer` hooks that manage complex, nested state objects.
-   [ ] **`zod`**: Adopt for all new form and API response validation to ensure type safety.
-   [ ] **`graphql-request`**: Consider if a GraphQL API is introduced and simple client-side interaction is needed.

## Testing

-   [ ] **`vitest`**: Implement as the primary unit and component testing framework.
-   [ ] **`@testing-library/react`**: Adopt as the standard for writing all component tests.
-   [ ] **`@testing-library/jest-dom`**: Add to the testing setup to provide better DOM-based assertions.
-   [ ] **`msw` (Mock Service Worker)**: Set up to mock API responses for robust integration and E2E tests.
-   [ ] **`playwright`**: Implement for critical user-flow end-to-end testing (e.g., registration, character creation).

## Utilities & Developer Experience

-   [ ] **`date-fns`**: Refactor existing date logic to use `date-fns` for consistency and tree-shakability.
-   [ ] **`lodash-es`**: Use for any complex data manipulation not easily handled by native JS methods.
-   [ ] **`@tanstack/react-query-devtools`**: Add to the development setup to aid in debugging server state.
-   [ ] **`storybook`**: Set up to create an isolated development environment for all shared UI components.
-   [ ] **`@welldone-software/why-did-you-render`**: Use during development to diagnose and fix performance issues related to re-renders.
-   [ ] **`i18next` & `react-i18next`**: Investigate if multi-language support becomes a platform requirement.
-   [ ] **`react-helmet-async`**: Implement to manage page titles and meta tags for better SEO and user experience.
-   [ ] **`class-variance-authority`**: Consider adopting if building a formal design system with component variants.
-   [ ] **`tailwind-merge`**: Use in conjunction with `clsx` if you adopt Tailwind CSS and need to merge classes.
-   [ ] **`yjs`**: Investigate as the foundational technology for any real-time collaborative editing features.