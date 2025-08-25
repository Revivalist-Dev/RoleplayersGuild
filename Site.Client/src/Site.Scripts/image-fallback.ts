// image-fallback.ts
// This script provides a site-wide solution for handling broken images.
// It uses event delegation to catch 'error' events on `<img>` tags.

document.addEventListener('error', (e: Event) => {
    const target = e.target as HTMLImageElement;

    // Verify the event target is an image.
    if (target && target.tagName === 'IMG') {
        const imageType = target.dataset.imageType;
        let fallbackUrl = '';

        switch (imageType) {
            case 'avatar':
                fallbackUrl = '/images/Defaults/NewAvatar.png';
                break;
            case 'card':
                fallbackUrl = '/images/Defaults/NewCharacter.png';
                break;
            default:
                // If no specific type is set, we can use a generic default or do nothing.
                // For now, we'll use the avatar as a general fallback.
                fallbackUrl = '/images/Defaults/NewAvatar.png';
                break;
        }

        // Prevent an infinite loop if the fallback image is also missing.
        if (target.src !== fallbackUrl) {
            target.src = fallbackUrl;
        }
    }
}, true); // Use capture phase to catch the event early.