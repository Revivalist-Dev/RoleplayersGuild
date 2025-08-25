import React, { useState, useRef } from 'react';
import ReactCrop, { Crop, PixelCrop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';

interface ImageCropModalProps {
    src: string;
    onCropComplete: (croppedImage: Blob) => void;
    onClose: () => void;
    aspect: number;
}

const ImageCropModal: React.FC<ImageCropModalProps> = ({ src, onCropComplete, onClose, aspect }) => {
    const [crop, setCrop] = useState<Crop>({ unit: '%', width: 50, height: 50, x: 25, y: 25 });
    const [completedCrop, setCompletedCrop] = useState<PixelCrop | null>(null);
    const imgRef = useRef<HTMLImageElement>(null);

    const getCroppedImg = (image: HTMLImageElement, crop: PixelCrop): Promise<Blob> => {
        const canvas = document.createElement('canvas');
        const scaleX = image.naturalWidth / image.width;
        const scaleY = image.naturalHeight / image.height;
        canvas.width = crop.width;
        canvas.height = crop.height;
        const ctx = canvas.getContext('2d');

        if (!ctx) {
            return Promise.reject(new Error('Failed to get canvas context.'));
        }

        ctx.drawImage(
            image,
            crop.x * scaleX,
            crop.y * scaleY,
            crop.width * scaleX,
            crop.height * scaleY,
            0,
            0,
            crop.width,
            crop.height
        );

        return new Promise((resolve, reject) => {
            canvas.toBlob((blob) => {
                if (!blob) {
                    reject(new Error('Canvas is empty.'));
                    return;
                }
                resolve(blob);
            }, 'image/png');
        });
    };

    const handleCrop = async () => {
        if (completedCrop && imgRef.current) {
            const croppedImageBlob = await getCroppedImg(imgRef.current, completedCrop);
            onCropComplete(croppedImageBlob);
        }
    };

    return (
        <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-lg">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title">Crop Image</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <div className="modal-body">
                        <ReactCrop
                            crop={crop}
                            onChange={(c: Crop) => setCrop(c)}
                            onComplete={(c: PixelCrop) => setCompletedCrop(c)}
                            aspect={aspect}
                        >
                            <img ref={imgRef} src={src} style={{ maxHeight: '70vh' }} />
                        </ReactCrop>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" onClick={onClose}>
                            Cancel
                        </button>
                        <button type="button" className="btn btn-primary" onClick={handleCrop}>
                            Crop
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ImageCropModal;
