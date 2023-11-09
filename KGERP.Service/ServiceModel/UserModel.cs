using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace KGERP.Service.ServiceModel
{
    public class UserModel
    {
        public int UserId { get; set; }
        // public string UserName { get; set; }
        // public string Email { get; set; }
        // public string Password { get; set; }

        public string EmployeeName { get; set; }
        public string MobileNo { get; set; }

        public bool IsEmailVerified { get; set; }
        public Nullable<System.Guid> ActivationCode { get; set; }


        [Display(Name = "User Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "User name required")]
        [RegularExpression(@"^[^\s\,]*$", ErrorMessage = "User Name Can't Have Spaces/ Comma")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Minimum 4 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Confirm password and password do not match")]

        public string ConfirmPassword { get; set; }

        [Display(Name = "Old Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        public bool Active { get; set; }
        public int UserTypeId { get; set; }
        public string UserTypeName { get { return BaseFunctionalities.GetEnumDescription((EnumUserType)UserTypeId); } }

        public int? DealerId { get; set; }
        public int? DeportId { get; set; }

        public IEnumerable<UserModel> DataList { get; set; }

        public SelectList EnumUserTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<EnumUserTypeDD>(), "Value", "Text"); } }
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());
    }
}
