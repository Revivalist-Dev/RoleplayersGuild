# Project TO-DO List

This document outlines the recommended next steps and refactoring tasks to fully integrate the recently added features into the codebase.

## 1. Client-Side State Management (`Zustand`)

The `zustand` library has been added to `Site.Client` for managing global UI state.

-   [ ] **Refactor UI State:** Identify components that share state across different "React Islands" or widely separated parts of the component tree.
    -   **Candidate:** A theme switcher (e.g., light/dark mode).
    -   **Action:** Create a new store (e.g., `uiStore.ts`) and move shared state logic into it.
    -   **Example:** Instead of passing down props, components can directly access state from the store:
        ```tsx
        // In any component that needs the theme
        import { useThemeStore } from './Site.Stores/themeStore';
        const theme = useThemeStore((state) => state.theme);
        ```

-   [ ] **Consolidate Global State:** Review existing uses of React's `Context` API for global state. If they are causing performance issues due to unnecessary re-renders, consider migrating them to a Zustand store for more granular state selection.

## 2. Strongly-Typed Configuration (`RoleplayersGuild`)

The Options pattern has been set up to provide strongly-typed access to `appsettings.json`.

-   [ ] **Refactor Configuration Access:** Search the codebase for direct calls to `IConfiguration` (e.g., `_configuration["Section:Key"]`).
    -   **Action:** Replace these calls by injecting the corresponding `IOptions<T>` object into the service's constructor and accessing the properties from the `.Value` property.
    -   **Before:**
        ```csharp
        private readonly IConfiguration _configuration;
        // ...
        var siteKey = _configuration["RecaptchaSettings:SiteKey"];
        ```
    -   **After:**
        ```csharp
        private readonly RecaptchaSettings _recaptchaSettings;

        public MyService(IOptions<RecaptchaSettings> recaptchaOptions)
        {
            _recaptchaSettings = recaptchaOptions.Value;
        }

        // ...
        var siteKey = _recaptchaSettings.SiteKey;
        ```
-   [ ] **Add Validation:** For critical settings, add `[Required]` or other data validation attributes to your configuration model classes in `Project.Configuration`. This will cause the application to fail on startup if a required setting is missing, preventing runtime errors.

## 3. API Documentation (`Swashbuckle`)

Swagger is now generating API documentation at the `/swagger` endpoint in the Development environment.

-   [ ] **Enhance API Documentation:** Improve the quality of the generated documentation by decorating your API controller actions and DTOs with XML comments and data annotations.
    -   **Action:** Enable XML documentation output for your project and add summary tags.
    -   **Example:**
        ```csharp
        /// <summary>
        /// Retrieves a specific character by their unique ID.
        /// </summary>
        /// <param name="id">The ID of the character to retrieve.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacter(int id)
        {
            // ...
        }
        ```

## 4. Database Migrations (`FluentMigrator`)

FluentMigrator is set up to run migrations automatically on startup.

-   [ ] **Adopt a Migration-First Workflow:** For all future database schema changes, do not modify the database directly.
    -   **Action:** Create a new migration class in the `Site.Database/Migrations` folder. Use a timestamp for the version number (e.g., `YYYYMMDDHHMM`).
    -   **Example:**
        ```csharp
        [Migration(202508130500, "Add Bio column to Characters table")]
        public class AddBioToCharacters : Migration
        {
            public override void Up()
            {
                Alter.Table("Characters").AddColumn("Bio").AsString(int.MaxValue).Nullable();
            }

            public override void Down()
            {
                Delete.Column("Bio").FromTable("Characters");
            }
        }
        ```

## 5. Health Checks

A health check endpoint is now available at `/healthz`.

-   [ ] **Add More Health Checks:** Add checks for other critical external dependencies.
    -   **Candidate:** AWS S3. You can install the `AspNetCore.HealthChecks.Aws.S3` NuGet package.
    -   **Action:** Extend the health check configuration in `Program.cs`:
        ```csharp
        builder.Services.AddHealthChecks()
            .AddNpgSql(...)
            .AddS3(options =>
            {
                options.BucketName = awsSettings.BucketName;
                // ... other options
            });
        ```
-   [ ] **Configure Production Monitoring:** Set up your production monitoring service (e.g., AWS CloudWatch, UptimeRobot) to periodically poll the `/healthz` endpoint to ensure your application is running correctly.

## 6. Infrastructure: Cloudflare & AWS Caching Strategy

To improve site performance and reduce AWS data transfer costs, it's crucial to configure an effective caching strategy between Cloudflare (the CDN) and AWS (the origin).

-   [ ] **Configure S3 `Cache-Control` Metadata:** Set long-lived cache headers for versioned/hashed assets and shorter-lived headers for non-versioned assets.
-   [ ] **Implement ASP.NET Core Response Caching:** Use the `[ResponseCache]` attribute on cacheable API endpoints and Razor Pages.
-   [ ] **Configure Cloudflare to Respect Origin Headers:** Set "Browser Cache TTL" to "Respect Existing Headers" and create a Page Rule for aggressive caching of static assets.

## 7. Code Style & Quality (StyleCop)

StyleCop rules have been configured to align with modern .NET practices.

-   [ ] **Fix Disabled Warnings (Optional):** Over time, consider fixing the underlying style issues for `SA1101` (this-prefixing) and `SA1633` (file headers) and re-enabling the rules in `stylecop.json` for maximum consistency.
-   [ ] **Document Public APIs (`CS1591`):** Progressively add XML documentation to all public-facing code. As coverage improves, consider removing the `CS1591` suppression from `RoleplayersGuild.csproj`.

## 8. Object Mapping (`Mapster`)

Mapster has been added to automate object-to-object mapping.

-   [ ] **Refactor Manual Mapping:** Identify and replace manual property copying with calls to `_mapper.Map<TDestination>(sourceObject)`.
-   [ ] **Create Custom Mappings:** For complex scenarios, create custom mapping configurations by implementing `IRegister`.

## 9. Image Handling & Moderation

-   [ ] **Implement Metadata Stripping:** Use a library to automatically remove all EXIF metadata from images upon upload.
-   [ ] **Add "Is Mature" Content Flag:** Add an `IsMature` property to the image model and a corresponding checkbox in the upload UI.
-   [ ] **Implement Mature Content Blurring:** Blur mature images by default on the frontend and provide a click-to-reveal mechanism.
-   [ ] **Improve Upload/Remove Logic & Controls:** Refactor the `ImageService` for robustness and enhance the frontend UI for better user feedback.

## 10. Chat: Custom Emoticons (Emotes)

-   [ ] **Create Emoticon Management UI:** Build an interface in the Admin Panel for staff to manage custom chat emoticons.
-   [ ] **Develop Emoticon Parsing Logic:** Enhance text-processing services to recognize and replace emoticon codes with `<img>` tags.
-   [ ] **Build Frontend Emoticon Picker:** Create a UI component in the chat clients to allow users to easily select and insert emoticons.

## 11. Marketplace: Commissions System

-   [ ] **Develop Commission Profiles:** Create a profile section for users to list their commission services (Writing, Profile Design, Code).
-   [ ] **Create a Marketplace Listing Page:** Build a central, filterable page to display all users offering commissions.
-   [ ] **Implement a Commission Request & Tracking System:** Develop a system to manage the lifecycle of a commission job (Requested, Accepted, etc.).

## 12. Backend: Custom Error Pages

-   [ ] **Implement Custom Error Pages:** Create user-friendly, branded error pages for common HTTP status codes (e.g., 404, 500) to replace the ASP.NET Core defaults.
### Step 3: Configure Cloudflare to Respect Origin Headers

This is the most critical step. You need to ensure Cloudflare's caching behavior is driven by the headers your origin sends.

-   [ ] **Set "Browser Cache TTL" to "Respect Existing Headers":**
    -   **Location:** In your Cloudflare Dashboard, go to `Caching` -> `Configuration`.
    -   **Action:** Set the "Browser Cache TTL" setting to "Respect Existing Headers". This tells Cloudflare to use the `max-age` value from your origin's `Cache-Control` headers to determine how long a user's browser should cache the resource.

-   [ ] **Create a Page Rule for Aggressive Caching:**
    -   **Location:** In your Cloudflare Dashboard, go to `Rules` -> `Page Rules`.
    -   **Action:** Create a Page Rule for the URL patterns that match your static assets (e.g., `*yourdomain.com/react-dist/*` or `*yourdomain.com/images/*`).
    -   **Settings for the Page Rule:**
        -   **Cache Level:** `Cache Everything` (This forces Cloudflare to cache the HTML content itself, not just static assets).
        -   **Edge Cache TTL:** `Respect Existing Headers` or a long duration like `a month`. Setting it to respect headers is generally the most flexible approach.

By following these steps, you create a robust caching hierarchy:
1.  A user's browser caches assets based on headers from Cloudflare.
2.  Cloudflare's edge network caches assets based on headers from your AWS origin.
3.  Your AWS origin has full control over how long every asset and API response should be cached.

## 7. Code Style & Quality (StyleCop)

To reduce build noise, two common StyleCop rules (`SA1101` and `SA1633`) have been temporarily disabled in `stylecop.json`. While this cleans up the build log, you may want to address the underlying style issues over time for maximum code consistency.

### How to Fix the Disabled Warnings

-   [ ] **Fix Missing `this.` Prefix (`SA1101`):**
    -   **Problem:** This warning occurs when you call an instance member of a class (a method, property, or field) without prefixing it with `this.`.
    -   **Fix:** The easiest way to fix this across the entire solution is to use your IDE's code cleanup tools. In Visual Studio or Rider, you can configure the code cleanup profile to enforce this rule and then run it on the solution.
    -   **Manual Fix Example:**
        -   **Before:** `MyProperty = "value";`
        -   **After:** `this.MyProperty = "value";`
    -   **To Re-enable:** In `stylecop.json`, change `"prefixLocalCallsWithThis": "none"` to `"prefixLocalCallsWithThis": "require"`.

-   [ ] **Fix Missing File Headers (`SA1633`):**
    -   **Problem:** This warning occurs when a C# file does not have the standard XML file header comment at the top.
    -   **Fix:** Most modern IDEs with Roslyn analyzer support can add these headers for you automatically. In Visual Studio, you can often use the Quick Actions lightbulb to add the header to a file or project.
    -   **Manual Fix Example:** Add the following to the top of every C# file:
        ```csharp
        // <copyright file="MyClass.cs" company="RoleplayersGuild">
        // Copyright (c) Ars Novitas. All rights reserved.
        // Licensed under the MIT license. See LICENSE in the project root for full license information.
        // </copyright>
        ```
    -   **To Re-enable:** In `stylecop.json`, change `"headerDecoration": "none"` back to `"headerDecoration": "xml"`.

-   [ ] **Document Public APIs (`CS1591`):**
    -   **Problem:** After enabling XML documentation generation, the build now produces thousands of warnings for public types and members that lack XML comments.
    -   **Fix:** The warning (`CS1591`) has been temporarily suppressed in `RoleplayersGuild.csproj` to keep the build log clean. The long-term goal is to document all public-facing code.
    -   **Action:** As you work on different parts of the codebase, add standard XML documentation blocks to all public classes, methods, and properties. This is especially important for your API controllers and service interfaces, as Swashbuckle will use these comments to enrich the Swagger UI.
    -   **Example:**
        ```csharp
        /// <summary>
        /// Represents a user's character in the system.
        /// </summary>
        public class Character
        {
            /// <summary>
            /// Gets or sets the character's unique identifier.
            /// </summary>
            public int Id { get; set; }
        }
        ```
    -   **To Re-enable:** Once you have a good level of documentation coverage, you can remove the `<NoWarn>$(NoWarn);1591</NoWarn>` line from `RoleplayersGuild.csproj` to enforce documentation on all new code.

-   [ ] **Fix Using Directive Placement (`SA1200`):**
    -   **Problem:** This warning requires `using` directives to be placed inside a `namespace` declaration. This conflicts with modern .NET features like global usings and top-level statements.
    -   **Fix:** The rule has been disabled in `stylecop.json` by setting `usingDirectivesMustBePlacedWithinNamespace` to `ignore`. This is the recommended approach for modern .NET projects to allow the use of `GlobalUsings.cs`.
    -   **To Re-enable:** This rule should not be re-enabled as long as you are using global usings.

## 8. Object Mapping (`Mapster`)

Mapster has been added to automate object-to-object mapping.

-   [ ] **Refactor Manual Mapping:** Identify places in your code where you are manually copying properties from one object to another (e.g., in your services or controllers when converting between entities and DTOs).
    -   **Action:** Inject the `IMapper` interface and replace the manual code with a call to `_mapper.Map<TDestination>(sourceObject)`.
    -   **Before:**
        ```csharp
        public UserDto GetUser(int id)
        {
            var user = _userDataService.GetUserById(id);
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                // ... etc
            };
        }
        ```
    -   **After:**
        ```csharp
        private readonly IMapper _mapper;

        public MyService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public UserDto GetUser(int id)
        {
            var user = _userDataService.GetUserById(id);
            return _mapper.Map<UserDto>(user);
        }
        ```
-   [ ] **Create Custom Mappings:** For complex scenarios where property names don't match or require custom logic, you can create a mapping configuration class that implements `IRegister`.
    -   **Action:** Create a new class (e.g., `MappingConfig.cs`) and define your custom mappings. Mapster will automatically discover and use these configurations.
    -   **Example:**
        ```csharp
        public class MappingConfig : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<User, UserDto>()
                    .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
            }
        }
        ```

## 9. Image Handling & Moderation

This section outlines a set of required improvements for the image upload and management pipeline.

-   [ ] **Implement Metadata Stripping:**
    -   **Action:** Use a library (like `SixLabors.ImageSharp`) to automatically remove all EXIF and other metadata from images upon upload.
    -   **Benefit:** Protects user privacy and reduces file size.

-   [ ] **Add "Is Mature" Content Flag:**
    -   **Action:** Add an `IsMature` boolean property to the image model in the database.
    -   **Action:** Update the UI to include a checkbox for users to mark an image as mature content during the upload process.

-   [ ] **Implement Mature Content Blurring:**
    -   **Action:** In the frontend (`Site.Client`), any image with the `IsMature` flag should be blurred by default using CSS filters.
    -   **Action:** Add a button or overlay on the blurred image that allows a user to click to reveal the content. Store the user's preference (e.g., in `localStorage`) to unblur all mature images for their session if they choose.

-   [ ] **Improve Upload/Remove Logic & Controls:**
    -   **Action:** Review and refactor the `ImageService` to be more robust. This could include better error handling, clearer separation of concerns for different image types (avatar vs. character image), and more explicit validation.
    -   **Action:** Enhance the frontend controls for image management, providing clearer feedback to the user during upload (e.g., progress bars) and a more intuitive interface for deleting or replacing existing images.

## 10. Community: Web Rings & Affiliates Program

This section outlines the implementation of a classic web ring or affiliate banner exchange program to foster partnerships with other communities.

-   [ ] **Create Affiliate Banners:**
    -   **Action:** Design a set of official RoleplayersGuild banners in various standard sizes (e.g., 88x31, 468x60) that affiliates can place on their websites.

-   [ ] **Develop a Public "Affiliates" Page:**
    -   **Action:** Create a new page on the site (e.g., `/information/affiliates`) that serves two purposes:
        1.  It displays the banners of all approved affiliate websites, linking back to them.
        2.  It provides the HTML snippets for RoleplayersGuild's own banners, making it easy for potential affiliates to copy and paste the code onto their sites.

-   [ ] **Build a Staff-Facing Management System:**
    -   **Action:** Create a new section in the Admin Panel for managing affiliates.
    -   **Features:**
        -   An application form or process for potential affiliates.
        -   A system for staff to approve or deny applications.
        -   A CRUD (Create, Read, Update, Delete) interface for managing affiliate entries, including their site name, URL, and banner image URL.

## 11. User Engagement: Referral Program

This section outlines the implementation of a referral program to incentivize user growth.

-   [ ] **Generate Unique Referral Codes:**
    -   **Action:** When a user account is created, generate a unique, shareable referral code (or link).
    -   **Action:** Display this code or link prominently on the user's account page.

-   [ ] **Track Referrals:**
    -   **Action:** During the registration process, add an optional "Referral Code" field.
    -   **Action:** When a new user signs up using a code, create a record in the database linking the new user to the referrer.

-   [ ] **Implement the Reward System:**
    -   **Action:** Create a background job or a scheduled task that periodically checks the referral count for each user.
    -   **Action:** When a user's referral count reaches a specified threshold (e.g., 5 successful referrals), automatically grant them a reward, such as a temporary premium membership, a unique badge, or other site privileges.
    -   **Action:** Send a notification to the user informing them of their reward.

## 12. Chat: Custom Emoticons (Emotes)

This section outlines the implementation of a custom emoticon system for the real-time chat.

-   [ ] **Create Emoticon Management UI:**
    -   **Action:** Build an interface in the Admin Panel for staff to upload, name, and manage custom chat emoticons. Each emote will consist of a name (e.g., `:smile:`) and an associated image.

-   [ ] **Develop Emoticon Parsing Logic:**
    -   **Action:** In the `BBCodeService` or a similar text-processing service, enhance the parsing logic to recognize and replace emoticon codes (e.g., `:smile:`) with the corresponding `<img>` tag pointing to the emote image.
    -   **Action:** Ensure this parsing is applied to chat messages before they are rendered on the client.

-   [ ] **Build Frontend Emoticon Picker:**
    -   **Action:** In the `Site.Client` and `Codex Client`, create a UI component (e.g., a clickable icon next to the chat input) that opens a panel displaying all available custom emoticons.
    -   **Action:** Clicking an emoticon in the picker should insert its code into the chat input field.

## 13. Marketplace: Commissions System

This section outlines the implementation of a commissions system within the site's marketplace, allowing users to offer and solicit services.

-   [ ] **Develop Commission Profiles:**
    -   **Action:** Create a new profile type or a new section on a user's main profile where they can list the commission services they offer.
    -   **Details:** The profile should support distinct categories like "Writing Commissions," "Profile Design Commissions," and "Code Commissions."
    -   **Fields:** Each commission type should have fields for a description, examples of work, pricing information (e.g., per word, flat rate), and current status (open/closed).

-   [ ] **Create a Marketplace Listing Page:**
    -   **Action:** Build a central marketplace page that lists all users who are open for commissions.
    -   **Features:** The page should be searchable and filterable by commission type.

-   [ ] **Implement a Commission Request & Tracking System:**
    -   **Action:** Develop a system for users to formally request a commission from another user. This should create a trackable "job" in the system.
    -   **Workflow:** The system should manage the state of the commission (Requested, Accepted, In Progress, Completed, Canceled) and facilitate communication between the client and the artist/writer.
    -   **Note:** This does not need to handle payment processing initially; it can simply be a system for tracking and managing the commission agreement.

## 14. Site Administration & Funding

This section contains non-technical, administrative tasks related to the site's operation and funding.

-   [ ] **Chime Referral Initiative:**
    -   **Action:** Reach out to a select group of trusted users to ask if they would be willing to sign up for Chime using your referral code.
    -   **Goal:** The referral bonuses will be used to help cover the site's hosting and operational costs over the next few months.

## 15. Monetization: Merchandise Store

This section outlines tasks related to setting up an official merchandise store for the website.

-   [ ] **Establish Shopify Store:**
    -   **Action:** Create and configure a new Shopify store for RoleplayersGuild.com.
    -   **Tasks:** Set up payment processing, shipping details, and basic store branding.

-   [ ] **Design "Great Purge" T-Shirt:**
    -   **Action:** Create a t-shirt design with the theme "I survived the great purge."
    -   **Action:** Prepare the design files for a print-on-demand service and create the product listing in the Shopify store.

## 16. Backend: Custom Error Pages

This section outlines a technical task for improving the user experience during application errors.

-   [ ] **Implement Custom Error Pages:**
    -   **Action:** Create a set of user-friendly, custom error pages for common HTTP status codes (e.g., 404 Not Found, 500 Internal Server Error).
    -   **Goal:** Replace the default ASP.NET Core error pages with branded pages that provide a better user experience and guide the user back to the main site.

## 13. Monetization & Funding Features

This section outlines the implementation of site features related to funding and monetization.

-   [ ] **Implement Dashboard Funding Goal:**
    -   **Action:** Create a new View Component or React Island to display a site funding goal on the main dashboard.
    -   **Backend:** The values for the goal (e.g., current amount, target amount) should be manageable by staff in the Admin Panel.
    -   **Frontend:** The component should visually represent the progress towards the goal (e.g., a progress bar).

-   [ ] **Create "Support Us via eBay" Page:**
    -   **Action:** Develop a new static page (e.g., `/information/support-us`) that explains how users can support the site by purchasing items from your eBay page.
    -   **Content:** The page should include a clear link to your eBay profile and a brief explanation of how the proceeds help fund the site.

-   [ ] **Integrate Site Advertisements:**
    -   **Action:** Research and select an ad provider (e.g., Google AdSense, EthicalAds).
    -   **Action:** Implement the necessary ad slots in the site's layout. Common locations include the header, footer, and sidebars.
    -   **Action:** Ensure the ad integration does not significantly degrade site performance or user experience.