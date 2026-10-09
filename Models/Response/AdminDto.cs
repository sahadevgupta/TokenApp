using System.Text.Json.Serialization;

namespace TokenApp.Models.Response
{
    public class AdminDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }
}
