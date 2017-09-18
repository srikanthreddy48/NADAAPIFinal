using NADAAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.ViewModels.AccountViewModels
{
    public class AccountViewModel
    {
        public string MemberId { get; set; }
    
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

       
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


       
        [Display(Name = "EndPoint")]
        public List<EndPoint> EndPoints { get; set; }

        public List<string> UserEndPoints { get; set; }

        [Display(Name = "ApplicationUser")]
        public List<ApplicationUser> ApplicationUsers { get; set; }


        [Display(Name = "VendorName")]
        public string VendorName { get; set; }


        [Display(Name= "VendorId")]
        public Guid VendorId { get; set; }
    }
}
