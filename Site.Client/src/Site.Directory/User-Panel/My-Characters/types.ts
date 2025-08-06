export interface Character {
    characterId: number;
    characterDisplayName: string;
    characterFirstName: string;
    characterMiddleName: string | null;
    characterLastName: string | null;
    characterBio: string | null;
    characterGender: number | null;
    sexualOrientation: number | null;
    characterSourceId: number | null;
    postLengthMin: number | null;
    postLengthMax: number | null;
    literacyLevel: number | null;
    lfrpStatus: number;
    eroticaPreferences: number | null;
    matureContent: boolean;
    isPrivate: boolean;
    disableLinkify: boolean;
    cardImageUrl: string | null;
    profileCss: string | null;
    profileHtml: string | null;
    customProfileEnabled: boolean;
}

export interface CharacterImage {
    characterImageId: number;
    characterImageUrl: string;
    imageCaption: string | null;
    isPrimary: boolean;
}

export interface CharacterInline {
    inlineId: number;
    inlineName: string;
    inlineImageUrl: string;
}

export interface EditorData {
    character: Character;
    selectedGenreIds: number[];
    images: CharacterImage[];
    inlines: CharacterInline[];
    avatarUrl: string | null;
    cardUrl: string | null;
}

export interface EditorLookups {
    genders: { genderId: number; genderName: string }[];
    sexualOrientations: { sexualOrientationId: number; orientationName: string }[];
    sources: { sourceId: number; sourceName: string }[];
    postLengths: { postLengthId: number; postLengthName: string }[];
    literacyLevels: { literacyLevelId: number; levelName: string }[];
    lfrpStatuses: { lfrpStatusId: number; statusName: string }[];
    eroticaPreferences: { eroticaPreferenceId: number; preferenceName: string }[];
    genres: { genreId: number; genreName: string }[];
}

export type EditorTab = 'Details' | 'BBFrame' | 'Gallery' | 'Customize';