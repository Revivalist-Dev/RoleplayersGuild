import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
// Remove the .tsx extension from this import
import CharacterEditor from './Site.Directory/User-Panel/My-Characters/CharacterEditor';

const rootElement = document.getElementById('character-editor-root');

if (rootElement) {
    const characterId = parseInt(rootElement.dataset.characterId || '0', 10);

    ReactDOM.createRoot(rootElement).render(
        <React.StrictMode>
            <CharacterEditor characterId={characterId} />
        </React.StrictMode>,
    );
}