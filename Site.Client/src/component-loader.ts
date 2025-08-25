import React from 'react';
import ReactDOM from 'react-dom/client';

/**
 * Finds all elements with a given data attribute and mounts a React component into each one.
 * @param componentName The name of the component, which must match the `data-component` attribute.
 * @param Component The actual React component to render.
 */
export function mountComponent<P extends object>(componentName: string, Component: React.ComponentType<P>) {
    const elements = document.querySelectorAll<HTMLElement>(`[data-component="${componentName}"]`);

    elements.forEach(element => {
        // Parse props from a data attribute. Assumes props are stored as a JSON string.
        const props = element.dataset.props ? JSON.parse(element.dataset.props) as P : {} as P;
        
        ReactDOM.createRoot(element).render(
            React.createElement(Component, props)
        );
    });
}
