import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Character } from '../../../../types';

interface CustomTabProps {
    character: Character;
    onUpdate: () => void;
}

const CustomTab: React.FC<CustomTabProps> = ({ character, onUpdate }) => {
    const [formData, setFormData] = useState({
        profileCss: character.profileCss || '',
        profileHtml: character.profileHtml || '',
        isEnabled: character.customProfileEnabled,
    });
    const [isSaving, setIsSaving] = useState(false);
    const [status, setStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

    useEffect(() => {
        setFormData({
            profileCss: character.profileCss || '',
            profileHtml: character.profileHtml || '',
            isEnabled: character.customProfileEnabled,
        });
    }, [character]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value, type } = e.target;
        const inputValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;
        setFormData((prev) => ({ ...prev, [name]: inputValue }));
    };

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setStatus(null);

        try {
            const response = await axios.put(`/api/characters/${character.characterId}/profile`, formData);
            setStatus({ message: response.data.message, type: 'success' });
            onUpdate(); // Refresh parent data
        } catch (error: any) {
            const errorMessage = error.response?.data?.message || 'Failed to save custom profile.';
            setStatus({ message: errorMessage, type: 'error' });
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <form onSubmit={handleSave}>
            <div className="form-check form-switch mb-3">
                <input
                    id="isEnabled"
                    name="isEnabled"
                    className="form-check-input"
                    type="checkbox"
                    checked={formData.isEnabled}
                    onChange={handleChange}
                />
                <label className="form-check-label" htmlFor="isEnabled">
                    Enable my custom profile
                </label>
            </div>

            <div className="mb-3">
                <label htmlFor="profileCss" className="form-label">
                    Profile CSS
                </label>
                <textarea
                    id="profileCss"
                    name="profileCss"
                    className="form-control"
                    rows={12}
                    placeholder=".Greeting { display: none; }"
                    value={formData.profileCss}
                    onChange={handleChange}
                ></textarea>
            </div>

            <div className="mb-3">
                <label htmlFor="profileHtml" className="form-label">
                    Profile HTML
                </label>
                <textarea
                    id="profileHtml"
                    name="profileHtml"
                    className="form-control"
                    rows={12}
                    placeholder="<p class='Greeting'>Hello, World.</p>"
                    value={formData.profileHtml}
                    onChange={handleChange}
                ></textarea>
            </div>

            <div className="d-flex justify-content-between align-items-center">
                <a
                    className="btn btn-secondary"
                    target="_blank"
                    href={`/Community/Characters/View/${character.characterId}`}
                >
                    View Profile
                </a>
                <button type="submit" className="btn btn-primary" disabled={isSaving}>
                    {isSaving ? 'Saving...' : 'Save Changes'}
                </button>
            </div>

            {status && <div className={`mt-3 alert alert-${status.type}`}>{status.message}</div>}
        </form>
    );
};

export default CustomTab;
