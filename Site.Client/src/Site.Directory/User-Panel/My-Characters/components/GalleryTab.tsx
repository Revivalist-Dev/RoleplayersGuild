import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { CharacterImage } from '../types';

interface GalleryTabProps {
    characterId: number;
    initialImages: CharacterImage[];
    onGalleryUpdate: () => void;
}

const GalleryTab: React.FC<GalleryTabProps> = ({ characterId, initialImages, onGalleryUpdate }) => {
    const [images, setImages] = useState(initialImages);
    const [imagesToDelete, setImagesToDelete] = useState<number[]>([]);
    const [filesToUpload, setFilesToUpload] = useState<FileList | null>(null);
    const [isSaving, setIsSaving] = useState(false);
    const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

    useEffect(() => {
        setImages(initialImages);
    }, [initialImages]);

    const handleCaptionChange = (id: number, caption: string) => {
        setImages(current => current.map(img => img.characterImageId === id ? { ...img, imageCaption: caption } : img));
    };

    const handleToggleDelete = (id: number, checked: boolean) => {
        setImagesToDelete(current => checked ? [...current, id] : current.filter(i => i !== id));
    };

    const handleUpdateGallery = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setStatus(null);
        const updates = images.map(img => ({ imageId: img.characterImageId, imageCaption: img.imageCaption, isPrimary: img.isPrimary }));
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

    const handleUploadNewImages = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!filesToUpload || filesToUpload.length === 0) {
            setStatus({ message: 'Please select files to upload.', type: 'error' });
            return;
        }
        setIsSaving(true);
        setStatus(null);
        const formData = new FormData();
        Array.from(filesToUpload).forEach(file => { formData.append('uploadedImages', file); });
        try {
            await axios.post(`/api/characters/${characterId}/gallery/upload`, formData);
            setStatus({ message: 'Images uploaded successfully!', type: 'success' });
            (document.getElementById('gallery-upload-input') as HTMLInputElement).value = '';
            setFilesToUpload(null);
            onGalleryUpdate();
        } catch (error) {
            setStatus({ message: 'Failed to upload images.', type: 'error' });
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div>
            <form onSubmit={handleUpdateGallery} className="card mb-4">
                <div className="card-header"><h5 className="mb-0">Manage Existing Images</h5></div>
                <div className="card-body">
                    {images.length === 0 ? <p className="text-muted">This gallery is empty.</p> : (
                        <div className="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3">
                            {images.map(image => (
                                <div key={image.characterImageId} className="col">
                                    <div className="card h-100">
                                        <img src={image.characterImageUrl} className="card-img-top" style={{ aspectRatio: '1 / 1', objectFit: 'cover' }} alt={image.imageCaption || 'Character image'} />
                                        <div className="card-body">
                                            <textarea className="form-control form-control-sm" rows={2} placeholder="Caption..." value={image.imageCaption || ''} onChange={(e) => handleCaptionChange(image.characterImageId, e.target.value)} />
                                        </div>
                                        <div className="card-footer">
                                            <div className="form-check">
                                                <input type="checkbox" className="form-check-input" id={`delete-${image.characterImageId}`} onChange={(e) => handleToggleDelete(image.characterImageId, e.target.checked)} />
                                                <label className="form-check-label small text-danger" htmlFor={`delete-${image.characterImageId}`}>Delete</label>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
                {images.length > 0 && (
                    <div className="card-footer text-end">
                        <button type="submit" className="btn btn-primary" disabled={isSaving}>Update Gallery</button>
                    </div>
                )}
            </form>

            <form onSubmit={handleUploadNewImages} className="card">
                <div className="card-header"><h5 className="mb-0">Upload New Images</h5></div>
                <div className="card-body">
                    <input id="gallery-upload-input" type="file" multiple className="form-control" onChange={(e) => setFilesToUpload(e.target.files)} accept="image/*" />
                </div>
                <div className="card-footer text-end">
                    <button type="submit" className="btn btn-success" disabled={isSaving}>Upload</button>
                </div>
            </form>

            {status && (
                <div className={`mt-3 alert alert-${status.type === 'success' ? 'success' : 'danger'}`}>
                    {status.message}
                </div>
            )}
        </div>
    );
};

export default GalleryTab;