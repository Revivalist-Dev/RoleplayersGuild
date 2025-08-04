// F:\Visual Studio\RoleplayersGuild\wwwroot\js\site.js

async function updateNotificationCounts() {
    try {
        const response = await fetch('/Api/NotificationCounts');

        // If the user is not logged in, the API returns a 401, so we just stop.
        if (!response.ok) {
            return;
        }

        const counts = await response.json();

        // Update the UI for unread threads
        const threadBadge = document.getElementById('thread-badge');
        if (threadBadge) {
            if (counts.unreadThreads > 0) {
                threadBadge.textContent = counts.unreadThreads;
                threadBadge.style.display = 'inline-block';
            } else {
                threadBadge.style.display = 'none';
            }
        }

        // Update the UI for unread image comments
        const imageCommentBadge = document.getElementById('image-comment-badge');
        if (imageCommentBadge) {
            if (counts.unreadImageComments > 0) {
                imageCommentBadge.textContent = counts.unreadImageComments;
                imageCommentBadge.style.display = 'inline-block';
            } else {
                imageCommentBadge.style.display = 'none';
            }
        }

    } catch (error) {
        console.error('Error fetching notification counts:', error);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    // A simple check to see if the user is likely logged in.
    if (document.querySelector('#navbar')) {
        updateNotificationCounts();
        // Optionally, poll for new notifications every few minutes
        // setInterval(updateNotificationCounts, 180000); // every 3 minutes
    }

    // --- BBCode Collapse Functionality ---
    // The click listener is now correctly attached to the body when the DOM is loaded.
    // The nested DOMContentLoaded listener was removed.
    document.body.addEventListener('click', function (e) {
        // Find the closest header when a click happens anywhere on the page
        const header = e.target.closest('.CollapseHeaderText');
        if (header) {
            // Find the parent container (.CollapseHeader) and then the content block (.CollapseBlock)
            const container = header.closest('.CollapseHeader');
            if (container) {
                const block = container.querySelector('.CollapseBlock');
                if (block) {
                    // Toggle the 'display' style to show or hide the content
                    block.style.display = block.style.display === 'block' ? 'none' : 'block';
                }
            }
        }
    });
});