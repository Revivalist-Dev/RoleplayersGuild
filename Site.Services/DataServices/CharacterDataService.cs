using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public class CharacterDataService : BaseDataService, ICharacterDataService
    {
        private readonly IImageService _imageService;
        private readonly IUrlProcessingService _urlProcessingService;
        private readonly ImageSettings _imageSettings;

        public CharacterDataService(IConfiguration config, IImageService imageService, IUrlProcessingService urlProcessingService, IOptions<ImageSettings> imageSettings) : base(config)
        {
            _imageService = imageService;
            _urlProcessingService = urlProcessingService;
            _imageSettings = imageSettings.Value;
        }

        public Task<Character?> GetCharacterAsync(int characterId) => GetRecordAsync<Character>("""SELECT * FROM "Characters" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        public Task<int> GetCharacterCountAsync(int userId) => GetScalarAsync<int>("""SELECT COUNT("CharacterId") FROM "Characters" WHERE "UserId" = @UserId""", new { UserId = userId });
        public Task<int> CreateNewCharacterAsync(int userId) =>
            GetScalarAsync<int>("""INSERT INTO "Characters" ("UserId", "IsApproved", "IsActive") VALUES (@UserId, TRUE, TRUE) RETURNING "CharacterId";""", new { UserId = userId });
        
        public async Task DeleteCharacterAsync(int characterId)
        {
            var images = await GetRecordsAsync<CharacterImage>("""SELECT "CharacterImageUrl" FROM "CharacterImages" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
            foreach (var image in images)
            {
                await _imageService.DeleteImageAsync((ImageUploadPath)image.CharacterImageUrl);
            }
            await ExecuteAsync("""DELETE FROM "Characters" WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        }

        public Task UpdateCharacterAsync(CharacterInputModel model)
        {
            const string sql = """
                         UPDATE "Characters" SET 
                             "CharacterDisplayName" = @CharacterDisplayName, 
                             "CharacterFirstName" = @CharacterFirstName, 
                             "CharacterMiddleName" = @CharacterMiddleName, 
                             "CharacterLastName" = @CharacterLastName, 
                             "CharacterBBFrame" = @CharacterBBFrame,
                             "CharacterGender" = @CharacterGender, 
                             "SexualOrientation" = @SexualOrientation, 
                             "CharacterSourceId" = @CharacterSourceId, 
                             "PostLengthMin" = @PostLengthMin, 
                             "PostLengthMax" = @PostLengthMax, 
                             "LiteracyLevel" = @LiteracyLevel, 
                             "LfrpStatus" = @LfrpStatus, 
                             "EroticaPreferences" = @EroticaPreferences, 
                             "MatureContent" = @MatureContent, 
                             "IsPrivate" = @IsPrivate, 
                             "DisableLinkify" = @DisableLinkify,
                             "CardImageUrl" = @CardImageUrl
                         WHERE "CharacterId" = @CharacterId
                         """;
            return ExecuteAsync(sql, model);
        }

        public async Task UpdateCharacterGenresAsync(int characterId, List<int> genreIds)
        {
            await ExecuteAsync("""DELETE FROM "CharacterGenres" WHERE "CharacterId" = @characterId""", new { characterId });
            if (genreIds is not null && genreIds.Any())
            {
                var sql = """INSERT INTO "CharacterGenres" ("CharacterId", "GenreId") VALUES (@characterId, @genreId);""";
                foreach (var genreId in genreIds) { await ExecuteAsync(sql, new { characterId, genreId }); }
            }
        }

        public Task UpdateCharacterBadgeAssignmentAsync(int characterId, int userBadgeId)
        {
            const string sql = """UPDATE "UserBadges" SET "AssignedToCharacterId" = 0 WHERE "AssignedToCharacterId" = @characterId; UPDATE "UserBadges" SET "AssignedToCharacterId" = @characterId WHERE "UserBadgeId" = @userBadgeId;""";
            return ExecuteAsync(sql, new { characterId, userBadgeId });
        }

        public Task UpdateCharacterBBFrameAsync(int characterId, string bbframeContent)
        {
            const string sql = """
                       UPDATE "Characters" 
                       SET "CharacterBBFrame" = @BBFrameContent
                       WHERE "CharacterId" = @CharacterId
                       """;
            return ExecuteAsync(sql, new { CharacterId = characterId, BBFrameContent = bbframeContent });
        }

        public Task<int> GetAssignedUserBadgeIdAsync(int characterId) => GetScalarAsync<int>("""SELECT "UserBadgeId" FROM "UserBadges" WHERE "AssignedToCharacterId" = @characterId""", new { characterId });
        public Task<IEnumerable<Character>> GetActiveCharactersForUserAsync(int userId) => GetRecordsAsync<Character>("""SELECT * FROM "CharactersForListing" WHERE "UserId" = @userId AND "IsApproved" = TRUE""", new { userId });
        public Task<CharacterImage?> GetImageAsync(int imageId) => GetRecordAsync<CharacterImage>("""SELECT CI.*, C."UserId" FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE CI."CharacterImageId" = @imageId""", new { imageId });
        public Task<int> AddImageAsync(string imageUrl, int characterId, int userId, bool isMature, string imageCaption, int width, int height) =>
            GetScalarAsync<int>("""
            INSERT INTO "CharacterImages"
                ("CharacterImageUrl", "CharacterId", "UserId", "IsMature", "ImageCaption", "Width", "Height", "ImageScale")
            VALUES
                (@ImageUrl, @CharacterId, @UserId, @IsMature, @ImageCaption, @Width, @Height, 0)
            RETURNING "CharacterImageId";
            """,
                new { ImageUrl = imageUrl, CharacterId = characterId, UserId = userId, IsMature = isMature, ImageCaption = imageCaption, Width = width, Height = height });
        public Task UpdateImageAsync(int imageId, bool isMature, string imageCaption) => ExecuteAsync("""UPDATE "CharacterImages" SET "IsMature" = @IsMature, "ImageCaption" = @ImageCaption WHERE "CharacterImageId" = @imageId""", new { imageId, isMature, imageCaption });
        public Task<CharacterImage?> GetImageWithOwnerAsync(int imageId) => GetRecordAsync<CharacterImage>("""SELECT CI.*, C."UserId" FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE CI."CharacterImageId" = @imageId""", new { imageId });

        public async Task<int> GetAvailableImageSlotCountAsync(int userId, int characterId)
        {
            var membershipTypeId = await GetScalarAsync<int>("""SELECT "MembershipTypeId" FROM "Memberships" WHERE "UserId" = @UserId AND ("EndDate" IS NULL OR "EndDate" > NOW()) ORDER BY "StartDate" DESC LIMIT 1""", new { UserId = userId });
            
            int maxImages = membershipTypeId switch
            {
                1 => _imageSettings.BronzeMemberMax,
                2 => _imageSettings.SilverMemberMax,
                3 => _imageSettings.GoldMemberMax,
                4 => _imageSettings.PlatinumMemberMax,
                _ => _imageSettings.MaxPerCharacter,
            };
            var usedSlots = await GetScalarAsync<int>("""SELECT COUNT("CharacterImageId") FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE C."UserId" = @userId""", new { userId });
            return maxImages - usedSlots > 0 ? maxImages - usedSlots : 0;
        }

        public Task UpdateImageDetailsAsync(int imageId, string caption, int imageScale) => ExecuteAsync("""UPDATE "CharacterImages" SET "ImageCaption" = @caption, "ImageScale" = @imageScale WHERE "CharacterImageId" = @imageId""", new { imageId, caption, imageScale });
        public Task DeleteImageRecordAsync(int imageId) => ExecuteAsync("""DELETE FROM "CharacterImages" WHERE "CharacterImageId" = @ImageId""", new { ImageId = imageId });
        public async Task UpdateImagePositionsAsync(List<int> imageIds)
        {
            var sql = """
                UPDATE "CharacterImages"
                SET "SortOrder" = CASE "CharacterImageId"
            """;
            for (int i = 0; i < imageIds.Count; i++)
            {
                sql += $" WHEN {imageIds[i]} THEN {i}";
            }
            sql += """
                END
                WHERE "CharacterImageId" = ANY(@ImageIds)
            """;

            await ExecuteAsync(sql, new { ImageIds = imageIds });
        }

        public async Task<IEnumerable<CharactersForListing>> GetCharactersForListingAsync(string screenStatus, int recordCount, int currentUserId)
        {
            var whereClauses = new List<string>
            {
                """u."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @currentUserId)""",
                """u."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @currentUserId)"""
            };

            string orderByClause = screenStatus == "OnlineCharacters"
                ? """ORDER BY u."LastAction" DESC"""
                : """ORDER BY c."DateSubmitted" DESC""";

            if (screenStatus == "OnlineCharacters")
            {
                whereClauses.Add("""u."LastAction" > (NOW() AT TIME ZONE 'UTC' - interval '15 minute') AND u."ShowWhenOnline" = TRUE""");
            }

            string sql = $"""
        SELECT c."CharacterId", c."UserId", c."CharacterDisplayName", c."CardImageUrl", u."LastAction", u."ShowWhenOnline",
               ca."AvatarImageUrl", (SELECT B."CharacterNameClass" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId"
                                     WHERE UB."UserId" = c."UserId" AND B."CharacterNameClass" IS NOT NULL
                                     ORDER BY B."SortOrder" LIMIT 1) AS "CharacterNameClass"
        FROM "Characters" c
        JOIN "Users" u ON c."UserId" = u."UserId"
        LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
        WHERE {string.Join(" AND ", whereClauses)}
        {orderByClause}
        LIMIT @recordCount
        """;

            var characters = await GetRecordsAsync<CharactersForListing>(sql, new { recordCount, currentUserId });
            foreach (var character in characters)
            {
                character.CardImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.CardImageUrl);
                character.AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.AvatarImageUrl);
            }
            return characters;
        }

        public async Task<CharacterWithDetails?> GetCharacterWithDetailsAsync(int characterId)
        {
            const string sql = """SELECT * FROM "CharactersWithDetails" WHERE "CharacterId" = @characterId""";
            var character = await GetRecordAsync<CharacterWithDetails>(sql, new { characterId });

            if (character != null)
            {
                ImageProcessingHelpers.ProcessCharacterDetails(character, _urlProcessingService);
            }
            return character;
        }

        public Task<IEnumerable<Genre>> GetCharacterGenresAsync(int characterId) => GetRecordsAsync<Genre>("""SELECT G."GenreId", G."GenreName" FROM "Genres" G JOIN "CharacterGenres" CG ON G."GenreId" = CG."GenreId" WHERE CG."CharacterId" = @characterId ORDER BY G."GenreName" """, new { characterId });

        public async Task<PagedResult<CharactersForListing>> SearchCharactersAsync(SearchInputModel search, int currentUserId, int pageIndex, int pageSize)
        {
            var whereClauses = new List<string> { """c."IsApproved" = TRUE AND c."IsPrivate" = FALSE""" };
            var parameters = new DynamicParameters();
            parameters.Add("CurrentUserId", currentUserId);
            whereClauses.Add("""(u."UserId" NOT IN (SELECT "UserBlocked" FROM "UserBlocking" WHERE "UserBlockedBy" = @CurrentUserId) AND u."UserId" NOT IN (SELECT "UserBlockedBy" FROM "UserBlocking" WHERE "UserBlocked" = @CurrentUserId))""");

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                if (int.TryParse(search.Name, out var characterId))
                {
                    whereClauses.Add("""c."CharacterId" = @CharacterId""");
                    parameters.Add("CharacterId", characterId);
                }
                else
                {
                    whereClauses.Add("""(c."CharacterDisplayName" ILIKE @Name OR c."CharacterFirstName" ILIKE @Name)""");
                    parameters.Add("Name", $"%{search.Name}%");
                }
            }
            if (search.GenderId > 0)
            {
                whereClauses.Add("""c."CharacterGender" = @GenderId""");
                parameters.Add("GenderId", search.GenderId);
            }
            if (search.SelectedGenreIds.Any())
            {
                whereClauses.Add("""c."CharacterId" IN (SELECT "CharacterId" FROM "CharacterGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", search.SelectedGenreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var fromSql = $"""
    FROM "Characters" c
    JOIN "Users" u ON c."UserId" = u."UserId"
    LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
    WHERE {whereSql}
    """;

            var countSql = $"SELECT COUNT(c.\"CharacterId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);
            if (totalCount == 0) return new PagedResult<CharactersForListing> { Items = Enumerable.Empty<CharactersForListing>(), PageIndex = pageIndex, PageSize = pageSize };

            string orderByClause = search.SortOrder switch { 1 => """ORDER BY c."CharacterId" ASC""", _ => """ORDER BY c."DateSubmitted" DESC NULLS LAST, c."CharacterId" DESC""", };
            var pagingSql = $"""
    SELECT c."CharacterId", c."UserId", c."CharacterDisplayName", c."CardImageUrl", u."LastAction", u."ShowWhenOnline",
           ca."AvatarImageUrl", (SELECT B."CharacterNameClass" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId" WHERE UB."UserId" = c."UserId" AND B."CharacterNameClass" IS NOT NULL ORDER BY B."SortOrder" LIMIT 1) AS "CharacterNameClass"
    {fromSql}
    {orderByClause}
    LIMIT @Take OFFSET @Skip
    """;
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = await GetRecordsAsync<CharactersForListing>(pagingSql, parameters);
            foreach (var item in items)
            {
                item.CardImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)item.CardImageUrl);
                item.AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)item.AvatarImageUrl);
            }

            return new PagedResult<CharactersForListing>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<CharactersForListing>> SearchUserCharactersAsync(int userId, SearchInputModel search, int pageIndex, int pageSize)
        {
            var whereClauses = new List<string> { """c."UserId" = @UserId""" };
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                if (int.TryParse(search.Name, out var characterId))
                {
                    whereClauses.Add("""c."CharacterId" = @CharacterId""");
                    parameters.Add("CharacterId", characterId);
                }
                else
                {
                    whereClauses.Add("""(c."CharacterDisplayName" ILIKE @Name OR c."CharacterFirstName" ILIKE @Name)""");
                    parameters.Add("Name", $"%{search.Name}%");
                }
            }
            if (search.GenderId > 0)
            {
                whereClauses.Add("""c."CharacterGender" = @GenderId""");
                parameters.Add("GenderId", search.GenderId);
            }
            if (search.SelectedGenreIds.Any())
            {
                whereClauses.Add("""c."CharacterId" IN (SELECT "CharacterId" FROM "CharacterGenres" WHERE "GenreId" = ANY(@GenreIds))""");
                parameters.Add("GenreIds", search.SelectedGenreIds);
            }

            var whereSql = string.Join(" AND ", whereClauses);
            var fromSql = $"""
    FROM "Characters" c
    JOIN "Users" u ON c."UserId" = u."UserId"
    LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
    WHERE {whereSql}
    """;

            var countSql = $"SELECT COUNT(c.\"CharacterId\") {fromSql}";
            var totalCount = await GetScalarAsync<int>(countSql, parameters);
            if (totalCount == 0)
            {
                return new PagedResult<CharactersForListing> { Items = Enumerable.Empty<CharactersForListing>(), PageIndex = pageIndex, PageSize = pageSize };
            }

            string orderByClause = search.SortOrder switch { 1 => """ORDER BY c."CharacterId" ASC""", _ => """ORDER BY c."DateSubmitted" DESC NULLS LAST, c."CharacterId" DESC""", };
            var pagingSql = $"""
    SELECT c."CharacterId", c."UserId", c."CharacterDisplayName", c."CardImageUrl", u."LastAction", u."ShowWhenOnline",
           ca."AvatarImageUrl", (SELECT B."CharacterNameClass" FROM "Badges" B JOIN "UserBadges" UB ON B."BadgeId" = UB."BadgeId" WHERE UB."UserId" = c."UserId" AND B."CharacterNameClass" IS NOT NULL ORDER BY B."SortOrder" LIMIT 1) AS "CharacterNameClass"
    {fromSql}
    {orderByClause}
    LIMIT @Take OFFSET @Skip
    """;
            parameters.Add("Skip", (pageIndex - 1) * pageSize);
            parameters.Add("Take", pageSize);

            var items = await GetRecordsAsync<CharactersForListing>(pagingSql, parameters);
            foreach (var item in items)
            {
                item.CardImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)item.CardImageUrl);
                item.AvatarImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)item.AvatarImageUrl);
            }

            return new PagedResult<CharactersForListing>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ImageCommentViewModel>> GetImageCommentsAsync(int imageId)
        {
            var comments = await GetRecordsAsync<ImageCommentViewModel>("""SELECT * FROM "ImageCommentsWithDetails" WHERE "ImageId" = @ImageId ORDER BY "CommentTimestamp" DESC""", new { ImageId = imageId });
            foreach (var comment in comments)
            {
                comment.CharacterImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)comment.CharacterImageUrl);
            }
            return comments;
        }
        public Task AddImageCommentAsync(int imageId, int characterId, string commentText) => ExecuteAsync("""INSERT INTO "CharacterImageComments" ("ImageId", "CharacterId", "CommentText") VALUES (@ImageId, @CharacterId, @CommentText)""", new { ImageId = imageId, CharacterId = characterId, CommentText = commentText });
        public Task DeleteImageCommentAsync(int commentId, int userId) => ExecuteAsync("""DELETE FROM "CharacterImageComments" AS CIC USING "CharacterImages" AS CI, "Characters" AS C_OWNER, "Characters" AS C_COMMENTER WHERE CIC."ImageId" = CI."CharacterImageId" AND CI."CharacterId" = C_OWNER."CharacterId" AND CIC."CharacterId" = C_COMMENTER."CharacterId" AND CIC."ImageCommentId" = @CommentId AND (@UserId = C_OWNER."UserId" OR @UserId = C_COMMENTER."UserId")""", new { CommentId = commentId, UserId = userId });

        public async Task<PagedResult<CharacterImage>> GetCharacterImagesAsync(int characterId, int pageIndex, int pageSize)
        {
            const string baseQuery = """FROM "CharacterImages" WHERE ("CharacterId" = @CharacterId)""";
            var countSql = "SELECT COUNT(\"CharacterImageId\") " + baseQuery;
            var pagingSql = $"SELECT * {baseQuery} ORDER BY \"CharacterImageId\" LIMIT @Take OFFSET @Skip";

            var totalCount = await GetScalarAsync<int>(countSql, new { CharacterId = characterId });
            if (totalCount == 0) return new PagedResult<CharacterImage> { PageIndex = pageIndex, PageSize = pageSize };

            var items = await GetRecordsAsync<CharacterImage>(pagingSql, new { CharacterId = characterId, Skip = (pageIndex - 1) * pageSize, Take = pageSize });

            return new PagedResult<CharacterImage>
            {
                Items = items.Select(img => ImageProcessingHelpers.ProcessCharacterImage(img, _urlProcessingService)).ToList(),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<CharacterImageWithDetails?> GetImageDetailsAsync(int imageId)
        {
            var image = await GetRecordAsync<CharacterImageWithDetails>("""SELECT CI."CharacterImageId", CI."CharacterId", C."UserId", C."CharacterDisplayName", CI."CharacterImageUrl", CI."ImageCaption", CI."IsMature" FROM "CharacterImages" CI JOIN "Characters" C ON CI."CharacterId" = C."CharacterId" WHERE CI."CharacterImageId" = @imageId""", new { imageId });
            if (image is not null)
            {
                image.CharacterImageUrl = _urlProcessingService.GetCharacterImageUrl((ImageUploadPath)image.CharacterImageUrl);
            }
            return image;
        }
        public Task<Character?> GetCharacterForEditAsync(int characterId, int userId) => GetRecordAsync<Character>("""SELECT * FROM "Characters" WHERE "CharacterId" = @characterId AND "UserId" = @userId""", new { characterId, userId });
        public Task UpdateCharacterCustomProfileAsync(int characterId, string? css, string? html, bool isEnabled) => ExecuteAsync("""UPDATE "Characters" SET "ProfileCss" = @CSS, "ProfileHtml" = @HTML, "CustomProfileEnabled" = @Enabled WHERE "CharacterId" = @CharID""", new { CSS = css, HTML = html, Enabled = isEnabled, CharID = characterId });
        public async Task<IEnumerable<CharacterImage>> GetCharacterImagesForGalleryAsync(int characterId)
        {
            var images = await GetRecordsAsync<CharacterImage>("""SELECT * FROM "CharacterImages" WHERE "CharacterId" = @characterId ORDER BY "CharacterImageId" """, new { characterId });
            return images.Select(img => ImageProcessingHelpers.ProcessCharacterImage(img, _urlProcessingService));
        }
        public Task UpsertCharacterAvatarAsync(int characterId, string avatarUrl)
        {
            const string sql = """
                                 INSERT INTO "CharacterAvatars" ("CharacterId", "AvatarImageUrl", "DateCreated")
                                 VALUES (@CharacterId, @AvatarUrl, NOW())
                                 ON CONFLICT ("CharacterId") DO UPDATE 
                                 SET "AvatarImageUrl" = EXCLUDED."AvatarImageUrl", "DateCreated" = NOW();
                                 """;
            return ExecuteAsync(sql, new { CharacterId = characterId, AvatarUrl = avatarUrl });
        }
        public Task<int> AddInlineImageAsync(string imageUrl, int characterId, int userId, string inlineName)
        {
            const string sql = """
                                 INSERT INTO "CharacterInlines" ("CharacterId", "UserId", "InlineImageUrl", "InlineName", "DateCreated")
                                 VALUES (@CharacterId, @UserId, @ImageUrl, @InlineName, NOW())
                                 RETURNING "InlineId"; 
                                 """;

            return GetScalarAsync<int>(sql, new
            {
                CharacterId = characterId,
                UserId = userId,
                ImageUrl = imageUrl,
                InlineName = inlineName
            });
        }
        public Task<CharacterInline?> GetInlineImageAsync(int inlineId)
        {
            const string sql = """SELECT * FROM "CharacterInlines" WHERE "InlineId" = @inlineId""";
            return GetRecordAsync<CharacterInline>(sql, new { inlineId });
        }

        public Task DeleteInlineImageRecordAsync(int inlineId)
        {
            const string sql = """DELETE FROM "CharacterInlines" WHERE "InlineId" = @inlineId""";
            return ExecuteAsync(sql, new { inlineId });
        }

        public Task<IEnumerable<CharacterInline>> GetInlineImagesAsync(int characterId)
        {
            const string sql = """SELECT * FROM "CharacterInlines" WHERE "CharacterId" = @characterId""";
            return GetRecordsAsync<CharacterInline>(sql, new { characterId });
        }

        public Task<CharacterAvatar?> GetCharacterAvatarAsync(int characterId)
        {
            const string sql = """SELECT * FROM "CharacterAvatars" WHERE "CharacterId" = @characterId""";
            return GetRecordAsync<CharacterAvatar>(sql, new { characterId });
        }

        public Task<IEnumerable<CharacterRecipient>> GetCharacterAndUserIdsByDisplayNamesAsync(List<string> displayNames)
        {
            const string sql = """
                SELECT "UserId", "CharacterId", "CharacterDisplayName"
                FROM "Characters"
                WHERE "CharacterDisplayName" = ANY(@DisplayNames)
                """;
            return GetRecordsAsync<CharacterRecipient>(sql, new { DisplayNames = displayNames });
        }

        public Task<int> GetUnapprovedCharacterCountAsync() => GetScalarAsync<int>("""SELECT COUNT(*) FROM "Characters" WHERE "IsApproved" = FALSE""");

        public Task<CharacterInline?> GetCharacterInlineAsync(int characterId, int inlineId) =>
            GetRecordAsync<CharacterInline>("""SELECT * FROM "CharacterInlines" WHERE "CharacterId" = @characterId AND "InlineId" = @inlineId""", new { characterId, inlineId });

        public Task<User?> GetImageOwnerAsync(int imageId)
        {
            const string sql = """
                SELECT U.* FROM "Users" U
                JOIN "Characters" C ON U."UserId" = C."UserId"
                JOIN "CharacterImages" CI ON C."CharacterId" = CI."CharacterId"
                WHERE CI."CharacterImageId" = @ImageId AND U."ReceivesImageCommentNotifications" = TRUE
                """;
            return GetRecordAsync<User>(sql, new { ImageId = imageId });
        }

        public Task SetCharacterStatusAsync(int characterId, int statusId) =>
            ExecuteAsync("""UPDATE "Characters" SET "CharacterStatusId" = @StatusId WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId, StatusId = statusId });

        public Task<ChatCharacterViewModel?> GetCharacterForChatAsync(int characterId)
        {
            const string sql = """
                SELECT
                    c."CharacterId", c."UserId", c."CharacterDisplayName",
                    'NormalCharacter' AS "CharacterNameClass",
                    ca."AvatarImageUrl"
                FROM "Characters" c
                LEFT JOIN "CharacterAvatars" ca ON c."CharacterId" = ca."CharacterId"
                WHERE c."CharacterId" = @CharacterId
                """;
            return GetRecordAsync<ChatCharacterViewModel>(sql, new { CharacterId = characterId });
        }

        public Task<int> GetTotalCharacterCountAsync()
        {
            return GetScalarAsync<int>("""SELECT COUNT(*) FROM "Characters" """);
        }

        public Task IncrementCharacterViewCountAsync(int characterId)
        {
            return ExecuteAsync("""UPDATE "Characters" SET "ViewCount" = "ViewCount" + 1 WHERE "CharacterId" = @CharacterId""", new { CharacterId = characterId });
        }

        public async Task DeleteImagesAsync(List<int> imageIds, int userId)
        {
            var images = await GetRecordsAsync<CharacterImage>("""SELECT * FROM "CharacterImages" WHERE "CharacterImageId" = ANY(@ImageIds) AND "UserId" = @UserId""", new { ImageIds = imageIds, UserId = userId });
            foreach (var image in images)
            {
                await _imageService.DeleteImageAsync((ImageUploadPath)image.CharacterImageUrl);
            }
            await ExecuteAsync("""DELETE FROM "CharacterImages" WHERE "CharacterImageId" = ANY(@ImageIds) AND "UserId" = @UserId""", new { ImageIds = imageIds, UserId = userId });
        }
    }
    
    internal static class ImageProcessingHelpers
    {
        internal static void ProcessCharacterDetails(CharacterWithDetails? character, IUrlProcessingService urlProcessingService)
        {
            if (character is null) return;
            character.AvatarImageUrl = urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.AvatarImageUrl);
            character.CardImageUrl = urlProcessingService.GetCharacterImageUrl((ImageUploadPath)character.CardImageUrl);
        }

        internal static CharacterImage ProcessCharacterImage(CharacterImage image, IUrlProcessingService urlProcessingService)
        {
            image.CharacterImageUrl = urlProcessingService.GetCharacterImageUrl((ImageUploadPath)image.CharacterImageUrl);
            return image;
        }
    }
}
