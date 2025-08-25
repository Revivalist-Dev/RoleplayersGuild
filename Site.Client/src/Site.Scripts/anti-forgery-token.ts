// This module provides a standardized way to retrieve the ASP.NET Core
// anti-forgery token from the DOM.

export function getAntiForgeryToken(): string | null {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]') as HTMLInputElement;
    if (tokenInput) {
        return tokenInput.value;
    }
    console.error('Anti-forgery token not found in the DOM.');
    return null;
}