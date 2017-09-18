using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<VendorAccount> VendorAccounts { get; set; }
    }
}
