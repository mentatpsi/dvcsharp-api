using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using dvcsharp_core_api.Models;
using dvcsharp_core_api.Data;
using dvcsharp_core_api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace dvcsharp_core_api
{
   [Route("api/[controller]")]
   public class AuthorizationsController : Controller
   {
      private readonly GenericDataContext _context;
      private readonly IConfiguration _configuration;
      private readonly IUserService _userService;

      public AuthorizationsController(IConfiguration configuration,
         GenericDataContext context,
         IUserService userService)
      {
         _configuration = configuration;
         _context = context;
         _userService = userService;
      }

      [HttpPost]
      [ProducesResponseType<AuthorizationResponse>(StatusCodes.Status200OK)]
      public IActionResult Post([FromBody] AuthorizationRequest authorizationRequest)
      {
         if (!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         var response = _userService.AuthorizeCreateAccessToken(_context, authorizationRequest);

         
         if (response != null)
         {
            var ssoResponse =
               _userService.CreateTemporarySSO(_context, authorizationRequest);

            response.ssoExpiration = ssoResponse.ssoExpiration;
            response.ssoSalt = ssoResponse.ssoSalt;
         }

         if(response == null) {
            return Unauthorized();
         }

         return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(response));
         
      }

      [HttpGet("GetTokenSSO")]
      public IActionResult GetTokenSSO()
      {
         var ssoCookieData = HttpContext.Request.Cookies["sso_ctx"];
         var ssoCookieSalt = HttpContext.Request.Cookies["sso_ctx_s"];
         

         if(String.IsNullOrEmpty(ssoCookieData)) {
            return Unauthorized();
         }

         
         //Base64 Conversion is not secure
         var ssoCookieDecoded = Convert.FromBase64String(ssoCookieData);
         
         var ssoCookie = JObject.Parse(System.Text.Encoding.UTF8.GetString(ssoCookieDecoded));

         var userId = ssoCookie["auth_user"];
         
         
         if(userId == null) {
            return Unauthorized();
         }
         
         int userIdInt = userId.ToObject<int>();
         
         var user = _context.Users
            .FirstOrDefault(
               user => user.ssoSalt == ssoCookieSalt &&
                       user.ssoExpiration > DateTime.Now &&
                       user.ID == userIdInt);

         if(user == null) {
            return NotFound();
         }

         user.ssoExpiration = DateTime.Now;
         _context.Users.Update(user);
         _context.SaveChanges();
         
         var response = new Models.AuthorizationResponse();
         response.role = user.role;
         response.accessToken = _userService.CreateAccessToken(user);

         response.ssoSalt = "Void after current use";
         response.ssoExpiration = user.ssoExpiration;
         
         return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(response));
      }
   }
}