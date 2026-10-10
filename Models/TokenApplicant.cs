namespace TokenApp.Models
{
    /// <summary>The person a token was issued to.</summary>
    public class TokenApplicant
    {
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        /// <summary>"mobile • city", skipping whichever part is missing.</summary>
        public string Details => string.Join(" • ", new[] { Mobile, City }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
