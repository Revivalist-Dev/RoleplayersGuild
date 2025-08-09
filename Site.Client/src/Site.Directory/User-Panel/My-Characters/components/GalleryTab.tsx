import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { DragEndEvent } from '@dnd-kit/core';
import { arrayMove } from '@dnd-kit/sortable';
import { CharacterImage } from '../../../../types';
import ImageManager from './ImageManager';
import GalleryTabView, { GalleryTabViewHandle } from '../../../Community/Characters/components/GalleryTabView';
import { forwardRef, useImperativeHandle, useRef } from 'react';

interface GalleryTabProps {
    characterId: number;
    images: CharacterImage[];
    onGalleryUpdate: () => void;
    onImageUpload: (newImage: CharacterImage) => void;
    onImagesChange: (images: CharacterImage[]) => void;
}

export interface GalleryTabHandle {
    relayout: () => void;
}

const GalleryTab = forwardRef<GalleryTabHandle, GalleryTabProps>(({ characterId, images, onGalleryUpdate, onImageUpload, onImagesChange }, ref) => {
    const galleryTabViewRef = useRef<GalleryTabViewHandle>(null);
    const [imagesToDelete, setImagesToDelete] = useState<number[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);


    const handleCaptionChange = (id: number, caption: string) => {
        // This will be handled by the parent component
    };

    const handleImageScaleChange = (id: number, scale: number) => {
        // This will be handled by the parent component
    };

    const handleDelete = (id: number) => {
        if (!window.confirm('Are you sure you want to delete this image? This cannot be undone.')) return;
        // This will be handled by the parent component
        if (id > 0) {
            setImagesToDelete(current => [...current, id]);
        }
    };

    const handleUpdateGallery = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setStatus(null);
        const updates = images.map((img, index) => ({ 
            imageId: img.characterImageId, 
            imageCaption: img.imageCaption, 
            imageScale: img.imageScale, 
            sortOrder: index 
        }));
        try {
            await axios.put(`/api/characters/${characterId}/gallery/update`, { images: updates, imagesToDelete: imagesToDelete });
            setStatus({ message: 'Gallery updated successfully!', type: 'success' });
            setImagesToDelete([]);
            onGalleryUpdate();
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
            const response = await axios.post<{ uploadedFileNames: string[] }>(`/api/characters/${characterId}/gallery/upload`, formData);
            setStatus({ message: 'Images uploaded successfully!', type: 'success' });
            
            // Assuming the API returns the details of the new image
            // This part needs to be adjusted based on the actual API response
            if (response.data && response.data.uploadedFileNames) {
                response.data.uploadedFileNames.forEach(fileName => {
                    const newImage: CharacterImage = {
                        characterImageId: Date.now(), // Temporary ID
                        characterImageUrl: fileName,
                        isMature: false,
                        imageCaption: 'New Image',
                        imageScale: 1,
                        width: 0, // These would ideally come from the server
                        height: 0,
                        userId: 0 // This will be set by the server
                    };
                    onImageUpload(newImage);
                });
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

    const handleDragEnd = (event: DragEndEvent) => {
        const { active, over } = event;
        if (over && active.id !== over.id) {
            const oldIndex = images.findIndex((item) => item.characterImageId === active.id);
            const newIndex = images.findIndex((item) => item.characterImageId === over.id);
            onImagesChange(arrayMove(images, oldIndex, newIndex));
        }
    };

    useImperativeHandle(ref, () => ({
        relayout: () => {
            galleryTabViewRef.current?.relayout();
        }
    }));

    return (
        <div className="d-flex flex-column h-100">
            <div className="row g-3 flex-grow-1" style={{ minHeight: 0 }}>
                <div className="col-lg-8 d-flex flex-column">
                    <div className="card h-100">
                        <div className="card-header"><h5 className="mb-0">Gallery Preview</h5></div>
                        <div className="card-body">
                            <GalleryTabView ref={galleryTabViewRef} images={images} onSortEnd={onImagesChange} />
                        </div>
                    </div>
                </div>
                <div className="col-lg-4 d-flex flex-column h-100">
                    <ImageManager
                        images={images}
                        onSortEnd={handleDragEnd}
                        onCaptionChange={handleCaptionChange}
                        onDelete={handleDelete}
                        onImageScaleChange={handleImageScaleChange}
                        onUpload={handleUploadNewImages}
                        onSaveChanges={handleUpdateGallery}
                        isSaving={isSaving}
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
