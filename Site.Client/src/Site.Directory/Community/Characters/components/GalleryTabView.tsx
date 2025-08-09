import React from 'react';
import { CharacterImage } from '../../../../types';
import PackeryGrid, { PackeryGridHandle } from '../../../Shared/Components/PackeryGrid';
import { forwardRef, useImperativeHandle, useRef, useEffect } from 'react';
import Draggabilly from 'draggabilly';

interface GalleryTabViewProps {
    images: CharacterImage[];
    onSortEnd: (images: CharacterImage[]) => void;
}

export interface GalleryTabViewHandle {
    relayout: () => void;
}

const GalleryTabView = forwardRef<GalleryTabViewHandle, GalleryTabViewProps>(({ images, onSortEnd }, ref) => {
    const packeryGridRef = useRef<PackeryGridHandle>(null);

    useEffect(() => {
        const pckry = packeryGridRef.current?.packery();
        if (pckry) {
            const items = pckry.getItemElements();
            items.forEach((item: HTMLElement) => {
                const draggie = new Draggabilly(item);
                pckry.bindDraggabillyEvents(draggie);
            });

            pckry.on('dragItemPositioned', () => {
                const itemElems = pckry.getItemElements();
                const newImages = itemElems.map((elem: HTMLElement) => {
                    const id = parseInt(elem.getAttribute('data-id') || '0', 10);
                    return images.find(img => img.characterImageId === id);
                }).filter((img): img is CharacterImage => !!img);
                onSortEnd(newImages);
            });
        }
    }, [images, onSortEnd]);

    useImperativeHandle(ref, () => ({
        relayout: () => {
            packeryGridRef.current?.relayout();
        }
    }));

    if (!images || images.length === 0) {
        return <p className="text-muted">This character has no images in their gallery.</p>;
    }

    const getGridItemClass = (scale: number | null | undefined) => {
        const scaleValue = Math.round(scale || 1);
        if (scaleValue > 1 && scaleValue <= 6) {
            return `grid-item grid-item--width${scaleValue}`;
        }
        return 'grid-item';
    };

    return (
        <div style={{ maxHeight: '800px', overflowY: 'auto' }}>
            <PackeryGrid ref={packeryGridRef}>
                {images.map(image => (
                    <div key={image.characterImageId} data-id={image.characterImageId} className={getGridItemClass(image.imageScale)}>
                        <a href={image.characterImageUrl} target="_blank" rel="noopener noreferrer">
                            <img src={image.characterImageUrl} alt="Character Image" style={{ width: '100%', height: 'auto', display: 'block' }} />
                        </a>
                    </div>
                ))}
            </PackeryGrid>
        </div>
    );
});

export default GalleryTabView;
