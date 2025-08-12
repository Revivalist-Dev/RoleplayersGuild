import React from 'react';
import CharacterEditor from './Site.Directory/User-Panel/My-Characters/CharacterEditor';
import CharacterViewer from './Site.Directory/Community/Characters/CharacterViewer';

const App: React.FC = () => {
    const editorRoot = document.getElementById('character-editor-root');
    const viewerRoot = document.getElementById('character-viewer-root');

    if (editorRoot) {
        const characterId = parseInt(editorRoot.dataset.characterId || '0', 10);
        const initialAvatarUrl = editorRoot.dataset.initialAvatarUrl || null;
        const initialCardUrl = editorRoot.dataset.initialCardUrl || null;
        return <CharacterEditor characterId={characterId} />;
    }

    if (viewerRoot) {
        const dataElement = document.getElementById('viewer-data');
        if (dataElement && dataElement.textContent) {
            const props = JSON.parse(dataElement.textContent);
            if (props.character) {
                return <CharacterViewer {...props} />;
            }
        }
    }

    return null;
};

export default App;
