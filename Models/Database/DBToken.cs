using SQLite;
using System;

namespace TokenApp.Models.Database
{
    [Table("tokens")]
    public class DBToken
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("token_number")]
        public int TokenNumber { get; set; }

        [Column("type")]
        public int Type { get; set; } // Stored as int for enum

        [Column("date_time")]
        public long DateTime { get; set; } // Stored as ticks

        [Column("status")]
        public int Status { get; set; } // Stored as int for enum

        [Column("location")]
        public string? Location { get; set; }

        [Column("batch_number")]
        public string? BatchNumber { get; set; }

        [Column("escort")]
        public string? Escort { get; set; }

        [Column("available_actions")]
        public int AvailableActions { get; set; } // Stored as int for flags

        [Column("created_at")]
        public long CreatedAt { get; set; }

        [Column("updated_at")]
        public long UpdatedAt { get; set; }

        [Column("is_synced")]
        public bool IsSynced { get; set; } = false;

        [Column("sync_timestamp")]
        public long? SyncTimestamp { get; set; }

        /// <summary>Id assigned by the backend once the token has been synced.</summary>
        [Column("remote_id")]
        public int? RemoteId { get; set; }

        /// <summary>Permanent rejection reason from the backend; rows with an error are not retried.</summary>
        [Column("sync_error")]
        public string? SyncError { get; set; }

        // Helper properties for mapping
        [Ignore]
        public TokenType TypeEnum
        {
            get => (TokenType)Type;
            set => Type = (int)value;
        }

        [Ignore]
        public TokenStatus StatusEnum
        {
            get => (TokenStatus)Status;
            set => Status = (int)value;
        }

        [Ignore]
        public TokenActions ActionsEnum
        {
            get => (TokenActions)AvailableActions;
            set => AvailableActions = (int)value;
        }

        [Ignore]
        public System.DateTime CreatedAtDateTime
        {
            get => new System.DateTime(CreatedAt, System.DateTimeKind.Utc);
            set => CreatedAt = value.Ticks;
        }

        [Ignore]
        public System.DateTime UpdatedAtDateTime
        {
            get => new System.DateTime(UpdatedAt, System.DateTimeKind.Utc);
            set => UpdatedAt = value.Ticks;
        }

        [Ignore]
        public System.DateTime DateTimeProperty
        {
            get => new System.DateTime(DateTime, System.DateTimeKind.Utc);
            set => DateTime = value.Ticks;
        }

        [Ignore]
        public System.DateTime? SyncTimestampDateTime
        {
            get => SyncTimestamp.HasValue ? new System.DateTime(SyncTimestamp.Value, System.DateTimeKind.Utc) : null;
            set => SyncTimestamp = value?.Ticks;
        }
    }
}
