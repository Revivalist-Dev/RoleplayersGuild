import React, { useEffect, useRef, useImperativeHandle, forwardRef } from 'react';
import Packery from 'packery';
import imagesLoaded from 'imagesloaded';

export interface PackeryGridHandle {
    relayout: () => void;
    packery: () => any | null;
}

interface PackeryGridProps {
    children: React.ReactNode;
}

const PackeryGrid = forwardRef<PackeryGridHandle, PackeryGridProps>(({ children }, ref) => {
    const gridRef = useRef<HTMLDivElement>(null);
    const pckryRef = useRef<any | null>(null);

    useEffect(() => {
        if (!gridRef.current) {
            return;
        }

        pckryRef.current = new Packery(gridRef.current, {
            itemSelector: '.grid-item',
            columnWidth: '.grid-sizer',
            gutter: '.gutter-sizer',
            percentPosition: true
        });

        const imgLoad = imagesLoaded(gridRef.current);
        const onAlways = () => pckryRef.current?.layout();
        imgLoad.on('always', onAlways);

        const handleResize = () => pckryRef.current?.layout();
        window.addEventListener('resize', handleResize);

        return () => {
            pckryRef.current?.destroy();
            imgLoad.off('always', onAlways);
            window.removeEventListener('resize', handleResize);
        };
    }, [children]);

    useImperativeHandle(ref, () => ({
        relayout: () => {
            pckryRef.current?.layout();
        },
        packery: () => {
            return pckryRef.current;
        }
    }));

    return (
        <div ref={gridRef} className="packery-grid">
            <div className="grid-sizer"></div>
            <div className="gutter-sizer"></div>
            {children}
        </div>
    );
});

export default PackeryGrid;
