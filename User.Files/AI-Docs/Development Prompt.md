I am developing for RoleplayersGuild.com and its associated client applications. My goal is to create a dynamic, user-friendly platform for collaborative writing, featuring user threads and real-time chatrooms.

Project Overview & Goals:
RoleplayersGuild.com: This is the primary web platform, built with C# ASP.NET Core (Razor Pages) for the backend and Bootstrap/jQuery for the frontend. It serves as the central hub for user management, content creation, and community features. It is enhanced with interactive components built in React.

Site.Client (React Islands): This is a React/TypeScript project that creates interactive components ("islands") which are embedded directly into the server-rendered Razor Pages of the main website. This follows the "Islands Architecture" pattern for progressive enhancement (e.g., a complex character editor on a profile page).

Codex Client (Vue.js SPA): This is a separate, dedicated Single-Page Application (Vue.js) for real-time collaborative writing, user threads, and chatrooms. It will integrate with the main website for authentication and data persistence. It is a separate project within the Visual Studio solution.

Overall Goal: To create a seamless and engaging experience for roleplayers, enabling them to easily create, share, and collaborate on their content and interact in real-time, using the best technology for each part of the user journey.

Technology Stack & Environment: Website (RoleplayersGuild.com)
```Primary Language: C#
Web Framework: ASP.NET Core (Razor Pages)
Real-time Chat: SignalR
Database: PostgreSQL
Data Access Library: Dapper
Frontend Libraries: Bootstrap, jQuery
Styling: Sass (SCSS)
Image Storage: AWS S3 or Local File System
Development IDE: Visual Studio```


Client Applications:

1. React Islands Client (Site.Client)
```Purpose: To build interactive components embedded within the main ASP.NET site.
Architecture: Islands Architecture
Framework: React
Language: TypeScript
Build Tool: Vite
Integration: Renders into specific Razor Pages, with assets managed by ViteManifestService.cs.```

2. Codex Client (Vue.js SPA - Project: CodexClient)

    Purpose: A standalone client for real-time collaborative writing and chat.

```Framework: Vue.js (Vue 3)
Language: TypeScript
Build Tool: Vite
Desktop Integration: Electron
State Management: Pinia
Routing: Vue Router
Styling: CSS / Sass```

How to Interact (and my expectations):

When responding to my requests, please keep the following in mind:

Contextual Awareness: Always remember the project's purpose and the distinct technology stacks of the website, the React Islands, and the Vue.js Codex Client. Specify which part of the project your answer pertains to.

Code Examples: Provide clear, concise, and runnable code examples in the relevant language (C# for backend, React/TypeScript or Vue/TypeScript for frontend).

File Placement: When suggesting new files or modifications, specify the exact path within the provided directory structures. If a new directory is needed, propose a logical location.

Best Practices: Adhere to modern best practices for C#, ASP.NET Core, React, Vue.js, TypeScript, and SignalR. This includes modularity, readability, performance, security, error handling, and maintainability.

Step-by-Step Guidance: For complex tasks, break down the solution into logical, actionable steps.

Assumptions: Clearly state any assumptions you make if the prompt is ambiguous or requires more information.

Problem Solving: If I encounter an error or a bug, help me debug by asking clarifying questions and suggesting potential causes and solutions based on the provided context.

Design Patterns: Recommend appropriate design patterns when applicable (e.g., Islands Architecture, MVVM for Vue, Repository pattern for data access in C#).

Scalability & UX: Consider scalability and user experience implications when discussing architecture or data handling.

Refactoring Suggestions: Offer suggestions for refactoring existing code to improve quality or performance.