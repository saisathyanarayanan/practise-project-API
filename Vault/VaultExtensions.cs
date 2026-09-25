using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace API.Vault
{
    public static class VaultExtensions
    {
        public static IConfigurationBuilder AddVault(this IConfigurationBuilder configuration, Action<VaultOptions> options, Action<IDictionary<string, string>, VaultResponse> updateConfig)
        {
            var vaultOptions = new VaultConfigurationSource(options, updateConfig);
            configuration.Add(vaultOptions);
            return configuration;
        }

        public static void AddVaultAppConfiguration(this IConfigurationBuilder config)
        {
            IConfiguration globalConfig = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "VAULT_")
                .Build();

            if (globalConfig.GetSection("ADDRESS").Exists() && globalConfig.GetSection("SECRET_ID").Exists())
            {
                config.AddVault(options =>
                {
                    options.Address = globalConfig["ADDRESS"];
                    options.MountPath = globalConfig["MOUNT_PATH"];
                    options.SecretType = globalConfig["SECRET_TYPE"];
                    options.Role = globalConfig["ROLE_ID"];
                    options.Secret = globalConfig["SECRET_ID"];
                    options.Timeout = globalConfig["VAULT_TIMEOUT"] ?? "1";
                    options.Version = globalConfig["VERSION"] ?? "1";
                },
                (data, vaultValues) =>
                {
                    data["ConnectionStrings:DefaultConnection"] = vaultValues.DBCS;
                    data["JwtConfig:secret"] = vaultValues.secret;
                    data["JwtConfig:expirationInMinutes"] = vaultValues.expirationInMinutes;
                    data["JwtConfig:client_id"] = vaultValues.client_id;
                    data["JwtConfig:Audience"] = vaultValues.Audience;
                    data["JwtConfig:Issuer"] = vaultValues.Issuer;
                });
            }

            if (File.Exists("appsettings.json"))
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            }
        }
    }
}