// NOTE: This script provides a client-side heartbeat to track user activity
// without interfering with server-side rendering.

import { getAntiForgeryToken } from './anti-forgery-token';

const getHeartbeatInterval = (): number => {
    const body = document.querySelector('body');
    const intervalMinutes = parseInt(body?.dataset.heartbeatIntervalMinutes || '15', 10);
    return intervalMinutes * 60 * 1000;
};

const sendHeartbeat = () => {
    const token = getAntiForgeryToken();
    if (!token) {
        // If the token is missing, we cannot send a valid request.
        // The getAntiForgeryToken function will log an error to the console.
        return;
    }

    // The endpoint is /api/userapi/activity, but since the controller is
    // named UserApiController, ASP.NET Core routes it to /api/user/activity.
    // We use fetch for a simple, lightweight POST request.
    fetch('/api/userapi/activity', {
        method: 'POST',
        // We don't need to send a body, but we must include credentials
        // so the server can identify the user via the auth cookie.
        credentials: 'include',
        headers: {
            // Although we're not sending a JSON body, specifying the content type
            // and an empty body is good practice for POST requests.
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({})
    }).catch(error => {
        // We log errors to the console but don't display them to the user,
        // as a failed heartbeat is not a critical failure.
        console.error('User activity heartbeat failed:', error);
    });
};

// Start the heartbeat interval.
setInterval(sendHeartbeat, getHeartbeatInterval());