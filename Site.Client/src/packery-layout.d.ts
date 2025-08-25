/* eslint-disable @typescript-eslint/no-explicit-any */
// Type definitions for Packery v2.1.2
// Project: https://packery.metafizzy.co/
// Definitions by: RoleplayersGuild AI Assistant

declare module 'packery' {
    interface PackeryOptions {
        itemSelector?: string;
        columnWidth?: number | string;
        rowHeight?: number | string;
        gutter?: number | string;
        percentPosition?: boolean;
        stamp?: string;
        isOriginLeft?: boolean;
        isOriginTop?: boolean;
        transitionDuration?: string;
        resize?: boolean;
        initLayout?: boolean;
    }

    class Packery {
        constructor(element: string | Element, options?: PackeryOptions);

        // Methods
        layout(): void;
        layoutItems(items: any[], isStill?: boolean): void;
        stamp(elements: any[]): void;
        unstamp(elements: any[]): void;
        appended(elements: any[]): void;
        prepended(elements: any[]): void;
        addItems(elements: any[]): void;
        remove(elements: any[]): void;
        destroy(): void;
        getItemElements(): any[];
        reloadItems(): void;

        // Events
        on(
            eventName: 'layoutComplete' | 'dragItemPositioned' | 'fitComplete' | 'removeComplete',
            listener: (items: any[]) => void
        ): this;
        off(
            eventName: 'layoutComplete' | 'dragItemPositioned' | 'fitComplete' | 'removeComplete',
            listener: (items: any[]) => void
        ): this;
        once(
            eventName: 'layoutComplete' | 'dragItemPositioned' | 'fitComplete' | 'removeComplete',
            listener: (items: any[]) => void
        ): this;
    }

    export = Packery;
}
