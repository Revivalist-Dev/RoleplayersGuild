import React, { useState, useEffect, useCallback, useRef } from 'react';
import axios from 'axios';
import Packery from 'packery';
import Draggabilly from 'draggabilly';
import imagesLoaded from 'imagesloaded';
import { CharacterImage } from '../../../../types';
import ImageManager from './ImageManager';
import styles from './GalleryTab.module.css';
import { forwardRef, useImperativeHandle } from 'react';

interface GalleryTabProps {
    characterId: number;
    images: CharacterImage[];
    onGalleryUpdate: (images: CharacterImage[]) => void;
    onImageUpload: (newImage: CharacterImage) => void;
}

export interface GalleryTabHandle {
    // relayout: () => void;
}

const getGridItemStyle = (image: CharacterImage): React.CSSProperties => {
    const baseSize = 150; // Corresponds to the .gridItem width/height in CSS
    const scaleMultiplier = 1 + (image.imageScale ?? 0) * 0.25; // 0=100%, 1=125%, 2=150%, etc.

    if (image.width && image.height) {
        const aspectRatio = image.width / image.height;
        let width = baseSize;
        let height = baseSize;

        if (aspectRatio > 1) {
            // Wider than tall
            width = baseSize * aspectRatio;
        } else {
            // Taller than wide
            height = baseSize / aspectRatio;
        }

        return {
            width: `${width * scaleMultiplier}px`,
            height: `${height * scaleMultiplier}px`,
        };
    }

    // Default style if no dimensions
    return {
        width: `${baseSize * scaleMultiplier}px`,
        height: `${baseSize * scaleMultiplier}px`,
    };
};

const GalleryTab = forwardRef<GalleryTabHandle, GalleryTabProps>(({ characterId, images, onGalleryUpdate, onImageUpload }, ref) => {
    const [initialImages, setInitialImages] = useState<CharacterImage[]>(images);
    const [currentImages, setCurrentImages] = useState<CharacterImage[]>(images);
    const [pendingDeletions, setPendingDeletions] = useState<number[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
    const [selectedImageId, setSelectedImageId] = useState<number | null>(null);
    const [resizingId, setResizingId] = useState<number | null>(null);
    const containerRef = useRef<HTMLDivElement>(null);
    const gridRef = useRef<HTMLDivElement>(null);
    const pckry = useRef<any | null>(null);

    useEffect(() => {
        if (gridRef.current && !pckry.current) {
            pckry.current = new (Packery as any)(gridRef.current, {
                itemSelector: '.grid-item',
                columnWidth: 100,
                gutter: 5
            });

            pckry.current.on('dragItemPositioned', () => {
                const itemElems = pckry.current?.getItemElements();
                if (itemElems) {
                    const newImageIds = itemElems.map((elem: HTMLElement) => parseInt(elem.getAttribute('data-id') || '0', 10));
                    handleOrderChange(newImageIds);
                }
            });
        }

        return () => {
            if (pckry.current) {
                pckry.current.destroy();
                pckry.current = null;
            }
        };
    }, []);

    useEffect(() => {
        if (pckry.current && gridRef.current) {
            imagesLoaded(gridRef.current, () => {
                if (pckry.current && gridRef.current) {
                    pckry.current.reloadItems();
                    
                    const itemElems = Array.from(gridRef.current.querySelectorAll('.grid-item'));
                    itemElems.forEach(item => {
                        if (!item.classList.contains('is-draggable')) {
                            const draggie = new Draggabilly(item as HTMLElement);
                            pckry.current.bindDraggabillyEvents(draggie);
                        }
                    });

                    pckry.current.layout();
                }
            });
        }
    }, [currentImages]);


    const handleCaptionChange = (id: number, caption: string) => {
        const newImages = currentImages.map(img => img.characterImageId === id ? { ...img, imageCaption: caption } : img);
        setCurrentImages(newImages);
    };

    const handleImageScaleChange = (id: number, scale: number) => {
        const newImages = currentImages.map(img => img.characterImageId === id ? { ...img, imageScale: scale } : img);
        setCurrentImages(newImages);
        setResizingId(id); // Track which image is being resized
    };

    useEffect(() => {
        if (resizingId && pckry.current && gridRef.current) {
            const itemElement = gridRef.current.querySelector(`[data-id="${resizingId}"]`);
            if (itemElement) {
                // Use a timeout to allow React to render the style change before fitting
                setTimeout(() => {
                    pckry.current.fit(itemElement);
                    setResizingId(null); // Reset after fitting
                }, 0);
            }
        } else if (pckry.current) {
            pckry.current.layout();
        }
    }, [currentImages, resizingId]);

    const handleResizeStart = (id: number) => {
        setResizingId(id);
    };

    const handleResizeEnd = (id: number) => {
        setResizingId(null);
    };

    const handleResize = (id: number) => {
    };

    const handleSoftDelete = (imageId: number) => {
        setPendingDeletions(prev => [...prev, imageId]);
        setCurrentImages(prev => prev.filter(img => img.characterImageId !== imageId));
    };

    const handleDelete = (id: number) => {
        if (!window.confirm('Are you sure you want to delete this image? This cannot be undone.')) return;
        
        setCurrentImages(currentImages.filter(img => img.characterImageId !== id));

        if (id > 0) {
            setPendingDeletions(current => [...current, id]);
        }
    };

    const handleOrderChange = useCallback((newImageIds: number[]) => {
        setCurrentImages(prevImages => {
            const imageMap = new Map(prevImages.map(img => [img.characterImageId, img]));
            const reorderedImages = newImageIds.map(id => imageMap.get(id)).filter(Boolean) as CharacterImage[];
            return reorderedImages;
        });
    }, []);

    const handleDragStart = useCallback(() => {
        if (containerRef.current) {
            containerRef.current.classList.add('is-dragging');
        }
    }, []);

    const handleDragEnd = useCallback((newImageIds: number[]) => {
        if (containerRef.current) {
            containerRef.current.classList.remove('is-dragging');
        }
        handleOrderChange(newImageIds);
    }, [handleOrderChange]);

    const handleUpdateGallery = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setStatus(null);
        const updates = currentImages.map((img, index) => ({
            imageId: img.characterImageId, 
            imageCaption: img.imageCaption, 
            imageScale: img.imageScale, 
            sortOrder: index 
        }));
        try {
            await axios.put(`/api/characters/${characterId}/gallery/update`, { images: updates, imagesToDelete: pendingDeletions });
            setStatus({ message: 'Gallery updated successfully!', type: 'success' });
            setPendingDeletions([]);
            onGalleryUpdate(currentImages);
        } catch (error) {
            setStatus({ message: 'Failed to update gallery.', type: 'error' });
        } finally {
            setIsSaving(false);
        }
    };

    const handleUploadNewImages = async (filesToUpload: FileList | null) => {
        if (!filesToUpload || filesToUpload.length === 0) {
            setStatus({ message: 'Please select files to upload.', type: 'error' });
            return;
        }
        setIsSaving(true);
        setStatus(null);
        const formData = new FormData();
        Array.from(filesToUpload).forEach(file => { formData.append('uploadedImages', file); });
        try {
            const response = await axios.post<CharacterImage[]>(`/api/characters/${characterId}/gallery/upload`, formData);
            setStatus({ message: 'Images uploaded successfully!', type: 'success' });

            if (response.data) {
                // Append new images returned from the server
                setCurrentImages(currentImages => [...currentImages, ...response.data]);
                // Optionally, call the gallery update to inform the parent component
                onGalleryUpdate([...currentImages, ...response.data]);
            }
            
            const uploadInput = document.getElementById('gallery-upload-input') as HTMLInputElement;
            if(uploadInput) uploadInput.value = '';
            // onGalleryUpdate(); // Optionally keep this to refetch all data
        } catch (error) {
            setStatus({ message: 'Failed to upload images.', type: 'error' });
        } finally {
            setIsSaving(false);
        }
    };

    const handleSaveGallery = async () => {
        setIsSaving(true);
        setStatus(null);

        try {
            // 1. Handle Reordering
            const imageIds = currentImages.map(img => img.characterImageId);
            await axios.post('/api/Characters/UpdateImagePositions', {
                characterId: characterId,
                imageIds: imageIds
            });

            // 2. Handle Deletions
            if (pendingDeletions.length > 0) {
                await axios.post('/api/Characters/DeleteImages', pendingDeletions);
            }

            // 3. Success: Reset state
            setInitialImages(currentImages);
            setPendingDeletions([]);
            setStatus({ message: 'Gallery saved successfully!', type: 'success' });

        } catch (error) {
            setStatus({ message: 'Failed to save gallery.', type: 'error' });
            // Optional: Revert changes on failure
            // setCurrentImages(initialImages);
        } finally {
            setIsSaving(false);
        }
    };


    return (
        <div ref={containerRef} className="d-flex flex-column h-100">
            <div className="row g-3 flex-grow-1" style={{ minHeight: 0 }}>
                <div className="col-lg-8 d-flex flex-column">
                    <div className="card h-100">
                        <div className="card-body d-flex flex-column">
                            <div ref={gridRef} className={`grid w-100 h-100 ${styles.gridContainer}`} style={{ border: "1px dashed #ccc" }}>
                                {currentImages.map(image => (
                                    <div key={image.characterImageId} className={`grid-item ${styles.gridItem}`} style={getGridItemStyle(image)} data-id={image.characterImageId}>
                                        <img src={image.characterImageUrl} alt={image.imageCaption || ''} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
                <div className="col-lg-4 d-flex flex-column h-100">
                    <ImageManager
                        images={currentImages}
                        onCaptionChange={handleCaptionChange}
                        onDelete={handleSoftDelete}
                        onImageScaleChange={handleImageScaleChange}
                        onResizeStart={handleResizeStart}
                        onResizeEnd={handleResizeEnd}
                        onResize={handleResize}
                        onUpload={handleUploadNewImages}
                        onSaveChanges={handleUpdateGallery}
                        isSaving={isSaving}
                        isEditable={true}
                        selectedImageId={selectedImageId}
                    />
                </div>
            </div>

            {status && (
                <div className={`mt-3 alert alert-${status.type}`}>
                    {status.message}
                </div>
            )}
        </div>
    );
});

export default GalleryTab;
