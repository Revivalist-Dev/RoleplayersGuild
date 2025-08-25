import React, { useState, useEffect } from 'react';
import { Character, CharacterImage } from '../../../types';
import BBFrameTabView from './components/BBFrameTabView';
import DataTabView from './components/DataTabView';
import DetailsTabView from './components/DetailsTabView';
import GalleryTabView from './components/GalleryTabView';

interface CharacterViewerProps {
    character: Character;
    images: CharacterImage[];
    bbFrameHtml: string;
    isOwner: boolean;
    userCanViewMatureContent: boolean;
    genres: string[];
}

const CharacterViewer: React.FC<CharacterViewerProps> = ({
    character,
    images,
    bbFrameHtml,
    isOwner,
    userCanViewMatureContent,
    genres,
}) => {
    const [activeTab, setActiveTab] = useState<'BBFrame' | 'Details' | 'Gallery' | 'Data'>('BBFrame');

    useEffect(() => {}, [activeTab]);

    const renderTabContent = () => {
        switch (activeTab) {
            case 'BBFrame':
                return (
                    <BBFrameTabView
                        bbFrameHtml={bbFrameHtml}
                        matureContent={character.matureContent}
                        userCanViewMatureContent={userCanViewMatureContent}
                    />
                );
            case 'Details':
                return <DetailsTabView character={character} genres={genres} />;
            case 'Gallery':
                return <GalleryTabView images={images} />;
            case 'Data':
                return <DataTabView />;
            default:
                return null;
        }
    };

    return (
        <div className="card">
            <div className="card-header">
                <ul className="nav nav-tabs card-header-tabs" id="profileTab" role="tablist">
                    <li className="nav-item" role="presentation">
                        <button
                            className={`nav-link ${activeTab === 'BBFrame' ? 'active' : ''}`}
                            onClick={() => setActiveTab('BBFrame')}
                        >
                            BBFrame
                        </button>
                    </li>
                    <li className="nav-item" role="presentation">
                        <button
                            className={`nav-link ${activeTab === 'Details' ? 'active' : ''}`}
                            onClick={() => setActiveTab('Details')}
                        >
                            Details
                        </button>
                    </li>
                    <li className="nav-item" role="presentation">
                        <button
                            className={`nav-link ${activeTab === 'Gallery' ? 'active' : ''}`}
                            onClick={() => setActiveTab('Gallery')}
                        >
                            Gallery
                        </button>
                    </li>
                    <li className="nav-item" role="presentation">
                        <button
                            className={`nav-link ${activeTab === 'Data' ? 'active' : ''}`}
                            onClick={() => setActiveTab('Data')}
                        >
                            Data
                        </button>
                    </li>
                </ul>
            </div>
            <div className={`card-body`}>
                <div className="tab-content" id="profileTabContent">
                    <div className="tab-pane fade show active" role="tabpanel">
                        {renderTabContent()}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CharacterViewer;
