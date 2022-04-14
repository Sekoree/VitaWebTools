using System.Text.Json.Serialization;

namespace VitaWebTools.Entities.PSN
{
    public class UserInfoResponse
    {
        [JsonPropertyName("scopes")]
        public string? Scopes { get; init; }
        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; init; }
        [JsonPropertyName("client_id")]
        public string? ClientId { get; init; }
        [JsonPropertyName("dcim_id")]
        public string? DcimId { get; init; }
        [JsonPropertyName("grant_type")]
        public string? GrantType { get; init; }
        [JsonPropertyName("user_id")]
        public string? UserId { get; init; }
        [JsonPropertyName("user_uuid")]
        public string? UserUUID { get; init; }
        [JsonPropertyName("online_id")]
        public string? OnlineId { get; init; }
        [JsonPropertyName("country_code")]
        public string? CountryCode { get; init; }
        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; init; }
        [JsonPropertyName("community_domain")]
        public string? CommunityDomain { get; init; }
        [JsonPropertyName("is_sub_account")]
        public bool? IsSubAccount { get; init; }
    }
}
