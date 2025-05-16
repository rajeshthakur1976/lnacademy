using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using TokenManager.Cache;
using TokenManager.Enum;
using TokenManager.Model;

namespace TokenManager.Service
{
    public class TokenManagerService : ITokenManagerService
    {
        private readonly ILogger<ITokenManagerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICacheProvider _cacheProvider;
        private readonly IAuthService _authService;
        public TokenManagerService(ILogger<ITokenManagerService> logger, IConfiguration configuration, ICacheProvider cacheProvider, IAuthService authService)
        {
            _logger = logger;
            _configuration = configuration;
            _cacheProvider = cacheProvider;
            _authService = authService;
        }

        #region Public Methods
        /// <summary>
        /// Generate FNO Token
        /// </summary>
        /// <returns></returns>
        public async Task AddORUpdateFNOToken()
        {
            try
            {
                List<ServiceConfig> serviceCol = new();
                var serviceConfigJson = _configuration["FNOServiceConfig"];
                _logger.LogInformation($"Service Config Json: {serviceConfigJson}");

                serviceCol = JsonConvert.DeserializeObject<List<ServiceConfig>>(serviceConfigJson);
                serviceCol.ForEach(async item =>
                {
                    for (int i = 1; i <= item.Count; i++)
                    {
                        string ClientID = item.Prefix + _configuration["constClientID"] + i;
                        _logger.LogInformation($"ClientID is : {ClientID}");
                        string ClientSec = item.Prefix + _configuration["constClientSec"] + i;
                        _logger.LogInformation($"ClientID Sec : {ClientSec}");
                        _logger.LogInformation($"Service Name is  : {item.ServiceName}");
                        TokenDetails tokenDetails = new() { ClientID = _configuration[ClientID], ClientSec = _configuration[ClientSec], TokenID = _configuration["TokenIDPrefix"] + i, Instance = _configuration["FNOInstance"], Tenant = _configuration["FNOTenant"], BaseUrl = _configuration["FNOBaseUrl"], ServicePoolType = item.ServiceName, TokenType = TokenType.FNOToken };
                        await GenerateTokenAndPushToRedish(tokenDetails);
                    }


                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in AddORUpdateToken  : {ex.Message}");
            }


        }

        public async Task AddORUpdateCEToken()
        {
            try
            {
                List<ServiceConfig> serviceCol = new();
                var serviceConfigJson = _configuration["CEServiceConfig"];
                _logger.LogInformation($"Service Config Json: {serviceConfigJson}");

                serviceCol = JsonConvert.DeserializeObject<List<ServiceConfig>>(serviceConfigJson);
                serviceCol.ForEach(async item =>
                {
                    for (int i = 1; i <= item.Count; i++)
                    {
                        string ClientID = item.Prefix + _configuration["constClientID"] + i;
                        _logger.LogInformation($"ClientID is : {ClientID}");
                        string ClientSec = item.Prefix + _configuration["constClientSec"] + i;
                        _logger.LogInformation($"ClientID Sec : {ClientSec}");
                        _logger.LogInformation($"Service Name is  : {item.ServiceName}");
                        TokenDetails tokenDetails = new() { ClientID = _configuration[ClientID], ClientSec = _configuration[ClientSec], TokenID = _configuration["TokenIDPrefix"] + i, Instance = _configuration["CEInstance"], Tenant = _configuration["CETenant"], BaseUrl = _configuration["CEBaseUrl"], ServicePoolType = item.ServiceName, TokenType = TokenType.CEToken };
                        await GenerateTokenAndPushToRedish(tokenDetails);
                    }


                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in AddORUpdateToken  : {ex.Message}");
            }


        }

        ///// <summary>
        ///// Generate FNO Token
        ///// </summary>
        ///// <returns></returns>
        //public async Task AddORUpdateCEToken()
        //{
        //    try
        //    {
        //        List<ServiceConfig> serviceCol = new();
        //        var serviceConfigJson = _configuration["CEServiceConfig"];
        //        _logger.LogInformation($"CE Service Config Json: {serviceConfigJson}");

        //        serviceCol = JsonConvert.DeserializeObject<List<ServiceConfig>>(serviceConfigJson);
        //        serviceCol.ForEach(async item =>
        //        {
        //            for (int i = 1; i <= item.Count; i++)
        //            {
        //                string ClientID = item.Prefix + _configuration["constClientID"] + i;
        //                _logger.LogInformation($"CE ClientID is : {ClientID}");
        //                string ClientSec = item.Prefix + _configuration["constClientSec"] + i;
        //                _logger.LogInformation($"CE ClientID Sec : {ClientSec}");
        //                _logger.LogInformation($" CE Service Name is  : {item.ServiceName}");
        //                TokenDetails tokenDetails = new() { ServicePoolType = ServicePoolType.CEOrderServicePool, ClientID = _configuration[ClientID], ClientSec = _configuration[ClientSec], TokenID = _configuration["TokenIDPrefix"] + i };
        //                await GenerateCETokenAndPushToRedish(item.ServiceName, tokenDetails);
        //            }


        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error occured in CE  AddORUpdateToken  : {ex.Message}");
        //    }


        //}

        #endregion
        #region Private Methods
        /// <summary>
        /// Generate FNO/CE Token and Add/Update in Redis
        /// </summary>
        /// <param name="servicePoolType"></param>
        /// <param name="tokenDetails"></param>
        /// <returns></returns>
        private async Task GenerateTokenAndPushToRedish(TokenDetails tokenDetails)
        {

            try
            {
                bool isTokenExists = await _cacheProvider.IsHashExists(tokenDetails.ServicePoolType, new RedisValue(tokenDetails.TokenID));

                if (isTokenExists)
                {
                    var val = await _cacheProvider.GetHashValue(tokenDetails.ServicePoolType, new RedisValue(tokenDetails.TokenID));
                    var servicepoolToken = JsonConvert.DeserializeObject<ServicePoolToken>(val);
                    _logger.LogInformation($"servicepoolToken  : {servicepoolToken}");
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(servicepoolToken.Token.Replace("Bearer ", "")) as JwtSecurityToken;
                    var expDate = jsonToken?.ValidTo;

                    if (expDate < DateTime.UtcNow.AddMinutes(Convert.ToInt16(_configuration["Interval"])))
                    {
                        string tokenExpiryDetailsMessage = "Token Expired for service " + tokenDetails.ServicePoolType + "Token ID was :" + tokenDetails.TokenID;
                        _logger.LogInformation($"Token Expiration Details  : {tokenExpiryDetailsMessage}");
                        _logger.LogInformation($"Instance is  : {tokenDetails.Instance}");
                        _logger.LogInformation($"Tenant is  : {tokenDetails.Tenant}");
                        _logger.LogInformation($"Client ID is  : {tokenDetails.ClientID}");
                        _logger.LogInformation($"Client Sec is  : {tokenDetails.ClientSec}");
                        _logger.LogInformation($"Base Url is  : {tokenDetails.BaseUrl}");
                        string token = await _authService.GenerateToken(tokenDetails.Instance, tokenDetails.Tenant, tokenDetails.ClientID, tokenDetails.ClientSec, tokenDetails.BaseUrl);
                        _logger.LogInformation($"New Generated Token for service {tokenDetails.ServicePoolType} Angainst Token ID {tokenDetails.TokenID}");
                        ServicePoolToken servicePoolToken = new() { Token = token, InProgressCall = 0, LastTokenAccessTime = "", TokenID = tokenDetails.TokenID };
                        await _cacheProvider.SettHashValue(tokenDetails.ServicePoolType, new HashEntry[] { new HashEntry(tokenDetails.TokenID, JsonConvert.SerializeObject(servicePoolToken)) });
                        _logger.LogInformation($"New Generated Token for service {tokenDetails.ServicePoolType} Angainst Token ID {tokenDetails.TokenID} is stored in Redis");
                    }

                }
                else
                {
                    _logger.LogInformation($"Token Generation Started for Non Existing Token...");
                    _logger.LogInformation($"Instance is  : {tokenDetails.Instance}");
                    _logger.LogInformation($"Tenant is  : {tokenDetails.Tenant}");
                    _logger.LogInformation($"Client ID is  : {tokenDetails.ClientID}");
                    _logger.LogInformation($"Client Sec is  : {tokenDetails.ClientSec}");
                    _logger.LogInformation($"Base Url is  : {tokenDetails.BaseUrl}");
                    string token = await _authService.GenerateToken(tokenDetails.Instance, tokenDetails.Tenant, tokenDetails.ClientID, tokenDetails.ClientSec, tokenDetails.BaseUrl);
                    _logger.LogInformation($"New token Generated for {tokenDetails.ServicePoolType}  angainst token ID {tokenDetails.TokenID} and Token is : {token}");
                    ServicePoolToken servicePoolToken = new() { Token = token, InProgressCall = 0, LastTokenAccessTime = "", TokenID = tokenDetails.TokenID };
                    await _cacheProvider.SettHashValue(tokenDetails.ServicePoolType, new HashEntry[] { new HashEntry(tokenDetails.TokenID, JsonConvert.SerializeObject(servicePoolToken)) });

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in Generating Token  : {ex.Message}");
            }
        }
        #endregion

    }
}
