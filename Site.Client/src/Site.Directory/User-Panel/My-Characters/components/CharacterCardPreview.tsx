import React, { useState } from 'react';

interface CharacterCardPreviewProps {
    displayName: string;
    cardImageUrl: string | null;
    avatarImageUrl: string | null;
}

const CharacterCardPreview: React.FC<CharacterCardPreviewProps> = ({ displayName, cardImageUrl, avatarImageUrl }) => {
    const [activeView, setActiveView] = useState<'card' | 'avatar'>('card');

    const defaultCardUrl = "/images/Defaults/NewCharacter.png";
    const defaultAvatarUrl = "/images/Defaults/NewAvatar.png";

    return (
        <div className="character-card-container sticky-top">
            <div className="card character-card text-center h-100">
                <div className="card-header p-2">
                    <h6 className="card-title m-0 text-truncate">{displayName || 'Character Name'}</h6>
                </div>

                <div className="card-content-wrapper">
                    {/* Card View */}
                    <div className={`card-img-wrapper ${activeView !== 'card' ? 'd-none' : ''}`}>
                        <img src={cardImageUrl || defaultCardUrl} className="rpg-img img-fluid" alt="Card Preview" />
                    </div>

                    {/* Avatar View */}
                    <div className={`avatar-view-container ${activeView !== 'avatar' ? 'd-none' : ''}`}>
                        <img src={avatarImageUrl || defaultAvatarUrl} className="img-thumbnail" style={{ width: '150px', height: '150px' }} alt="Avatar Large" />
                        <div className="d-flex justify-content-center gap-3 mt-3">
                            <img src={avatarImageUrl || defaultAvatarUrl} className="img-thumbnail" style={{ width: '75px', height: '75px' }} alt="Avatar Medium" />
                            <img src={avatarImageUrl || defaultAvatarUrl} className="img-thumbnail" style={{ width: '50px', height: '50px' }} alt="Avatar Small" />
                        </div>
                    </div>
                </div>
                <div className="card-footer p-1 text-decoration-none">
                    <small>Live Preview</small>
                </div>
            </div>

            {/* Controls */}
            <div className="character-card-controls">
                <div className="controls-box">
                    <button className={`view-toggle-btn ${activeView === 'card' ? 'active' : ''}`} onClick={() => setActiveView('card')} title="Card View">
                        <i className="bi bi-card-image"></i>
                    </button>
                    <button className={`view-toggle-btn ${activeView === 'avatar' ? 'active' : ''}`} onClick={() => setActiveView('avatar')} title="Avatar View">
                        <i className="bi bi-person-square"></i>
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CharacterCardPreview;