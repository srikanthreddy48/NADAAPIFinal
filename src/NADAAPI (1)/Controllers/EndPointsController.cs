using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NADAAPI.Repository;
using NADAAPI.ViewModels.AccountViewModels;
using NADAAPI.ViewModels.EndPointsViewModel;
using NADAAPI.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NADAAPI.Controllers
{
    public class EndPointsController : Controller
    {
        private IVendorRepository _vendorRepository;
        public EndPointsController(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        [HttpGet]
        public IActionResult GetEndpoints()
        {
            EndPointsViewModel endPointsVM = new EndPointsViewModel();
            endPointsVM.EndPoints = _vendorRepository.Endpoints;

            return View(endPointsVM);
        }

        [HttpGet]
        public IActionResult EditEndpoint(Guid Id, string uri)
        {
            EndPointsViewModel endPointsVM = new EndPointsViewModel();
            endPointsVM.EndPointId = Id;
            endPointsVM.EndPointUri = uri;
            return View(endPointsVM);
        }
        [HttpPost]
        public IActionResult EditEndpoint(EndPointsViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.EndPointId != null)
                {
                    try
                    {
                        EndPoint endPoint = _vendorRepository.GetEndPoint(model.EndPointId);
                        endPoint.EndPointUri = model.EndPointUri;
                        _vendorRepository.UpdateEndPoint(endPoint);
                        TempData["Result"] = "0";
                        TempData["ResultMessage"] = "Successfully Updated the EndPoint";
                    }
                    catch (Exception)
                    {
                        TempData["Result"] = "1";
                        TempData["ResultMessage"] = "Failed to Update the EndPoint";
                    }
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult DeleteEndPoint(Guid Id, string uri)
        {
            EndPointsViewModel endPointsVM = new EndPointsViewModel();
            endPointsVM.EndPointId = Id;
            endPointsVM.EndPointUri = uri;
            return View(endPointsVM);
        }
        [HttpPost]
        public IActionResult DeleteEndPoint(EndPointsViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (model.EndPointId != null)
                {
                    try
                    {
                        _vendorRepository.DeleteEndPoint(model.EndPointId);
                        TempData["Result"] = "0";
                        TempData["ResultMessage"] = "Successfully Deleted the Vendor";
                    }
                    catch (Exception)
                    {
                        TempData["Result"] = "1";
                        TempData["ResultMessage"] = "Failed to Delete the Vendor, Make Sure the UserAccounts Under this vendor are Deleted first.";
                    }

                }
            }

            return RedirectToAction("GetEndpoints");
        }

        [HttpGet]
        public IActionResult CreateEndPoint()
        {

            return View();
        }

        [HttpPost]
        public IActionResult CreateEndPoint(EndPointsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EndPoint endPoint = new EndPoint();
                    endPoint.EndPointUri = model.EndPointUri;
                    endPoint.Id = Guid.NewGuid();
                    endPoint.IsChecked = false;
                    _vendorRepository.SaveEndPoint(endPoint);
                    TempData["Result"] = "0";
                    TempData["ResultMessage"] = "Successfully Created EndPoint";  
                }
                catch (Exception)
                {
                    TempData["Result"] = "1";
                    TempData["ResultMessage"] = "Failed to Delete the EndPoint";
                }

            }
            return View(model);
        }
    }
}
