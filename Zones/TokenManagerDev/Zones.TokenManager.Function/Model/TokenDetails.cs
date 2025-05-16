using TokenManager.Enum;

namespace TokenManager.Model
{
    public class TokenDetails
    {
        public TokenType TokenType { get; set; }
        public string ClientID { get; set; }
        public string ClientSec { get; set; }
        public string TokenID { get; set; }
        public string BreareToken { get; set; }
        public string Instance { get; set; }
        public string Tenant { get; set; }
        public string BaseUrl { get; set; }
        public string ServicePoolType { get; set; }
    }
}
