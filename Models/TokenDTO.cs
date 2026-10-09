using System;

namespace TokenApp.Models
{
    /// <summary>
    /// Data Transfer Object for Token - optimized for UI display
    /// Contains only the data needed for presentation layer
    /// </summary>
    public class TokenDTO
    {
        public int Id { get; set; }
        public int TokenNumber { get; set; }
        public TokenType Type { get; set; }
        public DateTime DateTime { get; set; }
        public TokenStatus Status { get; set; }
        public string? Location { get; set; }
        public string? BatchNumber { get; set; }
        public string? Escort { get; set; }
        public TokenActions AvailableActions { get; set; }

        /// <summary>Id assigned by the backend; null until the token has been synced.</summary>
        public int? RemoteId { get; set; }

        /// <summary>False while the token is queued locally waiting to be sent to the backend.</summary>
        public bool IsSynced { get; set; }

        /// <summary>Set when the backend permanently rejected the token (e.g. 422); it will not be retried.</summary>
        public string? SyncError { get; set; }

        /// <summary>
        /// Computed property: format DateTime as "dd-MM-yyyy • hh:mm AM/PM"
        /// </summary>
        public string FormattedDateTime => DateTime.ToString("dd-MM-yyyy • hh:mm tt");

        /// <summary>
        /// Computed property: get the first relevant info (location, batch, or escort)
        /// </summary>
        public string? PrimaryInfo => Location ?? BatchNumber ?? Escort;

        /// <summary>
        /// Check if this action is available
        /// </summary>
        public bool HasAction(TokenActions action) => (AvailableActions & action) == action;
    }
}
