using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NADAAPI.Repository;
using NADAAPI.ViewModels.VendorViewModels;
using NADAAPI.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NADAAPI.Controllers
{
    public class VendorController : Controller
    {
        private IVendorRepository _vendorRepository;

        public VendorController(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        public ViewResult List()
        {
            VendorListViewModel vendorsListViewModel = new VendorListViewModel();
            vendorsListViewModel.Vendors = _vendorRepository.vendors;
            return View(vendorsListViewModel);
        }
        [HttpGet]
        public IActionResult CreateVendor()
        {
           
            return View();
        }

        [HttpPost]
        public IActionResult CreateVendor(VendorListViewModel model)
        {
           if(ModelState.IsValid)
            {
                try
                {
                    Vendor v = new Vendor();
                    v.Name = model.Name;
                    v.VendorId = Guid.NewGuid();
                    _vendorRepository.SaveVendor(v);
                    TempData["Result"] = "0";
                    TempData["ResultMessage"] = "Successfully Created Vendor";
                }
                catch (Exception)
                {
                    TempData["Result"] = "1";
                    TempData["ResultMessage"] = "Failed to Create the Vendor. Please check the Vendor Name";
                }
              
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult DeleteVendor(Guid Id)
        {
            Vendor v = _vendorRepository.GetVendor(Id);
            VendorListViewModel vm = new VendorListViewModel();
            vm.VendorId = Id;
            vm.Name = v.Name;
            return View(vm);
        }
        [HttpPost]
        public IActionResult DeleteVendor(VendorListViewModel model)
        {
            if (ModelState.IsValid)
            {
              
                if (model.VendorId != null)
                {
                    try
                    {
                        _vendorRepository.DeleteVendor(model.VendorId);
                        TempData["Result"] = "0";
                        TempData["ResultMessage"] = "Successfully Deleted the Vendor";
                    }
                    catch (Exception )
                    {
                        TempData["Result"] = "1";
                        TempData["ResultMessage"] = "Failed to Delete the Vendor, Make Sure the UserAccounts Under this vendor are Deleted first.";
                    }
                   
                }
            }
            return RedirectToAction("List");
        }
        [HttpGet]
        public IActionResult EditVendor(Guid Id)
        {
                Vendor v = _vendorRepository.GetVendor(Id);
                VendorListViewModel vm = new VendorListViewModel();
                vm.VendorId = Id;
                vm.Name = v.Name;
                return View(vm);
        }


        [HttpPost]
        public IActionResult EditVendor(VendorListViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (model.VendorId != null)
                {
                    try
                    {
                        Vendor vendor = _vendorRepository.GetVendor(model.VendorId);
                        vendor.Name = model.Name;
                        _vendorRepository.UpdateVendor(vendor);
                        TempData["Result"] = "0";
                        TempData["ResultMessage"] = "Successfully Edited the Vendor";
                    }
                    catch (Exception)
                    {
                        TempData["Result"] = "1";
                        TempData["ResultMessage"] = "Failed to Edit the Vendor";
                    }
                  
                }
            }

            return View(model);
        }
    }
}
