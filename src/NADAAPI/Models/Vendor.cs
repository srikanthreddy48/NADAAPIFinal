using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Models
{
    public class Vendor
    {
        [Key]
        public Guid VendorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        //[Required]
        //[MaxLength(100)]
        //public string Domain { get; set; }

        public ICollection<VendorAccount> VendorAccounts { get; set; }

    }
}
