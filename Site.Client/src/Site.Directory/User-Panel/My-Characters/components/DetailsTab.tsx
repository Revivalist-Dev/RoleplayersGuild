import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Character, EditorLookups } from '../../../../types';
import CharacterCardPreview from './CharacterCardPreview';
import ImageCropModal from './ImageCropModal';

interface DetailsTabProps {
    character: Character;
    lookups: EditorLookups;
    selectedGenres: number[];
    onSave: () => void;
    avatarUrl: string | null;
    cardUrl: string | null;
    onAvatarChange: (newUrl: string) => void;
    onCardChange: (newUrl: string) => void;
}

const DetailsTab: React.FC<DetailsTabProps> = ({
    character,
    lookups,
    selectedGenres,
    onSave,
    avatarUrl,
    cardUrl,
    onAvatarChange,
    onCardChange,
}) => {
    const [avatarFile, setAvatarFile] = useState<File | null>(null);
    const [cardFile, setCardFile] = useState<File | null>(null);

    // Cropping Modal State
    const [imageToCrop, setImageToCrop] = useState<string | null>(null);
    const [croppingAvatar, setCroppingAvatar] = useState(false);
    // TODO: The croppingCard state is not fully implemented.
    const [croppingCard, setCroppingCard] = useState(false);

    // State for the form data
    type FormDataType = Character & { selectedGenreIds: number[] };
    const [formData, setFormData] = useState<FormDataType>({ ...character, selectedGenreIds: selectedGenres });
    // State for the image preview URLs

    const [isSaving, setIsSaving] = useState(false);
    const [saveStatus, setSaveStatus] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

    useEffect(() => {
        setFormData({ ...character, selectedGenreIds: selectedGenres });
    }, [character, selectedGenres, avatarUrl, cardUrl]);

    // Clean up object URLs to prevent memory leaks
    useEffect(() => {
        // No longer need to manage blob URLs here as they are handled by the parent
    }, []);

    const handleAvatarFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (files && files.length > 0) {
            const reader = new FileReader();
            reader.onload = () => {
                setImageToCrop(reader.result as string);
                setCroppingAvatar(true);
            };
            reader.readAsDataURL(files[0]);
        }
    };

    const handleCardFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (files && files.length > 0) {
            const reader = new FileReader();
            reader.onload = () => {
                setImageToCrop(reader.result as string);
                setCroppingCard(true);
            };
            reader.readAsDataURL(files[0]);
        }
    };

    const handleAvatarCropComplete = (blob: Blob) => {
        const file = new File([blob], 'avatar.png', { type: 'image/png' });
        setAvatarFile(file);
        const newUrl = URL.createObjectURL(file);
        onAvatarChange(newUrl);
        closeCropModal();
    };

    const handleCardCropComplete = (blob: Blob) => {
        const file = new File([blob], 'card.png', { type: 'image/png' });
        setCardFile(file);
        const newUrl = URL.createObjectURL(file);
        onCardChange(newUrl);
        closeCropModal();
    };

    const closeCropModal = () => {
        setImageToCrop(null);
        setCroppingAvatar(false);
        setCroppingCard(false);
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
        const { name, value, type } = e.target;
        const inputValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;
        const finalValue = type === 'select-one' && value === '' ? null : inputValue;
        setFormData((prev) => ({ ...prev, [name]: finalValue }));
    };

    const handleGenreChange = (genreId: number) => {
        setFormData((prev) => {
            const newGenreIds = prev.selectedGenreIds.includes(genreId)
                ? prev.selectedGenreIds.filter((id) => id !== genreId)
                : [...prev.selectedGenreIds, genreId];
            return { ...prev, selectedGenreIds: newGenreIds };
        });
    };

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setSaveStatus(null);

        const submissionData = new FormData();
        Object.entries(formData).forEach(([key, value]) => {
            if (key === 'selectedGenreIds' && Array.isArray(value)) {
                value.forEach((id) => submissionData.append(key, String(id)));
            } else if (value !== null && value !== undefined) {
                submissionData.append(key, String(value));
            }
        });

        if (avatarFile) submissionData.append('avatarImage', avatarFile);
        if (cardFile) submissionData.append('cardImage', cardFile);

        try {
            const config = { headers: { 'Content-Type': 'multipart/form-data' } };
            let response;
            if (formData.characterId === 0) {
                response = await axios.post(`/api/characters`, submissionData, config);
                window.location.href = `/User-Panel/My-Characters/Edit/${response.data.characterId}`;
            } else {
                response = await axios.post(`/api/characters/${formData.characterId}/details`, submissionData, config);
                setSaveStatus({ message: 'Details saved successfully!', type: 'success' });
                onSave();
            }
        } catch (error) {
            let errorMessage = 'An unexpected error occurred. Failed to save details.';
            if (axios.isAxiosError(error) && error.response) {
                if (error.response.data && error.response.data.errors) {
                    errorMessage = `Failed to save: ${Object.values(error.response.data.errors).flat().join(' ')}`;
                } else if (error.response.data && error.response.data.message) {
                    errorMessage = error.response.data.message;
                } else {
                    errorMessage = `An error occurred: ${error.response.status} ${error.response.statusText}`;
                }
            }
            setSaveStatus({ message: errorMessage, type: 'error' });
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="row g-4">
            {imageToCrop && (
                <ImageCropModal
                    src={imageToCrop}
                    onCropComplete={croppingAvatar ? handleAvatarCropComplete : handleCardCropComplete}
                    onClose={closeCropModal}
                    aspect={croppingAvatar ? 1 : 9 / 16}
                />
            )}
            {/* Left Column: The Form */}
            <div className="col-lg-7">
                <form onSubmit={handleSave}>
                    {/* --- Basic Info --- */}
                    <h5 className="mb-3">Basic Information</h5>
                    <div className="row">
                        <div className="col-md-6 mb-3">
                            <label htmlFor="characterDisplayName" className="form-label">
                                Display Name <span className="text-danger">*</span>
                            </label>
                            <input
                                type="text"
                                id="characterDisplayName"
                                name="characterDisplayName"
                                className="form-control"
                                value={formData.characterDisplayName || ''}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div className="col-md-6 mb-3">
                            <label htmlFor="characterFirstName" className="form-label">
                                First Name <span className="text-danger">*</span>
                            </label>
                            <input
                                type="text"
                                id="characterFirstName"
                                name="characterFirstName"
                                className="form-control"
                                value={formData.characterFirstName || ''}
                                onChange={handleChange}
                                required
                            />
                        </div>
                    </div>
                    {/* ... (rest of the form rows for names, gender, etc.) ... */}
                    <div className="row">
                        <div className="col-md-6 mb-3">
                            <label htmlFor="characterMiddleName" className="form-label">
                                Middle Name
                            </label>
                            <input
                                type="text"
                                id="characterMiddleName"
                                name="characterMiddleName"
                                className="form-control"
                                value={formData.characterMiddleName || ''}
                                onChange={handleChange}
                            />
                        </div>
                        <div className="col-md-6 mb-3">
                            <label htmlFor="characterLastName" className="form-label">
                                Last Name
                            </label>
                            <input
                                type="text"
                                id="characterLastName"
                                name="characterLastName"
                                className="form-control"
                                value={formData.characterLastName || ''}
                                onChange={handleChange}
                            />
                        </div>
                    </div>
                    <div className="row">
                        <div className="col-md-6 mb-3">
                            <label htmlFor="characterGender" className="form-label">
                                Gender
                            </label>
                            <select
                                id="characterGender"
                                name="characterGender"
                                className="form-select"
                                value={formData.characterGender ?? ''}
                                onChange={handleChange}
                            >
                                <option value="">- Select -</option>
                                {lookups.genders.map((g: { genderId: number; genderName: string }) => (
                                    <option key={g.genderId} value={g.genderId}>
                                        {g.genderName}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <div className="col-md-6 mb-3">
                            <label htmlFor="sexualOrientation" className="form-label">
                                Sexual Orientation
                            </label>
                            <select
                                id="sexualOrientation"
                                name="sexualOrientation"
                                className="form-select"
                                value={formData.sexualOrientation ?? ''}
                                onChange={handleChange}
                            >
                                <option value="">- Select -</option>
                                {lookups.sexualOrientations.map(
                                    (o: { sexualOrientationId: number; orientationName: string }) => (
                                        <option key={o.sexualOrientationId} value={o.sexualOrientationId}>
                                            {o.orientationName}
                                        </option>
                                    )
                                )}
                            </select>
                        </div>
                    </div>

                    <hr className="my-4" />

                    {/* --- Roleplaying Preferences --- */}
                    <h5 className="mb-3">Roleplaying Preferences</h5>
                    {/* ... (rest of the form rows for preferences, genres, etc.) ... */}
                    <div className="row">
                        <div className="col-md-6 mb-3">
                            <label htmlFor="lfrpStatus" className="form-label">
                                Roleplay Status
                            </label>
                            <select
                                id="lfrpStatus"
                                name="lfrpStatus"
                                className="form-select"
                                value={formData.lfrpStatus ?? ''}
                                onChange={handleChange}
                            >
                                {lookups.lfrpStatuses.map((s: { lfrpStatusId: number; statusName: string }) => (
                                    <option key={s.lfrpStatusId} value={s.lfrpStatusId}>
                                        {s.statusName}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <div className="col-md-6 mb-3">
                            <label htmlFor="literacyLevel" className="form-label">
                                Literacy Level
                            </label>
                            <select
                                id="literacyLevel"
                                name="literacyLevel"
                                className="form-select"
                                value={formData.literacyLevel ?? ''}
                                onChange={handleChange}
                            >
                                <option value="">- Select -</option>
                                {lookups.literacyLevels.map((l: { literacyLevelId: number; levelName: string }) => (
                                    <option key={l.literacyLevelId} value={l.literacyLevelId}>
                                        {l.levelName}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>
                    <div className="mb-3">
                        <label className="form-label">Genres</label>
                        <div className="border rounded p-2" style={{ maxHeight: '200px', overflowY: 'auto' }}>
                            <div className="row">
                                {lookups.genres.map((g: { genreId: number; genreName: string }) => (
                                    <div key={g.genreId} className="col-md-4 col-sm-6">
                                        <div className="form-check">
                                            <input
                                                type="checkbox"
                                                id={`genre-${g.genreId}`}
                                                className="form-check-input"
                                                checked={formData.selectedGenreIds.includes(g.genreId)}
                                                onChange={() => handleGenreChange(g.genreId)}
                                            />
                                            <label htmlFor={`genre-${g.genreId}`} className="form-check-label">
                                                {g.genreName}
                                            </label>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    <hr className="my-4" />

                    {/* --- Images --- */}
                    <h5 className="mb-3">Images</h5>
                    <div className="row">
                        <div className="col-md-6 mb-3">
                            <label htmlFor="avatarImage" className="form-label">
                                New Avatar Image
                            </label>
                            <input
                                type="file"
                                id="avatarImage"
                                name="avatarImage"
                                className="form-control"
                                onChange={handleAvatarFileChange}
                                accept="image/*"
                            />
                        </div>
                        <div className="col-md-6 mb-3">
                            <label htmlFor="cardImage" className="form-label">
                                New Card Image
                            </label>
                            <input
                                type="file"
                                id="cardImage"
                                name="cardImage"
                                className="form-control"
                                onChange={handleCardFileChange}
                                accept="image/*"
                            />
                        </div>
                    </div>

                    <hr className="my-4" />

                    {/* --- Settings --- */}
                    <h5 className="mb-3">Settings</h5>
                    <div className="row">
                        <div className="col-md-4 mb-3 form-check form-switch ps-5 pt-2">
                            <input
                                type="checkbox"
                                id="matureContent"
                                name="matureContent"
                                className="form-check-input"
                                checked={formData.matureContent}
                                onChange={handleChange}
                            />
                            <label htmlFor="matureContent" className="form-check-label">
                                Contains Mature Content
                            </label>
                        </div>
                        <div className="col-md-4 mb-3 form-check form-switch ps-5 pt-2">
                            <input
                                type="checkbox"
                                id="isPrivate"
                                name="isPrivate"
                                className="form-check-input"
                                checked={formData.isPrivate}
                                onChange={handleChange}
                            />
                            <label htmlFor="isPrivate" className="form-check-label">
                                Private Character
                            </label>
                        </div>
                        <div className="col-md-4 mb-3 form-check form-switch ps-5 pt-2">
                            <input
                                type="checkbox"
                                id="disableLinkify"
                                name="disableLinkify"
                                className="form-check-input"
                                checked={formData.disableLinkify}
                                onChange={handleChange}
                            />
                            <label htmlFor="disableLinkify" className="form-check-label">
                                Disable Auto-linking
                            </label>
                        </div>
                    </div>

                    {/* --- Save Button & Status --- */}
                    <div className="d-flex justify-content-end mt-4">
                        <button type="submit" className="btn btn-primary" disabled={isSaving}>
                            {isSaving ? 'Saving...' : 'Save Details'}
                        </button>
                    </div>

                    {saveStatus && <div className={`mt-3 alert alert-${saveStatus.type}`}>{saveStatus.message}</div>}
                </form>
            </div>

            {/* Right Column: The Preview */}
            <div className="col-lg-5">
                <CharacterCardPreview
                    displayName={formData.characterDisplayName}
                    cardImageUrl={cardUrl}
                    avatarImageUrl={avatarUrl}
                />
            </div>
        </div>
    );
};

export default DetailsTab;
