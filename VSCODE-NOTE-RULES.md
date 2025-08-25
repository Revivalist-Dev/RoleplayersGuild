# VS Code Notes & Highlighting: Implementation Rules

This document provides the official guidelines for installing and using the project's standardized note-taking and comment-highlighting extensions in Visual Studio Code.

## 1. Core Rule: Extension Installation

The previously used `Project Notes + TODO Highlighter` extension is **deprecated** and must be uninstalled. It has been replaced by two new, incompatible extensions that must be installed.

-   [ ] **Uninstall:** `Project Notes + TODO Highlighter`
-   [ ] **Install:** **Project Notes** (Publisher: T.S. Studio)
-   [ ] **Install:** **Comment Highlighter** (Publisher: T.S. Studio)

**IMPORTANT:** Running the old extension simultaneously with the new ones will cause issues.

## 2. Initial Project Setup

To ensure notes are handled correctly and do not clutter the project repository, the following one-time setup steps are required.

-   [ ] **Update `.gitignore`:** The `Project Notes` extension creates a local notes folder named `.pnotes` by default. This folder must be added to the project's `.gitignore` file to prevent notes from being committed to source control.
    ```
    # .gitignore

    # VS Code Project Notes
    .pnotes/
    ```

-   [ ] **Enable Comment Snippets:** To use the tag snippets provided by `Comment Highlighter`, you must enable quick suggestions for comments in your VS Code `settings.json` file.
    -   **Action:** Open your `settings.json` (Command Palette: `Preferences: Open User Settings (JSON)`) and add the following configuration:
        ```json
        "editor.quickSuggestions": {
          "comments": true
        }
        ```

## 3. Usage Guidelines

### Using Project Notes

This system is for creating and linking to project-specific documentation directly from the code.

-   **Creating/Linking to a Note:**
    -   In any file, create a comment using the format: `// Project File: MyNoteName.md`
    -   Place your cursor on that line.
    -   Open the Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`).
    -   Run the command: `Project Notes: Open Note File Link`.
    -   This will either create `MyNoteName.md` inside the `.pnotes` folder or open it if it already exists.

-   **Global Notes:** For notes that are not specific to this project, use the format `// Global File: MyGlobalNote.md`. The same command will open them from your global notes folder.

### Using Comment Highlighter

This system is for tagging comments to make them easily identifiable and searchable.

-   **Standard Tags:** The following tags are highlighted by default. They are case-insensitive and the trailing colon is optional.
    -   `TODO`: For outstanding tasks.
    -   `FIXME`: For code that is broken and needs fixing.
    -   `BUG`: To mark a known bug.
    -   `REVIEW`: For code that needs a peer review.
    -   `NOTE`: For general notes or explanations.
    -   `IDEA`: For new feature or improvement ideas.
    -   `RESEARCH`: To mark areas that require further investigation.
    -   `OPTIMIZE`: For code that works but is inefficient.
    -   `HACK`: For temporary or sub-optimal workarounds.
    -   `CHANGED`: To document a recent change.
    -   `DEBUG`: For temporary debugging statements.
    -   `TEMP`: For temporary code.

-   **Tag Browser:** Use the "Tag Browser" panel in the VS Code sidebar to view a list of all tagged comments in the project. Clicking a tag will navigate you directly to its location in the code.