using System.Text.Json.Serialization;

namespace TokenApp.Models.Request
{
    public class RejectTokenRequestDto
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
