import { Collapse } from 'bootstrap';

// REVIEW: This module encapsulates all logic for the UserNav flyout panel.
// It handles state management (position, orientation, collapsed state) and
// user interactions (dragging, toggling, pinning).

const initUserNav = () => {
    const flyout = document.getElementById('userNavFlyout');
    if (!flyout) return;

    const toggleButton = document.getElementById('userNavToggle');
    const toggleIcon = document.getElementById('userNavIcon');
    const dragHandle = document.getElementById('userNavDragHandle');
    const pinToggleButton = document.getElementById('userNavPinToggle');
    const accordionEl = document.getElementById('userNavAccordion');
    const posStoreKey = 'userNavPosition';
    const accordionStorageKey = 'userNavActiveAccordion';
    
    // Default state, ensuring top and left are numbers.
    const state = { top: 75, left: 10, orientation: 'left', isCollapsed: true };
    let drag = { initialX: 0, initialY: 0, initialLeft: 0, initialTop: 0 };

    const saveState = () => {
        localStorage.setItem(posStoreKey, JSON.stringify(state));
    };

    const loadState = () => {
        const savedState = JSON.parse(localStorage.getItem(posStoreKey) || '{}');
        state.orientation = savedState.orientation || 'left';
        state.isCollapsed = savedState.isCollapsed !== false;
        state.top = parseInt(savedState.top, 10) || 75;
        state.left = parseInt(savedState.left, 10) || 10;
    };

    const updateDOM = () => {
        flyout.className = `user-nav-flyout dock-${state.orientation}`;
        flyout.classList.toggle('collapsed', state.isCollapsed);
        
        flyout.style.top = '';
        flyout.style.left = '';
        flyout.style.right = '';
        flyout.style.bottom = '';

        if (state.orientation === 'right') {
            flyout.style.top = `${state.top}px`;
            flyout.style.right = `${state.left}px`;
        } else {
            flyout.style.top = `${state.top}px`;
            flyout.style.left = `${state.left}px`;
        }
        
        updateToggleIcon();
        flyout.style.visibility = 'visible';
    };

    const updateToggleIcon = () => {
        if (!toggleIcon) return;
        const isRight = state.orientation === 'right';
        const icon = state.isCollapsed
            ? (isRight ? 'bi-chevron-left' : 'bi-chevron-right')
            : (isRight ? 'bi-chevron-right' : 'bi-chevron-left');
        
        toggleIcon.className = `bi ${icon}`;
        toggleIcon.style.visibility = 'visible';
    };

    const handlePinToggleClick = (e: MouseEvent) => {
        e.preventDefault();
        state.orientation = state.orientation === 'left' ? 'right' : 'left';
        saveState();
        updateDOM();
    };

    const handleToggleClick = (e: MouseEvent) => {
        e.preventDefault();
        state.isCollapsed = !state.isCollapsed;
        saveState();
        updateDOM();
    };

    const handleDragStart = (e: MouseEvent) => {
        e.preventDefault();
        drag = {
            initialX: e.clientX,
            initialY: e.clientY,
            initialLeft: flyout.offsetLeft,
            initialTop: flyout.offsetTop
        };
        document.body.classList.add('is-dragging');
        window.addEventListener('mousemove', handleDragMove);
        window.addEventListener('mouseup', handleDragEnd);
    };

    const handleDragMove = (e: MouseEvent) => {
        const dy = e.clientY - drag.initialY;
        state.top = drag.initialTop + dy;
        updateDOM();
    };

    const handleDragEnd = () => {
        window.removeEventListener('mousemove', handleDragMove);
        window.removeEventListener('mouseup', handleDragEnd);
        document.body.classList.remove('is-dragging');

        const margin = 10;
        const flyoutRect = flyout.getBoundingClientRect();
        state.top = Math.max(margin, Math.min(state.top, window.innerHeight - flyoutRect.height - margin));
        
        saveState();
        updateDOM();
    };

    const initAccordion = () => {
        if (!accordionEl) return;

        const activeCollapseId = localStorage.getItem(accordionStorageKey);
        if (!state.isCollapsed && activeCollapseId) {
            const elementToOpen = document.getElementById(activeCollapseId);
            if (elementToOpen) {
                const collapseInstance = new Collapse(elementToOpen, { toggle: false });
                collapseInstance.show();
            }
        }

        accordionEl.addEventListener('shown.bs.collapse', e => {
            localStorage.setItem(accordionStorageKey, (e.target as HTMLElement).id);
        });
    };

    loadState();
    updateDOM();
    initAccordion();

    if (dragHandle) dragHandle.addEventListener('mousedown', handleDragStart);
    if (toggleButton) toggleButton.addEventListener('click', handleToggleClick);
    if (pinToggleButton) pinToggleButton.addEventListener('click', handlePinToggleClick);
};

// Ensure the script runs after the DOM is fully loaded.
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initUserNav);
} else {
    initUserNav();
}