using System;

namespace MyLeasing.Common.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string RolId { get; set; }
        public bool IsSuccess { get; set; }

        public DateTime Expiration { get; set; }

        public DateTime ExpirationLocal => Expiration.ToLocalTime();
    }
}

