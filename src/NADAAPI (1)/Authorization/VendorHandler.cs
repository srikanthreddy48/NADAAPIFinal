using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NADAAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NADAAPI.Authorization
{
    
    public class VendorHandler : AuthorizationHandler<VendorRequirement>
    {
        private readonly IVendorRepository _vendorRepository;
        private string _reqstdControllerAction;
        public VendorHandler(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }
        protected override Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          VendorRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "aud"))
            {
                return Task.CompletedTask;
            }

            var vendorId = context.User.Claims.Where(x => x.Type == "VendorId");
            string str = null;
            foreach (var item in vendorId)
            {
                str = item.Value;
            }
            Guid g = new Guid(str);
            var userName = context.User.FindFirst(c => c.Type == ClaimTypes.Email).Value;

            var vendorAccount = _vendorRepository.GetVendorAccount(str, userName);

            if(vendorAccount == null)
            {
                return Task.CompletedTask;
            }

            string action = ((ActionContext)context.Resource).RouteData.Values["action"].ToString();
            string controller = ((ActionContext)context.Resource).RouteData.Values["controller"].ToString();


            _reqstdControllerAction = controller + "/" + action;

            if (!context.User.HasClaim(c => c.Type == "EndPoint"))
            {
                return Task.CompletedTask;
            }

            var vendorAction = context.User.Claims.Where(x => x.Type == "EndPoint");

            foreach (var rst in vendorAction)
            {
                if (rst.Value == _reqstdControllerAction  && vendorAccount != null)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
