using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Models
{
    public class VendorAccount
    {
        [Key]
        public Guid Vendor_AccountId { get; set; }

        public Vendor Vendor { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
