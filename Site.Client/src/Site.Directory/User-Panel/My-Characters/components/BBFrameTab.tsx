import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { CharacterInline } from '../types';

interface BBFrameTabProps {
    characterId: number;
    initialBio: string | null;
    initialInlines: CharacterInline[];
    onUpdate: () => void;
}

const BBFrameTab: React.FC<BBFrameTabProps> = ({ characterId, initialBio, initialInlines, onUpdate }) => {
    const [bioContent, setBioContent] = useState(initialBio || '');
    const [inlines, setInlines] = useState(initialInlines);
    const [newInlineName, setNewInlineName] = useState('');
    const [newInlineFile, setNewInlineFile] = useState<File | null>(null);
    const [isSavingBio, setIsSavingBio] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

    useEffect(() => {
        setBioContent(initialBio || '');
        setInlines(initialInlines);
    }, [initialBio, initialInlines]);

    const handleSaveBio = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSavingBio(true);
        setStatus(null);
        try {
            // Note: This API endpoint (`/api/characters/{id}/bbframe`) needs to be created on your backend.
            await axios.put(`/api/characters/${characterId}/bbframe`, { bioContent });
            setStatus({ message: 'Bio saved successfully!', type: 'success' });
        } catch (error) {
            setStatus({ message: 'Failed to save bio.', type: 'error' });
        } finally {
            setIsSavingBio(false);
        }
    };

    const handleUploadInline = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newInlineFile || !newInlineName) {
            setStatus({ message: 'Both a name and a file are required.', type: 'error' });
            return;
        }
        setIsUploading(true);
        setStatus(null);
        const formData = new FormData();

        formData.append('name', newInlineName);
        formData.append('file', newInlineFile);

        try {
            await axios.post(`/api/characters/${characterId}/inlines/upload`, formData);
            setStatus({ message: `Inline "${newInlineName}" uploaded successfully!`, type: 'success' });
            setNewInlineName('');
            setNewInlineFile(null);
            (document.getElementById('inline-file-input') as HTMLInputElement).value = '';
            onUpdate();
        } catch (error) {
            setStatus({ message: 'Failed to upload inline image.', type: 'error' });
        } finally {
            setIsUploading(false);
        }
    };

    const handleDeleteInline = async (inlineId: number) => {
        if (!window.confirm('Are you sure you want to delete this inline image? This cannot be undone.')) return;
        setStatus(null);
        try {
            // Note: This API endpoint (`/api/characters/{id}/inlines/{inlineId}`) needs to be created on your backend.
            await axios.delete(`/api/characters/${characterId}/inlines/${inlineId}`);
            setStatus({ message: 'Inline deleted successfully.', type: 'success' });
            onUpdate();
        } catch (error) {
            setStatus({ message: 'Failed to delete inline.', type: 'error' });
        }
    };

    const copyToClipboard = (text: string) => {
        navigator.clipboard.writeText(text);
    };

    return (
        <div className="row g-3">
            <div className="col-lg-8">
                <form onSubmit={handleSaveBio}>
                    <div className="mb-3">
                        <label htmlFor="bbframe-editor" className="form-label">Profile Content (BBCode)</label>
                        <textarea id="bbframe-editor" className="form-control" rows={15} value={bioContent} onChange={(e) => setBioContent(e.target.value)} />
                    </div>
                    <div className="d-flex justify-content-end">
                        <button type="submit" className="btn btn-primary" disabled={isSavingBio}>{isSavingBio ? 'Saving...' : 'Save Bio'}</button>
                    </div>
                </form>
            </div>

            <div className="col-lg-4">
                <div className="card mb-3">
                    <div className="card-header"><h5 className="mb-0">Manage Inlines</h5></div>
                    <div className="card-body" style={{ maxHeight: '400px', overflowY: 'auto' }}>
                        {inlines.length === 0 ? <p className="text-muted small">No inline images.</p> : (
                            <ul className="list-group list-group-flush">
                                {inlines.map(inline => (
                                    <li key={inline.inlineId} className="list-group-item d-flex justify-content-between align-items-center">
                                        <div>
                                            <img src={inline.inlineImageUrl} alt={inline.inlineName} className="img-thumbnail me-2" style={{ width: '40px', height: '40px', objectFit: 'cover' }} />
                                            <span className="fw-bold">{inline.inlineName}</span>
                                            <div className="input-group input-group-sm mt-1">
                                                <input type="text" className="form-control" value={`[img=${inline.inlineId}]`} readOnly />
                                                <button className="btn btn-outline-secondary" type="button" onClick={() => copyToClipboard(`[img=${inline.inlineId}]`)} title="Copy BBCode"><i className="bi bi-clipboard"></i></button>
                                            </div>
                                        </div>
                                        <button className="btn btn-sm btn-outline-danger" title="Delete" onClick={() => handleDeleteInline(inline.inlineId)}><i className="bi bi-trash"></i></button>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                </div>

                <div className="card">
                    <div className="card-header"><h5 className="mb-0">Upload New Inline</h5></div>
                    <div className="card-body">
                        <form onSubmit={handleUploadInline}>
                            <div className="mb-3">
                                <label htmlFor="inline-name-input" className="form-label">Inline Name</label>
                                <input id="inline-name-input" type="text" className="form-control" placeholder="e.g., header-image" value={newInlineName} onChange={(e) => setNewInlineName(e.target.value)} />
                            </div>
                            <div className="mb-3">
                                <label htmlFor="inline-file-input" className="form-label">Image File</label>
                                <input id="inline-file-input" type="file" className="form-control" onChange={(e) => setNewInlineFile(e.target.files ? e.target.files[0] : null)} accept="image/*" />
                            </div>
                            <div className="d-grid">
                                <button type="submit" className="btn btn-success" disabled={isUploading}>{isUploading ? 'Uploading...' : 'Upload'}</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>

            {status && (
                <div className={`col-12 mt-3 alert alert-${status.type === 'success' ? 'success' : 'danger'}`}>
                    {status.message}
                </div>
            )}
        </div>
    );
};

export default BBFrameTab;