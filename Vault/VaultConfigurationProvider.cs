using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.Commons;

namespace API.Vault
{
    public class VaultConfigurationProvider : ConfigurationProvider
    {
        private readonly VaultOptions _config;
        private readonly IVaultClient _client;
        private readonly Action<IDictionary<string, string>, VaultResponse> _updateConfiguration;

        public VaultConfigurationProvider(VaultOptions config, Action<IDictionary<string, string>, VaultResponse> myUpdateConfiguration)
        {
            _config = config;
            _updateConfiguration = myUpdateConfiguration;

            Console.WriteLine("Service Timeout set to " + _config.Timeout + " minute(s)");

            var vaultClientSettings = new VaultClientSettings(_config.Address, new AppRoleAuthMethodInfo(_config.Role, _config.Secret))
            {
                VaultServiceTimeout = new TimeSpan(0, Convert.ToInt32(_config.Timeout), 0)
            };

            _client = new VaultClient(vaultClientSettings);
        }

        public override void Load()
        {
            LoadAsync(_updateConfiguration).Wait();
        }

        public async Task LoadAsync(Action<IDictionary<string, string>, VaultResponse> updateConfiguration)
        {
            await GetFromVault(updateConfiguration);
        }

        public async Task GetFromVault(Action<IDictionary<string, string>, VaultResponse> myUpdateAction)
        {
            VaultResponse vaultValues;

            Console.WriteLine("MountPath: " + _config.MountPath);

            if (_config.SecretType == "secrets" && _config.Version == "1")
            {
                Console.WriteLine("Config Version: " + _config.Version);

                // NOTE: mountPoint is set to "DIGI" because that is the name of your secrets engine
                Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(_config.MountPath, mountPoint: "DIGI");
                Console.WriteLine("secrets: " + secrets.Data.Data.Count.ToString());

                var normalizedDict = new Dictionary<string, string>();
                foreach (var item in secrets.Data.Data)
                {
                    normalizedDict[item.Key] = item.Value?.ToString();
                    if (item.Key.Contains(":"))
                    {
                        Data[item.Key] = item.Value?.ToString();
                    }
                }

                vaultValues = JsonConvert.DeserializeObject<VaultResponse>(
                    JsonConvert.SerializeObject(normalizedDict)
                );
            }
            else
            {
                Console.WriteLine("Config Version: " + _config.Version);
                Secret<Dictionary<string, object>> secrets = await _client.V1.Secrets.KeyValue.V1.ReadSecretAsync(_config.MountPath);
                Console.WriteLine("secrets: " + secrets.Data.Count.ToString());

                var normalizedDict = new Dictionary<string, string>();
                foreach (var item in secrets.Data)
                {
                    normalizedDict[item.Key] = item.Value?.ToString();
                    if (item.Key.Contains(":"))
                    {
                        Data[item.Key] = item.Value?.ToString();
                    }
                }

                vaultValues = JsonConvert.DeserializeObject<VaultResponse>(
                    JsonConvert.SerializeObject(normalizedDict)
                );
            }

            myUpdateAction(this.Data, vaultValues);
        }
    }

    public class VaultConfigurationSource : IConfigurationSource
    {
        private readonly VaultOptions _config;
        private readonly Action<IDictionary<string, string>, VaultResponse> _updateConfig;

        public VaultConfigurationSource(Action<VaultOptions> config, Action<IDictionary<string, string>, VaultResponse> updateConfig)
        {
            _updateConfig = updateConfig;
            _config = new VaultOptions();
            config.Invoke(_config);
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new VaultConfigurationProvider(_config, _updateConfig);
        }
    }
}