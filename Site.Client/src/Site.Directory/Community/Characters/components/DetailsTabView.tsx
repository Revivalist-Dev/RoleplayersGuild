import React from 'react';
import { Character } from '../../../../types';

interface DetailsTabViewProps {
    character: Character;
    genres: string[];
}

const DetailsTabView: React.FC<DetailsTabViewProps> = ({ character, genres }) => {
    return (
        <div>
            <p className="mb-1">
                <strong>Full Name:</strong> {character.characterFirstName} {character.characterMiddleName}{' '}
                {character.characterLastName}
            </p>
            <p className="mb-1">
                <strong>Gender:</strong> {character.characterGender}
            </p>
            <p className="mb-1">
                <strong>Sexual Orientation:</strong> {character.sexualOrientation}
            </p>
            {character.universeName && (
                <p className="mb-0">
                    <strong>Universe:</strong>{' '}
                    <a href={`/Community/Universes/View/${character.universeId}`}>{character.universeName}</a>
                </p>
            )}
            <hr />
            <ul className="list-unstyled">
                <li>
                    <strong>Contact Pref:</strong> {character.lfrpStatusName ?? character.lfrpStatus}
                </li>
                <li>
                    <strong>Source:</strong> {character.characterSource ?? character.characterSourceId}
                </li>
                <li>
                    <strong>Post Length:</strong> {character.postLengthMin} to {character.postLengthMax}
                </li>
                <li>
                    <strong>Literacy Level:</strong> {character.literacyLevel}
                </li>
                <li>
                    <strong>Genres:</strong> {genres.join(', ')}
                </li>
            </ul>
        </div>
    );
};

export default DetailsTabView;
