using System.ComponentModel.DataAnnotations;

namespace AzureWebAPI.Models
{
    public class RefreshTokenRequest
    {

        [Required]
        public string RefreshToken { get; set; }
    }
}
