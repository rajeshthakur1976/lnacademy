using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenManager
{
    public static class ConfigurationKeys
    {
        /// <summary>
        /// StorageConnectionString for Azure Function
        /// </summary>
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        public const string DatabaseUserId = "DbCredentials:UserId";
        public const string KeyVaultName = "anshukinshu";
        public const string KeyVaultTenantId = "8bf0715b-c829-4323-b194-b87a2b754582";
        public const string StorageConnectionString = "StorageCredentials:ConnectionString";
    }
}
