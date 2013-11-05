using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class TwoFactorAuthInputModel
    {
        [Required]
        public string Code { get; set; }

        public string FirstPinPosition { get; set; }

        public string FirstPinCharacter { get; set; }
        
        public string SecondPinPosition { get; set; }

        public string SecondPinCharacter { get; set; }

        public string ReturnUrl { get; set; }
    }
}