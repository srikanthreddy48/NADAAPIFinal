using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NADAAPI.Data;
using NADAAPI.Models;
using NADAAPI.Repository;
using NADAAPI.ViewModels.AccountViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NADAAPI.Controllers
{
    public class AccountController : Controller
    {
        private IPasswordHasher<ApplicationUser> _hasher;
        private UserManager<ApplicationUser> _userMgr;
        private IVendorRepository _vendorRepository;
        private JWToken _token;

        public AccountController( UserManager<ApplicationUser> userMgr,
            IPasswordHasher<ApplicationUser> hasher, IVendorRepository vendorRepository, IOptions<JWToken> token)
        {
            _userMgr = userMgr;
            _hasher = hasher;
            _vendorRepository = vendorRepository;
            _token = token.Value;
        }

     
        [AllowAnonymous]
        [HttpPost("api/account/token")]
        public async Task<IActionResult> CreateToken([FromBody] AccountViewModel model)

        {
            try
            {
              
                var user = await _userMgr.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userMgr.GetClaimsAsync(user);
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                            new Claim(JwtRegisteredClaimNames.Email,user.Email),
                            new Claim("VendorId",_vendorRepository.GetVendorAccountByUserId(user.Id).Vendor.VendorId.ToString())
                        }.Union(userClaims);

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_token.Key));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: _token.Issuer,
                            audience: _token.Audience,
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(15),
                            signingCredentials: creds
                            );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return BadRequest("Failed To Generate Token");
        }
    }
}
