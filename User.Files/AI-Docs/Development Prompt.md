I am developing for RoleplayersGuild.com and its associated Vue.js application, the "Codex Client." My goal is to create a dynamic, user-friendly platform for collaborative writing, featuring user threads and real-time chatrooms.


Project Overview & Goals:
```RoleplayersGuild.com: This is the primary web platform, built with C# ASP.NET Core (Razor Pages) for the backend and Bootstrap/jQuery for the frontend. It will serve as the central hub for user management, content creation (articles, characters, stories, etc.), community features, and integration with the Codex Client.
Codex Client: This is a dedicated live web client (Vue.js application) for real-time collaborative writing, user threads, and chatrooms. It integrates with the main website for authentication and data persistence.

Overall Goal: To create a seamless and engaging experience for roleplayers, enabling them to easily create, share, and collaborate on their content and interact in real-time.```


Technology Stack & Environment:
```Website: RoleplayersGuild.com
Primary Language: C#
Web Framework: ASP.NET Core (Razor Pages)
Real-time Chat: SignalR
Current Database: PostgreSQL
Data Access Library: Dapper
Frontend Libraries: Bootstrap, jQuery
Styling: Sass (SCSS)
Image Storage: AWS S3 or Local File System
Development IDE: Visual Studio```


Vue Application: "Codex Client":
```Framework: Vue.js (likely Vue 3 given vite.config.ts, tsconfig.json, and the file structure)
Language: TypeScript
Build Tool: Vite
Desktop Integration: Electron (indicated by the electron directory and related files)
State Management: Pinia (inferred from useChatStore.ts, useSettingsStore.ts, useUserStore.ts naming convention)
Routing: Vue Router (indicated by src/router/index.ts)
Styling: CSS (with App.Assets and App.Styles directories suggesting structured CSS/Sass, also indicated by the website's use of Sass)```


How to Interact (and my expectations):
```When responding to my requests, please keep the following in mind:
Contextual Awareness: Always remember the project's purpose and the distinct technology stacks of the website and the Codex Client. Specify which part of the project your answer pertains to.
Code Examples: Provide clear, concise, and runnable code examples in the relevant language (C# for backend, Vue/TypeScript/JavaScript for frontend).
File Placement: When suggesting new files or modifications, specify the exact path within the provided directory structures. If a new directory is needed, propose a logical location.
Best Practices: Adhere to modern best practices for C#, ASP.NET Core, Vue.js, TypeScript, and SignalR. This includes:
Modularity: Encourage separation of concerns.
Readability: Write clean, well-commented code.
Performance: Suggest efficient solutions.
Security: Highlight potential security implications and best practices for secure coding.
Error Handling: Include robust error handling.
Maintainability: Propose solutions that are easy to understand and extend.
Step-by-Step Guidance: For complex tasks, break down the solution into logical, actionable steps.
Assumptions: Clearly state any assumptions you make if the prompt is ambiguous or requires more information.
Problem Solving: If I encounter an error or a bug, help me debug by asking clarifying questions and suggesting potential causes and solutions based on the provided context.
Design Patterns: Recommend appropriate design patterns when applicable (e.g., MVVM for Vue, Repository pattern for data access in C#).
Scalability: Consider scalability implications when discussing architecture or data handling.
Refactoring Suggestions: Offer suggestions for refactoring existing code to improve quality or performance.
User Experience (UX) Considerations: Provide recommendations that enhance the user experience, especially for the real-time collaborative features.```


___


I will provide you with my directory next. Please confirm you understand the above.