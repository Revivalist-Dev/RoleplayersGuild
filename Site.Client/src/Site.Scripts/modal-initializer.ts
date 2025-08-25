// modal-initializer.ts
// This script manually initializes all Bootstrap modals on a page.
// This is a robust alternative to relying on Bootstrap's automatic
// data-attribute-based initialization, which can sometimes fail.

import { Modal } from 'bootstrap';

document.addEventListener('DOMContentLoaded', () => {
    const modalTriggers = document.querySelectorAll('[data-bs-toggle="modal"]');
    
    modalTriggers.forEach(trigger => {
        const targetSelector = trigger.getAttribute('data-bs-target');
        if (targetSelector) {
            const modalElement = document.querySelector<HTMLElement>(targetSelector);
            if (modalElement) {
                // This ensures that a modal instance is created for each trigger,
                // effectively attaching the necessary click listeners.
                // The instance is not stored as it's only needed for initialization.
                Modal.getOrCreateInstance(modalElement);
            }
        }
    });
});