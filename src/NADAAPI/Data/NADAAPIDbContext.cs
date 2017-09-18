using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using NADAAPI.Models;

namespace NADAAPI.Data
{
    public class NADAAPIDbContext : IdentityDbContext<ApplicationUser>
    {
        public NADAAPIDbContext(DbContextOptions<NADAAPIDbContext> options)
            : base(options)
        {
           
        }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<VendorAccount> VendorAccount { get; set; }

        public DbSet<EndPoint> EndPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<VendorAccount>()
              .HasOne(p => p.Vendor)
              .WithMany(i => i.VendorAccounts);

        }
    }

}
