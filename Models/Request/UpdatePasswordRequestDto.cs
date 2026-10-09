using System.Text.Json.Serialization;

namespace TokenApp.Models.Request
{
    public class UpdatePasswordRequestDto
    {
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
