using System.Text.Json.Serialization;

namespace TokenApp.Models.Request
{
    public class GenerateTokenRequestDto
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("token_number")]
        public int TokenNumber { get; set; }

        /// <summary>yyyy-MM-dd</summary>
        [JsonPropertyName("token_date")]
        public string TokenDate { get; set; } = string.Empty;
    }
}
