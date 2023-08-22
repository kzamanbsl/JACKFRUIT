using System.ComponentModel.DataAnnotations;

namespace BSA.Models
{
    public class PasswordChangeModel
    {
        [Display(Name = "Old Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Old Password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [Display(Name = "Confirm Password")]        
        public string ConfirmPassword { get; set; }
    }
}