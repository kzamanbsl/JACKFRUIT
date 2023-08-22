using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BSA.Models
{
    public class UserLogin
    {
        [Display(Name = "Member No")]
        [Required(AllowEmptyStrings =false, ErrorMessage = "Member No is required")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings =false, ErrorMessage ="Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="Remember Me")]
        public bool RememberMe { get; set; }
    }
}