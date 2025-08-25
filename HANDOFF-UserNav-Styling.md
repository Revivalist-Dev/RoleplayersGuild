# Handoff Document: UserNav Architecture (Vertical Flyout)

This document provides a comprehensive overview of the current architecture for the UserNav component, its styling, and its interactive scripts.

---

## 1. Current Status & Architecture Overview

The site now uses a single, unified UserNav component: a vertical flyout panel that can be docked to the left or right side of the viewport. The previous horizontal navigation bar has been completely removed from the codebase.

The UserNav's state (collapsed status, position, and docked side) is persisted in the user's `localStorage`.

-   **Styling**: Managed by Vite with HMR. Source files are in `Site.Assets/styles/scss/`.
-   **Scripts**: All interaction logic is handled by dependency-free TypeScript.
-   **Rendering**: The component is rendered as a ViewComponent in the main `_Layout.cshtml` file.

---

## 2. Key Files for the UserNav Component

### C# & Razor (The Backend Structure)

1.  **View Component (`Site.Services/ViewComponents/UserNavViewComponent.cs`)**:
    -   **Role**: The C# class responsible for fetching all necessary data for the UserNav (e.g., notification counts, quick links) and passing it to the view.

2.  **Component View (`Site.Directory/Shared/Components/UserNav/Default.cshtml`)**:
    -   **Role**: Contains the outer HTML structure for the flyout panel, including the header with the toggle, dashboard, and drag handle controls. It renders the `_UserNav.cshtml` partial inside its body.

3.  **Partial View (`Site.Directory/Shared/_UserNav.cshtml`)**:
    -   **Role**: Contains the main content of the UserNav panel, including the Bootstrap accordion for the "Personal", "Community", and other sections.

### SCSS (The Styling)

1.  **Main Stylesheet (`Site.Assets/styles/scss/components/_UserNav.scss`)**:
    -   **Role**: Contains all styles for the flyout panel, including positioning, colors, themes, and the logic for the collapsed state and the repositioning of the controls when the panel is open.

### TypeScript (The Interactivity)

1.  **Main Logic Script (`Site.Client/src/Site.Scripts/UserNav.ts`)**:
    -   **Role**: This is the single source of truth for all UserNav interactivity. It manages the component's state, handles drag-and-drop, processes clicks on the toggle and side-switching buttons, and saves the state to `localStorage`.

### Global Configuration

-   **Main Layout (`Site.Directory/Shared/_Layout.cshtml`)**: Renders the `UserNav` view component on every page.
-   **Script Entry Point (`Site.Client/src/main.tsx`)**: Imports and executes the `UserNav.ts` script to activate the component.

---

## 3. Current Tasks & Next Steps

This document has been updated to reflect the significant refactoring and simplification of the UserNav component. The primary task was to eliminate the dual-navigation system and consolidate all functionality into the vertical flyout panel.

**Completed Work:**
-   Removed all files and logic related to the `UserNavHorizontal` component.
-   Renamed all `UserNavVertical` files to a standard `UserNav`.
-   Consolidated all TypeScript logic into a single `UserNav.ts` script.
-   Fixed numerous layout and styling bugs related to the component's appearance and behavior.
-   Resolved several unrelated build and runtime errors discovered during testing.

**Next Steps:**
-   The component is now stable and functional. The next developer can proceed with any further feature requests or styling adjustments.