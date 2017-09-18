using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NADAAPI.Models;
using NADAAPI.Repository;
using NADAAPI.ViewModels.AccountViewModels;
using System.Security.Claims;
using NADAAPI.ViewModels.VendorViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NADAAPI.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<ApplicationUser> _userMgr;
        private IVendorRepository _vendorRepository;
        public HomeController(UserManager<ApplicationUser> userMgr, IVendorRepository vendorRepository)
        {
            _userMgr = userMgr;
            _vendorRepository = vendorRepository;
        }
        public IActionResult Index(Guid Id)

        {
            List<VendorAccount> lstvn = new List<VendorAccount>();
            lstvn= _vendorRepository.GetVendorAccountsByVendorId(Id);
            AccountViewModel listVm = new AccountViewModel();
            listVm.ApplicationUsers = lstvn.Select(u => u.ApplicationUser).ToList();
            listVm.VendorId = Id;
            listVm.VendorName = _vendorRepository.GetVendor(Id).Name;
            return View(listVm);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string Id,Guid vendorId)
        {
            AccountViewModel vm1 = new AccountViewModel();
            vm1.EndPoints = _vendorRepository.Endpoints;
            vm1.VendorId = vendorId;
            vm1.VendorName = _vendorRepository.GetVendor(vendorId).Name;
            //vm1.VendorId = _vendorRepository;
            if (Id != null)
            {
                try
                {
                    var user = await _userMgr.FindByIdAsync(Id);
                    var userClaims = await _userMgr.GetClaimsAsync(user);
                    vm1.MemberId = user.Id;
                    vm1.Email = user.Email;

                    foreach (var item in vm1.EndPoints)
                    {
                        foreach (var c in userClaims)
                        {
                            if (item.EndPointUri == c.Value)
                            {
                                item.IsChecked = true;
                            }
                        }
                    }
                    return View(vm1);
                }
                catch (Exception)
                {
                    TempData["Result"] = "1";
                    TempData["ResultMessage"] = "Failed to Add User, Please check all the details of the user are filled.";
                }
               
            }

            return View(vm1);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(AccountViewModel accountVM)
        {
            if (ModelState.IsValid)
            {
                if (accountVM.MemberId != null)
                {
                    try
                    {
                        var user = await _userMgr.FindByIdAsync(accountVM.MemberId);
                        if (accountVM.Password != null)
                        {
                            await _userMgr.RemovePasswordAsync(user);
                            await _userMgr.AddPasswordAsync(user, accountVM.Password);
                        }
                        var userClaims = await _userMgr.GetClaimsAsync(user);
                        var result = await _userMgr.RemoveClaimsAsync(user, userClaims);
                        if (result.Succeeded)
                        {
                            List<EndPoint> endPointList = accountVM.EndPoints;

                            foreach (var endpoint in endPointList)
                            {
                                if (endpoint.IsChecked)
                                {
                                    await _userMgr.AddClaimAsync(user, new Claim("EndPoint", endpoint.EndPointUri));
                                }
                            }
                            TempData["Result"] = "0";
                            TempData["ResultMessage"] = "Successfully Updated UserClaims";
                        }
                        else
                        {
                            TempData["Result"] = "1";
                            TempData["ResultMessage"] = "Failed to Remove the Existing UserClaims";
                        }
                    }
                    catch (Exception)
                    {
                        TempData["Result"] = "1";
                        TempData["ResultMessage"] = "Failed to Update the UserClaims";
                    }
                   
                    return RedirectToAction("EditUser", "Home", new  { Id =accountVM.MemberId, vendorId = accountVM.VendorId });
                }

                else
                {
                    try
                    {
                        var userCheck = await _userMgr.FindByEmailAsync(accountVM.Email);
                        if (userCheck == null)
                        {
                            var user = new ApplicationUser()
                            {
                                UserName = accountVM.Email,
                                Email = accountVM.Email,
                            };
                            var userResult = await _userMgr.CreateAsync(user, accountVM.Password);
                            if (userResult.Succeeded)
                            {
                                VendorAccount vact = new VendorAccount();
                                Vendor v = _vendorRepository.GetVendor(accountVM.VendorId);
                                vact.ApplicationUser = user;
                                vact.Vendor = v;
                                _vendorRepository.SaveVendorAccount(vact);

                                List<EndPoint> endPointList = accountVM.EndPoints;
                                foreach (var endpoint in endPointList)
                                {
                                    if (endpoint.IsChecked && userResult.Succeeded)
                                    {
                                        await _userMgr.AddClaimAsync(user, new Claim("EndPoint", endpoint.EndPointUri));
                                    }
                                }
                                TempData["Result"] = "0";
                                TempData["ResultMessage"] = "Successfully Created User and UserClaims";
                                return RedirectToAction("EditUser", "Home", new { Id = user.Id, vendorId = accountVM.VendorId });
                            }

                            TempData["Result"] = "1";
                            TempData["ResultMessage"] = userResult.ToString();
                        }
                        else
                        {
                            TempData["Result"] = "1";
                            TempData["ResultMessage"] = String.Format("The UserAccount : {0} Already exists in the System. Please try a new User",accountVM.Email);
                            return RedirectToAction("EditUser", "Home", new {  vendorId = accountVM.VendorId });
                        }
                       
                    }
                    catch (Exception e)
                    {
                        TempData["Result"] = "1";
                        TempData["ResultMessage"] = ((System.ArgumentException)e).Message;
                    }
                }
            }
            return View(accountVM);

        }
        [HttpGet]
        public async Task<IActionResult> DeleteUser(Guid Id, Guid vendorId)
        {
            var user = await _userMgr.FindByIdAsync(Id.ToString());
            AccountViewModel acctVm = new AccountViewModel();
            acctVm.MemberId = Id.ToString();
            var userClaims = await _userMgr.GetClaimsAsync(user);
            List<string> myList = new List<string>();
            foreach (var item in userClaims)
            {
                myList.Add(item.Value);
            }
            acctVm.UserEndPoints = myList;
            acctVm.Email = user.Email;
            acctVm.VendorId = vendorId;
            acctVm.VendorName = _vendorRepository.GetVendor(vendorId).Name;
            return View(acctVm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(AccountViewModel accountVM)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userMgr.FindByIdAsync(accountVM.MemberId.ToString());
                    var userClaims = await _userMgr.GetClaimsAsync(user);
                    var result = await _userMgr.RemoveClaimsAsync(user, userClaims);
                    if (result.Succeeded)
                    {
                        var vendorAcctId = _vendorRepository.GetVendorAccountByUserId(accountVM.MemberId);

                        _vendorRepository.DeleteVendorAccount(vendorAcctId.Vendor_AccountId.ToString());
                        _vendorRepository.DeleteUser(accountVM.MemberId);
                    }
                    TempData["Result"] = "0";
                    TempData["ResultMessage"] = "Successfully Deleted the User";
                }
                catch (Exception)
                {
                    TempData["Result"] = "1";
                    TempData["ResultMessage"] = "Failed to Delete the User";
                }
               
                return RedirectToAction("Index", "Home", new { Id = accountVM.VendorId });
            }
            return View(accountVM);

        }



        //*******GetOne******

        [Authorize(Policy = "VendorCheck")]
        [Route("Home/GetOne")]
        public string GetOne()
        {
            return "Action1";
        }

        //******GetTwo*******

        [Route("Home/GetTwo")]
        public string GetTwo()
        {
            return "Action2";
        }

        //*****GetThree *******

        [Authorize(Policy = "VendorCheck")]
        [Route("Home/GetThree")]
        public string GetThree()
        {
            return "Action3";
        }

    }
}
