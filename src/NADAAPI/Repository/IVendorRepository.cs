using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NADAAPI.Models;

namespace NADAAPI.Repository
{
    public interface IVendorRepository
    {
        List<Vendor> vendors { get; }

        List<EndPoint> Endpoints { get; }
        VendorAccount GetVendorAccountId(Guid VendorAccountId,string UserName);

        VendorAccount GetVendorAccount(string VendorId, string UserName);


        List<ApplicationUser> ApplicationUsers { get; }

        List<VendorAccount> GetVendorAccountsByVendorId(Guid vendorId);

        VendorAccount GetVendorAccountByUserId(string userId);

        void SaveVendorAccount(VendorAccount vendoraccount);
        void SaveVendor(Vendor vendor);

        Vendor GetVendor(Guid VendorId);
        void DeleteVendor(Guid VendorId);
        void UpdateVendor(Vendor vendor);

        void DeleteUser(string Id);

        void DeleteVendorAccount(string VendorAcctId);

        VendorAccount GetVendorAccount(Guid VendorAcctId);

        EndPoint GetEndPoint(Guid EndPointId);

        void UpdateEndPoint(EndPoint EndPoint);
        void DeleteEndPoint(Guid EndPointId);

        void SaveEndPoint(EndPoint EndPoint);


    }
}
