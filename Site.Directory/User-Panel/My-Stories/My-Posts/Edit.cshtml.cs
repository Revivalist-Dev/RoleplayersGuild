using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;

namespace RoleplayersGuild.Site.Directory.User_Panel.My_Stories
{
    public class EditMyPostModel : UserPanelBaseModel
    {
        // NOTE: Redundant fields were removed. 'DataService' and 'UserService' from the base class will be used.

        // UPDATED: Constructor to match the new base class signature.
        public EditMyPostModel(IDataService dataService, IUserService userService)
            : base(dataService, userService)
        {
        }

        [BindProperty]
        public PostInputModel Input { get; set; } = new();
        public SelectList UserCharacters { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            var post = await DataService.GetStoryPostForEditAsync(id, userId);
            if (post is null) return NotFound();

            Input = new PostInputModel
            {
                StoryPostId = post.StoryPostId,
                StoryId = post.StoryId,
                CharacterId = post.CharacterId,
                PostContent = post.PostContent ?? ""
            };

            await PopulateCharactersAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = UserService.GetUserId(User);
            if (userId == 0) return Forbid();

            await PopulateCharactersAsync(userId);
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userOwnsCharacter = UserCharacters.Any(c => c.Value == Input.CharacterId.ToString());
            if (!userOwnsCharacter)
            {
                return Forbid();
            }

            await DataService.UpdateStoryPostAsync(Input.StoryPostId, Input.CharacterId, Input.PostContent);
            return RedirectToPage("/Community/Stories/Posts", new { id = Input.StoryId });
        }

        private async Task PopulateCharactersAsync(int userId)
        {
            var characters = await DataService.GetActiveCharactersForUserAsync(userId);
            UserCharacters = new SelectList(characters, "CharacterId", "CharacterDisplayName", Input.CharacterId);
        }
    }
}