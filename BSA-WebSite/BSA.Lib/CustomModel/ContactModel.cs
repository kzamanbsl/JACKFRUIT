using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bsa.lib.CustomModel
{
    public class ContactModel
    {
        public int Id{ get; set; }

        [Display(Name = "Name/Company Name")]
        public string CompanyName { get; set; }
        public string Name{ get; set; }
        [Required(ErrorMessage = "Field can't be empty")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
        public string Email{ get; set; }
        public string Subject{ get; set; }
        public string Massage{ get; set; }
        public string Mobile { get; set; }
        public string FilePath { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Upload Your CV")]
        [Required(ErrorMessage = "Please choose file to upload.")]
        public string File { get; set; }
    }
}
