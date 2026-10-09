namespace TokenApp.Constants
{
    public static class ApiConstants
    {
        public const string BaseUrl = "https://mandirtoken.cloud99.in/api";

        public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

        // token_type values defined by the API documentation
        public const string TokenTypeRed = "red";
        public const string TokenTypeYellow = "yellow";
        public const string TokenTypeEmergency = "emergency";

        // status values defined by the API documentation
        public const string StatusPending = "pending";
        public const string StatusApproved = "approved";

        public const string TokenDateFormat = "yyyy-MM-dd";
    }
}
