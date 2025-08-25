// This is the main entry point for the site's JavaScript.
// It imports and executes all necessary site-wide scripts and stylesheets.

import './vendor';
import './script-loader';
import './Site.Scripts/twitter-widget';
import './Site.Scripts/user-activity-heartbeat';
import './Site.Scripts/anti-forgery-token';
import './Site.Scripts/UserNav';
import './Site.Scripts/ChatRoom';
import './Site.Scripts/image-fallback';
import './Site.Scripts/modal-initializer';

// Import the main stylesheet entry point.
// Vite will handle HMR for all imported partials.
import '@/styles/scss/vendor.scss';
import '@/styles/scss/site.scss';
