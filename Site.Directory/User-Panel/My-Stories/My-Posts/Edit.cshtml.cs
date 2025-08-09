using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Stories.My_Posts
{
    public class EditMyPostModel : UserPanelBaseModel
    {
        private readonly IContentDataService _contentDataService;

        public EditMyPostModel(
            ICharacterDataService characterDataService,
            ICommunityDataService communityDataService,
            IMiscDataService miscDataService,
            IUserService userService,
            IContentDataService contentDataService)
            : base(characterDataService, communityDataService, miscDataService, userService)
        {
            _contentDataService = contentDataService;
        }

        [BindProperty]
        public PostInputModel Input { get; set; } = new();
        public SelectList UserCharacters { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var post = await _contentDataService.GetStoryPostForEditAsync(id, LoggedInUserId);
            if (post is null) return NotFound();

            Input = new PostInputModel
            {
                StoryPostId = post.StoryPostId,
                StoryId = post.StoryId,
                CharacterId = post.CharacterId,
                PostContent = post.PostContent ?? ""
            };

            await PopulateCharactersAsync(LoggedInUserId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await PopulateCharactersAsync(LoggedInUserId);
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userOwnsCharacter = UserCharacters.Any(c => c.Value == Input.CharacterId.ToString());
            if (!userOwnsCharacter)
            {
                return Forbid();
            }

            await _contentDataService.UpdateStoryPostAsync(Input.StoryPostId, Input.CharacterId, Input.PostContent);
            return RedirectToPage("/Community/Stories/Posts", new { id = Input.StoryId });
        }

        private async Task PopulateCharactersAsync(int userId)
        {
            var characters = await _characterDataService.GetActiveCharactersForUserAsync(userId);
            UserCharacters = new SelectList(characters, "CharacterId", "CharacterDisplayName", Input.CharacterId);
        }
    }
}
