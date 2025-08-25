function loadTwitterWidget(): void {
    const timelineElement = document.querySelector('.twitter-timeline');

    if (timelineElement) {
        const script = document.createElement('script');
        script.src = 'https://platform.twitter.com/widgets.js';
        script.async = true;
        script.charset = 'utf-8';
        document.body.appendChild(script);
    }
}

loadTwitterWidget();