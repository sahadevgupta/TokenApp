using System.Text.Json.Serialization;

namespace TokenApp.Models.Response
{
    /// <summary>
    /// Common { "success", "message" } envelope returned by every documented endpoint.
    /// </summary>
    public class ApiEnvelopeDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Error body (401/403/422). Laravel adds a field-level "errors" map on validation failures.
    /// </summary>
    public class ApiErrorDto
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, List<string>>? Errors { get; set; }
    }
}
