using RoleplayersGuild.Site.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface ICharacterDataService : IBaseDataService
    {
        Task<Character?> GetCharacterAsync(int characterId);
        Task<int> GetCharacterCountAsync(int userId);
        Task<int> CreateNewCharacterAsync(int userId);
        Task DeleteCharacterAsync(int characterId);
        Task UpdateCharacterAsync(CharacterInputModel model);
        Task UpdateCharacterGenresAsync(int characterId, List<int> genreIds);
        Task UpdateCharacterBadgeAssignmentAsync(int characterId, int userBadgeId);
        Task UpdateCharacterBBFrameAsync(int characterId, string bbframeContent);
        Task<int> GetAssignedUserBadgeIdAsync(int characterId);
        Task<IEnumerable<Character>> GetActiveCharactersForUserAsync(int userId);
        Task<CharacterImage?> GetImageAsync(int imageId);
        Task<int> AddImageAsync(string imageUrl, int characterId, int userId, bool isMature, string imageCaption, int width, int height);
        Task UpdateImageAsync(int imageId, bool isMature, string imageCaption);
        Task<CharacterImage?> GetImageWithOwnerAsync(int imageId);
        Task<int> GetAvailableImageSlotCountAsync(int userId, int characterId);
        Task UpdateImageDetailsAsync(int imageId, string caption, double? imageScale);
        Task UpdateImageSizeAsync(int imageId, int width, int height);
        Task UpdateImagePositionsAsync(List<int> imageIds);
        Task DeleteImageRecordAsync(int imageId);
        Task DeleteImagesAsync(List<int> imageIds, int userId);
        Task<IEnumerable<CharactersForListing>> GetCharactersForListingAsync(string screenStatus, int recordCount, int currentUserId);
        Task<CharacterWithDetails?> GetCharacterWithDetailsAsync(int characterId);
        Task<IEnumerable<Genre>> GetCharacterGenresAsync(int characterId);
        Task<PagedResult<CharactersForListing>> SearchCharactersAsync(SearchInputModel search, int currentUserId, int pageIndex, int pageSize);
        Task<PagedResult<CharactersForListing>> SearchUserCharactersAsync(int userId, SearchInputModel search, int pageIndex, int pageSize);
        Task<IEnumerable<ImageCommentViewModel>> GetImageCommentsAsync(int imageId);
        Task AddImageCommentAsync(int imageId, int characterId, string commentText);
        Task DeleteImageCommentAsync(int commentId, int userId);
        Task<PagedResult<CharacterImage>> GetCharacterImagesAsync(int characterId, int pageIndex, int pageSize);
        Task<CharacterImageWithDetails?> GetImageDetailsAsync(int imageId);
        Task<Character?> GetCharacterForEditAsync(int characterId, int userId);
        Task UpdateCharacterCustomProfileAsync(int characterId, string? css, string? html, bool isEnabled);
        Task<IEnumerable<CharacterImage>> GetCharacterImagesForGalleryAsync(int characterId);
        Task UpsertCharacterAvatarAsync(int characterId, string avatarUrl);
        Task<int> AddInlineImageAsync(string imageUrl, int characterId, int userId, string inlineName);
        Task<CharacterInline?> GetInlineImageAsync(int inlineId);
        Task DeleteInlineImageRecordAsync(int inlineId);
        Task<IEnumerable<CharacterInline>> GetInlineImagesAsync(int characterId);
        Task<CharacterAvatar?> GetCharacterAvatarAsync(int characterId);
        Task<IEnumerable<CharacterRecipient>> GetCharacterAndUserIdsByDisplayNamesAsync(List<string> displayNames);
        Task<int> GetUnapprovedCharacterCountAsync();
        Task<CharacterInline?> GetCharacterInlineAsync(int characterId, int inlineId);
        Task<User?> GetImageOwnerAsync(int imageId);
        Task SetCharacterStatusAsync(int characterId, int statusId);
        Task<ChatCharacterViewModel?> GetCharacterForChatAsync(int characterId);
        Task<int> GetTotalCharacterCountAsync();
        Task IncrementCharacterViewCountAsync(int characterId);
    }
}
