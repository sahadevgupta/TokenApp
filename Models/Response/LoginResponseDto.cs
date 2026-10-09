using System.Text.Json.Serialization;

namespace TokenApp.Models.Response
{
    public class LoginResponseDto : ApiEnvelopeDto
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("admin")]
        public AdminDto? Admin { get; set; }
    }
}
