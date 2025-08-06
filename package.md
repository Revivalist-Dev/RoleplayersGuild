# **RPG.NET Project Packages & Recommendations**

This document provides an overview of the essential NuGet packages required for the project, as well as recommendations for other libraries and tools that would be beneficial for a modern roleplaying website.

## **Current Essential Packages**

These are the core third-party libraries we have incorporated into the new service-based architecture. They are required for the existing services to function correctly.

- **Dapper (v2.1.35)**: A high-performance micro-ORM (Object-Relational Mapper). It is the foundation of our DataService, allowing us to execute raw SQL queries and have the results automatically mapped to our C\# model classes. It's extremely fast and gives us full control over our SQL.

- **AWSSDK.S3 (v3.7.308.10)**: The official AWS SDK for Amazon S3. This is used by the ImageService to upload, manage, and delete character images and other assets stored in your S3 bucket. AWSSDK.Core is a required dependency.

- **MailKit (v4.6.0)**: A modern, robust, and cross-platform email library. It is the official recommendation for .NET Core and replaces the obsolete SmtpClient. Our NotificationService uses it to handle all outgoing emails securely and asynchronously.

- **SixLabors.ImageSharp (v3.1.4)**: A powerful, modern library for image processing. It is fully cross-platform and replaces the legacy System.Drawing. The ImageService uses it to resize user-uploaded images into "full" and "thumbnail" versions before storing them.

- **HtmlAgilityPack (v1.11.61)**: A robust HTML parsing library for .NET that provides DOM traversal/manipulation similar to jQuery. Essential for sanitizing user-submitted HTML content (profiles, forum posts) to prevent XSS attacks. Key features:
  - Lightweight (no browser engine dependency)
  - Supports malformed HTML
  - XPath query support
  - Can remove/whitelist specific tags/attributes
  Consider pairing with a dedicated sanitizer like AngleSharp for stricter security. Performance note: parsing is fast but complex manipulations may benefit from caching.
  