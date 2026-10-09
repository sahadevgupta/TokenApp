using System.Text.Json.Serialization;

namespace TokenApp.Models.Response
{
    /// <summary>
    /// Token as returned by the backend. Which fields are populated depends on the endpoint
    /// (generate/apply: id, token_type, status; print: token_number, token_type, token_date).
    /// </summary>
    public class TokenApiDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("token_number")]
        public int? TokenNumber { get; set; }

        [JsonPropertyName("token_date")]
        public string? TokenDate { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    /// <summary>{ success, message, data: token } — generate-print-token and token/apply.</summary>
    public class TokenDataResponseDto : ApiEnvelopeDto
    {
        [JsonPropertyName("data")]
        public TokenApiDto? Data { get; set; }
    }

    /// <summary>{ success, token } — token/print/{id}.</summary>
    public class PrintTokenResponseDto : ApiEnvelopeDto
    {
        [JsonPropertyName("token")]
        public TokenApiDto? Token { get; set; }
    }
}
