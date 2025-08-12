import React, { useEffect, useRef } from 'react';
import Packery from 'packery';
import { CharacterImage } from '../../../../types';
import styles from './GalleryTabView.module.css';

interface GalleryTabViewProps {
    images: CharacterImage[];
}

const GalleryTabView: React.FC<GalleryTabViewProps> = ({ images }) => {
    const gridRef = useRef<HTMLDivElement>(null);
    const pckry = useRef<any | null>(null);

    useEffect(() => {
        if (gridRef.current && images.length > 0) {
            pckry.current = new (Packery as any)(gridRef.current, {
                itemSelector: `.${styles.gridItem}`,
                columnWidth: 100,
                rowHeight: 100,
                gutter: 10
            });

            pckry.current.on('layoutComplete', () => {
                if (gridRef.current) {
                    gridRef.current.classList.add(styles.layoutReady);
                }
            });
        }

        return () => {
            pckry.current?.destroy();
        };
    }, [images]);

    return (
        <div ref={gridRef} className={styles.grid}>
            {images.map(image => (
                <div
                    key={image.characterImageId}
                    className={styles.gridItem}
                    style={{
                        width: `${image.imageScale * 100}px`,
                        height: `${image.imageScale * 100}px`
                    }}
                >
                    <img src={image.characterImageUrl} alt={image.imageCaption || 'Character Image'} className={styles.gridItemImage} />
                </div>
            ))}
        </div>
    );
};

export default GalleryTabView;
