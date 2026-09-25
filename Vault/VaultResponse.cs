namespace API.Vault
{
    public partial class VaultResponse
    {
        public string DBCS { get; set; }
        public string secret { get; set; }
        public string expirationInMinutes { get; set; }
        public string client_id { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string CASHost { get; set; }
        public string BasePath { get; set; }
        public string ServiceMailID { get; set; }
        public string ServiceMailPassword { get; set; }
        public string ServiceMailUserName { get; set; }
        public string? MyInfoApi_BaseUrl { get; set; }
        public string? MyInfoApi_GetTokenUrl { get; set; }
        public string? MyInfoApi_GetSGRIPRForJCCUrl { get; set; }
        public string? MyInfoApi_GetPRNumberDetailsUrl { get; set; }
        public string? MyInfoApi_SystemId { get; set; }
        public string? MyInfoApi_Username { get; set; }
        public string? MyInfoApi_GrantType { get; set; }
        public string? MyInfoApi_Password { get; set; }
        public string? SGConnectClientID { get; set; }
        public string? SGConnectClientSecret { get; set; }
        public string? SGConnectTokenURL { get; set; }
        public string? SGConnectUserInfoURL { get; set; }
        public string? SGConnectRedirectURL { get; set; }
    }
}