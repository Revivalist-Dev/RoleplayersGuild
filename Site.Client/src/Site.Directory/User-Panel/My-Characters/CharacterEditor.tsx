import React, { useState, useEffect, useCallback } from 'react';
import axios from 'axios';

// Import Components
import DetailsTab from './components/DetailsTab';
import GalleryTab from './components/GalleryTab';
import BBFrameTab from './components/BBFrameTab';
import CustomizeTab from './components/CustomizeTab';
import LoadingSpinner from '../../Shared/Components/LoadingSpinner'; // ADDED: Import the new LoadingSpinner component

// Import Shared Types
import { Character, EditorData, EditorLookups, EditorTab } from './types';

interface CharacterEditorProps {
    characterId: number;
}

const CharacterEditor: React.FC<CharacterEditorProps> = ({ characterId }) => {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [editorData, setEditorData] = useState<EditorData | null>(null);
    const [lookupData, setLookupData] = useState<EditorLookups | null>(null);
    const [activeTab, setActiveTab] = useState<EditorTab>('Details');

    const fetchInitialData = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const [characterResponse, lookupsResponse] = await Promise.all([
                characterId > 0 ? axios.get<EditorData>(`/api/characters/${characterId}`) : Promise.resolve(null),
                axios.get<EditorLookups>('/api/characters/editor-lookups')
            ]);

            setLookupData(lookupsResponse.data);

            if (characterResponse) {
                setEditorData(characterResponse.data);
            } else {
                const newCharacter: Character = {
                    characterId: 0,
                    characterDisplayName: '',
                    characterFirstName: '',
                    characterMiddleName: '',
                    characterLastName: '',
                    characterBBFrame: '',
                    characterGender: null,
                    sexualOrientation: null,
                    characterSourceId: null,
                    postLengthMin: null,
                    postLengthMax: null,
                    literacyLevel: null,
                    lfrpStatus: 1,
                    eroticaPreferences: null,
                    matureContent: false,
                    isPrivate: false,
                    disableLinkify: false,
                    cardImageUrl: null,
                    profileCss: '',
                    profileHtml: '',
                    customProfileEnabled: false,
                };
                setEditorData({
                    character: newCharacter,
                    selectedGenreIds: [],
                    images: [],
                    inlines: [],
                    avatarUrl: null,
                    cardUrl: null
                });
            }
        } catch (err) {
            setError('Failed to load character data. Please try again.');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [characterId]);

    useEffect(() => {
        fetchInitialData();
    }, [fetchInitialData]);


    const renderActiveTab = () => {
        if (!editorData || !lookupData) return null;

        switch (activeTab) {
            case 'Details':
                return <DetailsTab
                    character={editorData.character}
                    lookups={lookupData}
                    selectedGenres={editorData.selectedGenreIds}
                    onSave={fetchInitialData}
                    initialAvatarUrl={editorData.avatarUrl}
                    initialCardUrl={editorData.cardUrl}
                />;
            case 'Gallery':
                return <GalleryTab
                    characterId={characterId}
                    initialImages={editorData.images}
                    onGalleryUpdate={fetchInitialData}
                />;
            case 'BBFrame':
                return <BBFrameTab
                    characterId={characterId}
                    initialBBFrame={editorData.character.characterBBFrame}
                    initialInlines={editorData.inlines}
                    onUpdate={fetchInitialData}
                />;
            case 'Customize':
                return <CustomizeTab
                    character={editorData.character}
                    onUpdate={fetchInitialData}
                />;
            default:
                return null;
        }
    };

    if (loading) {
        // CHANGED: Replaced the default Bootstrap spinner with the new custom component
        return (
            <div className="d-flex justify-content-center align-items-center my-5 p-5">
                <LoadingSpinner />
            </div>
        );
    }

    if (error) {
        return <div className="alert alert-danger">{error}</div>;
    }

    const isNewCharacter = characterId === 0;

    return (
        <div className="card">
            <div className="card-header">
                <ul className="nav nav-tabs card-header-tabs">
                    <li className="nav-item">
                        <button className={`nav-link ${activeTab === 'Details' ? 'active' : ''}`} onClick={() => setActiveTab('Details')}>Details</button>
                    </li>
                    <li className="nav-item">
                        <button className={`nav-link ${activeTab === 'BBFrame' ? 'active' : ''}`} disabled={isNewCharacter} onClick={() => setActiveTab('BBFrame')}>BBFrame & Inlines</button>
                    </li>
                    <li className="nav-item">
                        <button className={`nav-link ${activeTab === 'Gallery' ? 'active' : ''}`} disabled={isNewCharacter} onClick={() => setActiveTab('Gallery')}>Gallery</button>
                    </li>
                    <li className="nav-item">
                        <button className={`nav-link ${activeTab === 'Customize' ? 'active' : ''}`} disabled={isNewCharacter} onClick={() => setActiveTab('Customize')}>Customize</button>
                    </li>
                </ul>
            </div>
            <div className="card-body p-3">
                {renderActiveTab()}
            </div>
        </div>
    );
};

export default CharacterEditor;