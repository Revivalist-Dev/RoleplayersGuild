import React from 'react';
import ReactDOM from 'react-dom/client';
import $ from 'jquery';
import '../../Site.Styles/scss/site.scss';
import App from './App';

// Make jQuery available globally
(window as any).$ = $;
(window as any).jQuery = $;

const rootElement = document.getElementById('react-root');

if (rootElement) {
    ReactDOM.createRoot(rootElement).render(
        <React.StrictMode>
            <App />
        </React.StrictMode>,
    );
}
