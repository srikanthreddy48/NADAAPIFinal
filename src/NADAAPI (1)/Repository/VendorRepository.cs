using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NADAAPI.Data;
using NADAAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Repository
{
    public class VendorRepository : IVendorRepository
    {
        private NADAAPIDbContext _nadaDbContext;
        private UserManager<ApplicationUser> _userMgr;

        public VendorRepository(NADAAPIDbContext nadaDbContext, UserManager<ApplicationUser> userMgr)
        {
            _nadaDbContext = nadaDbContext;
            _userMgr = userMgr;
        }

        public List<ApplicationUser> ApplicationUsers
        {
            get
            {
                return _nadaDbContext.Users
                    .Include(i => i.Claims)
                    .OrderBy(e => e.UserName).ToList();
            }
        }

        public List<EndPoint> Endpoints
        {
            get
            {
                return _nadaDbContext.EndPoints.OrderBy(e => e.EndPointUri).ToList();
            }
        }

        public List<Vendor> vendors
        {
            get
            {
                return _nadaDbContext.Vendor.OrderBy(c => c.Name).ToList();
            }
        }

        public List<VendorAccount> GetVendorAccountsByVendorId(Guid vendorId)
        {

            return _nadaDbContext.VendorAccount
                .Include(u=>u.ApplicationUser)
                .Include(v=>v.Vendor)
                .Where(i => i.Vendor.VendorId == vendorId).ToList();
                
        }
        public VendorAccount GetVendorAccount(string VendorId, string UserName)
        {

            var vendorAccount = _nadaDbContext.VendorAccount
                .Include(i => i.Vendor)
                 .Include(i => i.ApplicationUser)
                 //.Where(i => i.Vendor.VendorId == VendorId)
                 .Where(i => i.ApplicationUser.Email == UserName).FirstOrDefault();

            return vendorAccount;
        }

        public VendorAccount GetVendorAccountId(Guid VendorAccountId, string UserName)
        {
            var vn = _nadaDbContext.VendorAccount
                .Include(i => i.ApplicationUser)
                .Where(i => i.Vendor_AccountId == VendorAccountId)
                .Where(i => i.ApplicationUser.Email == UserName).FirstOrDefault();

            return vn;
        }

       

        public VendorAccount GetVendorAccountByUserId(string userId)
        {
            return _nadaDbContext.VendorAccount
               .Include(u => u.ApplicationUser)
               .Include(v => v.Vendor)
               .FirstOrDefault(i => i.ApplicationUser.Id==userId);

        }

        public void SaveVendorAccount(VendorAccount vendoraccount)
        {
            _nadaDbContext.Entry(vendoraccount).State = EntityState.Added;
            _nadaDbContext.SaveChanges();
        }

        public void SaveVendor(Vendor vendor)
        {
            _nadaDbContext.Entry(vendor).State = EntityState.Added;
            _nadaDbContext.SaveChanges();
        }

        public Vendor GetVendor(Guid VendorId)
        {
            return _nadaDbContext.Vendor.SingleOrDefault(v => v.VendorId == VendorId);
        }

        public void DeleteVendor(Guid VendorId)
        {
            Vendor vn = GetVendor(VendorId);
            _nadaDbContext.Remove(vn);
            _nadaDbContext.SaveChanges();
        }

        public void UpdateVendor(Vendor vendor)
        {
            _nadaDbContext.SaveChanges();
        }

        public async void DeleteUser(string Id)
        {
            ApplicationUser ap = await _userMgr.FindByIdAsync(Id.ToString());
            _nadaDbContext.Remove(ap);
            _nadaDbContext.SaveChanges();
        }

        public void DeleteVendorAccount(string VendorAcctId)
        {
            VendorAccount vn = GetVendorAccount(new Guid(VendorAcctId));
            _nadaDbContext.Remove(vn);
            _nadaDbContext.SaveChanges();
        }

        public VendorAccount GetVendorAccount(Guid VendorAcctId)
        {
            return _nadaDbContext.VendorAccount.FirstOrDefault(v => v.Vendor_AccountId == VendorAcctId);
        }

        public EndPoint GetEndPoint(Guid EndPointId)
        {
            return _nadaDbContext.EndPoints.SingleOrDefault(v => v.Id == EndPointId);
        }

        public void UpdateEndPoint(EndPoint EndPoint)
        {
            _nadaDbContext.SaveChanges();
        }

        public void DeleteEndPoint(Guid EndPointId)
        {
            EndPoint endPoint = GetEndPoint(EndPointId);
            _nadaDbContext.Remove(endPoint);
            _nadaDbContext.SaveChanges();
        }

        public void SaveEndPoint(EndPoint EndPoint)
        {
            _nadaDbContext.Entry(EndPoint).State = EntityState.Added;
            _nadaDbContext.SaveChanges();
        }
    }
}
