import React, { useState, useEffect, useRef } from 'react';
import { CharacterImage } from '../../../../types';

interface ImageManagerProps {
    images: CharacterImage[];
    onCaptionChange: (id: number, caption: string) => void;
    onDelete: (id: number) => void;
    onImageScaleChange: (id: number, scale: number) => void;
    onResizeStart: (id: number) => void;
    onResizeEnd: (id: number) => void;
    onResize: (id: number) => void;
    onUpload: (files: FileList | null) => void;
    onSaveChanges: (e: React.FormEvent) => void;
    isSaving: boolean;
    isEditable: boolean;
    selectedImageId?: number | null;
}

const ImageManager: React.FC<ImageManagerProps> = ({ images, onCaptionChange, onDelete, onImageScaleChange, onUpload, onSaveChanges, isSaving, isEditable, selectedImageId, onResizeStart, onResizeEnd, onResize }) => {
    const [selectedImageIds, setSelectedImageIds] = useState<number[]>([]);
    const listRef = useRef<HTMLUListElement>(null);

    useEffect(() => {
        if (selectedImageId && listRef.current) {
            const item = listRef.current.querySelector(`[data-id="${selectedImageId}"]`);
            if (item) {
                item.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
    }, [selectedImageId]);

    const handleSelectionChange = (id: number) => {
        setSelectedImageIds(prev =>
            prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]
        );
    };

    const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.checked) {
            setSelectedImageIds(images.map(i => i.characterImageId));
        } else {
            setSelectedImageIds([]);
        }
    };

    const handleDeleteSelected = () => {
        if (selectedImageIds.length === 0) return;
        if (window.confirm(`Are you sure you want to delete ${selectedImageIds.length} selected images?`)) {
            selectedImageIds.forEach(id => onDelete(id));
            setSelectedImageIds([]);
        }
    };

    return (
        <div className="d-flex flex-column h-100">
            <div className="card flex-grow-1 d-flex flex-column" style={{ minHeight: 0 }}>
                <div className="card-header d-flex justify-content-between align-items-center">
                    <h5 className="mb-0">Manage Images</h5>
                    <div>
                        <input type="checkbox" onChange={handleSelectAll} />
                        <button className="btn btn-sm btn-outline-danger ms-2" onClick={handleDeleteSelected} disabled={selectedImageIds.length === 0}>Delete Selected</button>
                    </div>
                </div>
                <div className="card-body" style={{ height: '400px', overflowY: 'scroll' }}>
                    <ul className="list-group list-group-flush" ref={listRef}>
                        {images.map(image => (
                            <ImageItem
                                key={image.characterImageId}
                                image={image}
                                onCaptionChange={onCaptionChange}
                                onDelete={onDelete}
                                onImageScaleChange={onImageScaleChange}
                                onResizeStart={onResizeStart}
                                onResizeEnd={onResizeEnd}
                                onResize={onResize}
                                isSelected={selectedImageIds.includes(image.characterImageId)}
                                onSelectionChange={handleSelectionChange}
                                isEditable={isEditable}
                            />
                        ))}
                    </ul>
                </div>
            </div>
            <form onSubmit={(e) => { e.preventDefault(); onUpload((document.getElementById('gallery-upload-input') as HTMLInputElement).files); }} className="card mt-3">
                <div className="card-header"><h5 className="mb-0">Upload New Images</h5></div>
                <div className="card-body">
                    <input id="gallery-upload-input" type="file" multiple className="form-control" accept="image/*" />
                </div>
                <div className="card-footer text-end">
                    <button type="submit" className="btn btn-success" disabled={isSaving}>Upload</button>
                </div>
            </form>
            <div className="card mt-3">
                <div className="card-header"><h5 className="mb-0">Save Changes</h5></div>
                <div className="card-body">
                    <p className="small text-muted">Save any changes to image names, sizes, or sort order.</p>
                    <div className="d-grid">
                        <button type="button" className="btn btn-primary" disabled={isSaving} onClick={onSaveChanges}>Save Gallery Changes</button>
                    </div>
                </div>
            </div>
        </div>
    );
};

const ImageItem = ({ image, onCaptionChange, onDelete, onImageScaleChange, isSelected, onSelectionChange, isEditable, onResizeStart, onResizeEnd, onResize }: { image: CharacterImage, onCaptionChange: (id: number, caption: string) => void, onDelete: (id: number) => void, onImageScaleChange: (id: number, scale: number) => void, isSelected: boolean, onSelectionChange: (id: number) => void, isEditable: boolean, onResizeStart: (id: number) => void, onResizeEnd: (id: number) => void, onResize: (id: number) => void }) => {

    return (
        <li className="list-group-item" data-id={image.characterImageId}>
            <div className="d-flex align-items-center mb-2">
                {isEditable && <input type="checkbox" checked={isSelected} onChange={() => onSelectionChange(image.characterImageId)} className="me-2" />}
                <img src={image.characterImageUrl} alt={image.imageCaption || 'thumbnail'} className="img-thumbnail me-3" style={{ width: '60px', height: '60px', objectFit: 'cover' }} />
                <input
                    type="text"
                    className="form-control form-control-sm"
                    placeholder="Image Name..."
                    value={image.imageCaption || ''}
                    onChange={(e) => onCaptionChange(image.characterImageId, e.target.value)}
                    disabled={!isEditable}
                />
                {isEditable && <button className="btn btn-sm btn-outline-danger ms-2" title="Delete" onClick={() => onDelete(image.characterImageId)}><i className="bi bi-trash"></i></button>}
            </div>
            {isEditable && (
                <div>
                    <label className="form-label small">Size</label>
                    <input
                        type="range"
                        className="form-range"
                        min="0"
                        max="4"
                        step="1"
                        value={image.imageScale ?? 1}
                        onChange={(e) => onImageScaleChange(image.characterImageId, parseInt(e.target.value, 10))}
                        onInput={() => onResize(image.characterImageId)}
                        onMouseDown={() => onResizeStart(image.characterImageId)}
                        onMouseUp={() => onResizeEnd(image.characterImageId)}
                    />
                </div>
            )}
        </li>
    );
};

export default ImageManager;
