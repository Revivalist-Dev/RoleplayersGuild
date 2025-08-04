using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoleplayersGuild.Site.Model
{
    // ----------------------------------------------------------------
    // Community Feature Input Models (Chat, Proposals)
    // ----------------------------------------------------------------

    public class ChatRoomInputModel
    {
        public int ChatRoomId { get; set; }
        [Required]
        [StringLength(50)]
        public string? ChatRoomName { get; set; }
        public string? ChatRoomDescription { get; set; }
        [Required]
        public int? ContentRatingId { get; set; }
        public int? UniverseId { get; set; }

        public ChatRoomInputModel() { }
        public ChatRoomInputModel(ChatRoomWithDetails chatRoom)
        {
            ChatRoomId = chatRoom.ChatRoomId;
            ChatRoomName = chatRoom.ChatRoomName;
            ChatRoomDescription = chatRoom.ChatRoomDescription;
            ContentRatingId = chatRoom.ContentRatingId;
            UniverseId = chatRoom.UniverseId;
        }
    }

    public class ProposalInputModel
    {
        public int ProposalId { get; set; }
        [Required] public string? Title { get; set; }
        [Required] public string? Description { get; set; }
        [Required] public int? ContentRatingId { get; set; }
        [Required] public int? StatusId { get; set; }
        public bool IsPrivate { get; set; }
        public bool DisableLinkify { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();

        public ProposalInputModel() { }
        public ProposalInputModel(Proposal p)
        {
            ProposalId = p.ProposalId;
            Title = p.Title;
            Description = p.Description;
            ContentRatingId = p.ContentRatingId;
            StatusId = p.StatusId;
            IsPrivate = p.IsPrivate;
            DisableLinkify = p.DisableLinkify;
        }
    }
}