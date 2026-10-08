namespace API.Vault
{
    public class VaultOptions
    {
        public string Address { get; set; }
        public string Role { get; set; }
        public string Secret { get; set; }
        public string MountPath { get; set; }
        public string SecretType { get; set; }
        public string Timeout { get; set; }
        public string Version { get; set; }
    }
}