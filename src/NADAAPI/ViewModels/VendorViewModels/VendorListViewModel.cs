using NADAAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.ViewModels.VendorViewModels
{
    public class VendorListViewModel
    {
        public IEnumerable<Vendor> Vendors { get; set; }

        public List<ApplicationUser> ApplicationUsers { get; set; }

       
        [Display(Name = "VendorId")]
        public string Name { get; set; }

        [Display(Name = "VendorId")]
        public Guid VendorId { get; set; }
    }
}
