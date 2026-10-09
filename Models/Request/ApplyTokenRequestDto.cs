using System.Text.Json.Serialization;

namespace TokenApp.Models.Request
{
    public class ApplyTokenRequestDto
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
