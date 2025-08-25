// This file is the entry point for all third-party libraries used in the traditional
// Razor Pages part of the site. Vite will bundle these into a single `vendor.js`
// and `vendor.css` file, which can then be referenced in your main _Layout.cshtml.

// NOTE: The main SASS file is no longer imported here. It is now handled
// by its own dedicated entry point, `styles.ts`, to allow for separate
// CSS and JS bundling in production.

// Import Bootstrap JavaScript
// NOTE: Bootstrap's JS components are now initialized manually where needed
// (e.g., modal-initializer.ts) to avoid bundling unused code.
// The full 'bootstrap' import is not used to allow for this selective loading.

// Import Bootstrap Icons CSS
import 'bootstrap-icons/font/bootstrap-icons.css';

// Import other global libraries
import jQuery from 'jquery';
(window as any).$ = jQuery;
(window as any).jQuery = jQuery;
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
import 'packery';
import 'imagesloaded';
import 'linkifyjs';

// Import Microsoft SignalR
// Note: The SignalR client is often loaded from a CDN or a static file in wwwroot
// for simplicity, but bundling it here ensures version consistency.
import '@microsoft/signalr';
