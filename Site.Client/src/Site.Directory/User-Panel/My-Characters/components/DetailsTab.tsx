import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Character, EditorLookups } from '../types';

interface DetailsTabProps {
    character: Character;
    lookups: EditorLookups;
    selectedGenres: number[];
    onSave: () => void;
}

const DetailsTab: React.FC<DetailsTabProps> = ({ character, lookups, selectedGenres, onSave }) => {
    const [avatarFile, setAvatarFile] = useState<File | null>(null);
    const [cardFile, setCardFile] = useState<File | null>(null);
    const [formData, setFormData] = useState({ ...character, selectedGenreIds: selectedGenres });
    const [isSaving, setIsSaving] = useState(false);
    const [saveStatus, setSaveStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

    useEffect(() => {
        setFormData({ ...character, selectedGenreIds: selectedGenres });
    }, [character, selectedGenres]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
        const { name, value, type } = e.target;
        const inputValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;
        setFormData(prev => ({ ...prev, [name]: inputValue }));
    };

    const handleGenreChange = (genreId: number) => {
        setFormData(prev => {
            const newGenreIds = prev.selectedGenreIds.includes(genreId)
                ? prev.selectedGenreIds.filter(id => id !== genreId)
                : [...prev.selectedGenreIds, genreId];
            return { ...prev, selectedGenreIds: newGenreIds };
        });
    };

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setSaveStatus(null);

        const submissionData = new FormData();

        // Append all form fields to the FormData object
        Object.entries(formData).forEach(([key, value]) => {
            if (key === 'selectedGenreIds') return; // Handled separately
            if (value !== null && value !== undefined) {
                submissionData.append(key, String(value));
            }
        });

        formData.selectedGenreIds.forEach(id => submissionData.append('selectedGenreIds', id.toString()));

        if (avatarFile) {
            submissionData.append('avatarImage', avatarFile);
        }
        if (cardFile) {
            submissionData.append('cardImage', cardFile);
        }

        try {
            // ✅ CHANGE: Added a config object to explicitly set the Content-Type header.
            // This ensures multipart/form-data is used for file uploads, overriding any
            // potentially incorrect global axios settings.
            const config = {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            };

            let response;
            if (formData.characterId === 0) {
                response = await axios.post(`/api/characters`, submissionData, config);
                window.location.href = `/User-Panel/My-Characters/Edit/${response.data.newCharacterId}`;
            } else {
                response = await axios.post(`/api/characters/${formData.characterId}/details`, submissionData, config);
            }

            setSaveStatus({ message: response.data.message, type: 'success' });
            onSave();
        } catch (error) {
            setSaveStatus({ message: 'Failed to save details.', type: 'error' });
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <form onSubmit={handleSave}>
            <div className="mb-3">
                <label htmlFor="characterDisplayName" className="form-label">Display Name</label>
                <input type="text" id="characterDisplayName" name="characterDisplayName" className="form-control" value={formData.characterDisplayName || ''} onChange={handleChange} required />
            </div>

            <div className="row">
                <div className="col-md-6 mb-3">
                    <label htmlFor="avatarImage" className="form-label">New Avatar Image</label>
                    <input type="file" id="avatarImage" name="avatarImage" className="form-control" onChange={e => setAvatarFile(e.target.files ? e.target.files[0] : null)} accept="image/*" />
                </div>
                <div className="col-md-6 mb-3">
                    <label htmlFor="cardImage" className="form-label">New Card Image</label>
                    <input type="file" id="cardImage" name="cardImage" className="form-control" onChange={e => setCardFile(e.target.files ? e.target.files[0] : null)} accept="image/*" />
                </div>
            </div>

            {/* ... other form fields remain the same ... */}
            <div className="mb-3">
                <label htmlFor="characterGender" className="form-label">Gender</label>
                <select id="characterGender" name="characterGender" className="form-select" value={formData.characterGender || ''} onChange={handleChange}>
                    <option value="">- Select -</option>
                    {lookups.genders.map(g => <option key={g.genderId} value={g.genderId}>{g.genderName}</option>)}
                </select>
            </div>

            <div className="mb-3">
                <label className="form-label">Genres</label>
                <div className="border rounded p-2" style={{ maxHeight: '200px', overflowY: 'auto' }}>
                    <div className="row">
                        {lookups.genres.map(g => (
                            <div key={g.genreId} className="col-md-4 col-sm-6">
                                <div className="form-check">
                                    <input type="checkbox" id={`genre-${g.genreId}`} className="form-check-input" checked={formData.selectedGenreIds.includes(g.genreId)} onChange={() => handleGenreChange(g.genreId)} />
                                    <label htmlFor={`genre-${g.genreId}`} className="form-check-label">{g.genreName}</label>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            <div className="card-footer d-flex justify-content-end mt-4 bg-transparent px-0">
                <button type="submit" className="btn btn-primary" disabled={isSaving}>
                    {isSaving ? 'Saving...' : 'Save Details'}
                </button>
            </div>

            {saveStatus && (
                <div className={`mt-3 alert alert-${saveStatus.type}`}>
                    {saveStatus.message}
                </div>
            )}
        </form>
    );
};

export default DetailsTab;