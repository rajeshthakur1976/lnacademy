namespace TokenManager.Model
{
    public class ServicePoolToken
    {
        public string Token { get; set; }
        public string LastTokenAccessTime { get; set; }
        public int InProgressCall { get; set; }
        public string TokenID { get; set; }
    }
}
