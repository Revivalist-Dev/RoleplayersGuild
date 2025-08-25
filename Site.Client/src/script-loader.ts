// script-loader.ts
// This script dynamically loads and mounts React components based on the
// presence of `data-component` attributes in the DOM. This enables code splitting.

import { mountComponent } from './component-loader';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type ComponentLoader = () => Promise<{ default: React.ComponentType<any> }>;

const componentRegistry: { [key: string]: ComponentLoader } = {
    CharacterEditor: () => import('./Site.Directory/User-Panel/My-Characters/CharacterEditor'),
    CharacterViewer: () => import('./Site.Directory/Community/Characters/CharacterViewer'),
    // Add other components here as they are created.
};

// --- Page-Specific Script Loading ---
const pageScriptRegistry: { [key: string]: () => Promise<{ default: () => void }> } = {
};

const loadPageScript = async () => {
    const pageContainer = document.querySelector<HTMLElement>('[data-page-script]');
    if (pageContainer?.dataset.pageScript) {
        const scriptName = pageContainer.dataset.pageScript;
        if (pageScriptRegistry[scriptName]) {
            const module = await pageScriptRegistry[scriptName]();
            module.default();
        }
    }
};


document.addEventListener('DOMContentLoaded', () => {
    // Load React Components
    Object.keys(componentRegistry).forEach((componentName) => {
        if (document.querySelector(`[data-component="${componentName}"]`)) {
            componentRegistry[componentName]().then((module) => {
                mountComponent(componentName, module.default);
            });
        }
    });

    // Load Page-Specific Scripts
    loadPageScript();
});