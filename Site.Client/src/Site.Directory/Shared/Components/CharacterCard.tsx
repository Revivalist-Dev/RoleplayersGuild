import React, { useState } from 'react';

// Define the props the component will accept
export interface CharacterCardProps {
    characterId: number;
    displayName: string;
    cardImageUrl: string | null;
    avatarImageUrl: string | null;
    showAdminControls?: boolean;
}

const CharacterCard: React.FC<CharacterCardProps> = ({
    characterId,
    displayName,
    cardImageUrl,
    avatarImageUrl,
    showAdminControls = false,
}) => {
    const [activeView, setActiveView] = useState<'card' | 'avatar'>('card');

    const defaultAvatarUrl = '/images/Defaults/NewAvatar.png';
    const defaultCardUrl = '/images/Defaults/NewCharacter.png';

    const profileUrl = `/Community/Characters/View/${characterId}`;
    const editUrl = `/User-Panel/My-Characters/Edit/${characterId}`;

    return (
        <div className="character-card-container">
            {/* Main Card */}
            <div className="card character-card text-center h-100">
                <div className="card-header p-2">
                    <h6 className="card-title m-0 text-truncate">{displayName}</h6>
                </div>
                <div className="card-content-wrapper">
                    <div className={`card-img-wrapper ${activeView !== 'card' ? 'd-none' : ''}`}>
                        <img src={cardImageUrl || defaultCardUrl} className="rpg-img img-fluid" alt={displayName} />
                    </div>
                    <div className={`avatar-view-container p-3 ${activeView !== 'avatar' ? 'd-none' : ''}`}>
                        <img
                            src={avatarImageUrl || defaultAvatarUrl}
                            className="img-thumbnail mb-2"
                            style={{ width: '150px', height: '150px' }}
                            alt="Avatar Large"
                        />
                        <div className="d-flex justify-content-center gap-2">
                            <img
                                src={avatarImageUrl || defaultAvatarUrl}
                                className="img-thumbnail"
                                style={{ width: '75px', height: '75px' }}
                                alt="Avatar Medium"
                            />
                            <img
                                src={avatarImageUrl || defaultAvatarUrl}
                                className="img-thumbnail"
                                style={{ width: '50px', height: '50px' }}
                                alt="Avatar Small"
                            />
                        </div>
                    </div>
                </div>
                {showAdminControls ? (
                    <div className="card-footer p-1 d-flex justify-content-around">
                        <a className="btn btn-sm btn-primary" href={editUrl} title="Edit">
                            <i className="bi bi-pencil"></i> Edit
                        </a>
                        <a className="btn btn-sm btn-secondary" href={profileUrl} title="View">
                            <i className="bi bi-eye"></i> View
                        </a>
                    </div>
                ) : (
                    <a href={profileUrl} className="card-footer p-1 text-decoration-none">
                        <small>View Profile</small>
                    </a>
                )}
            </div>
            {/* Controls Sidebar */}
            <div className="character-card-controls">
                <div className="controls-box">
                    <button
                        className={`view-toggle-btn ${activeView === 'card' ? 'active' : ''}`}
                        onClick={() => setActiveView('card')}
                        title="Card View"
                    >
                        <i className="bi bi-card-image"></i>
                    </button>
                    <button
                        className={`view-toggle-btn ${activeView === 'avatar' ? 'active' : ''}`}
                        onClick={() => setActiveView('avatar')}
                        title="Avatar View"
                    >
                        <i className="bi bi-person-square"></i>
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CharacterCard;
