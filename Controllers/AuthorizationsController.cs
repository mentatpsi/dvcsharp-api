using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using dvcsharp_core_api.Models;
using dvcsharp_core_api.Data;
using dvcsharp_core_api.Service;
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
      public IActionResult Post([FromBody] AuthorizationRequest authorizationRequest)
      {
         if(!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         var response = _userService.
            AuthorizeCreateAccessToken(_context, authorizationRequest);
            
         if(response == null) {
            return Unauthorized();
         }

         return Ok(response);
      }

      [HttpGet("GetTokenSSO")]
      public IActionResult GetTokenSSO()
      {
         var ssoCookieData = HttpContext.Request.Cookies["sso_ctx"];

         if(String.IsNullOrEmpty(ssoCookieData)) {
            return Unauthorized();
         }

         var ssoCookieDecoded = Convert.FromBase64String(ssoCookieData);
         var ssoCookie = JObject.Parse(System.Text.Encoding.UTF8.GetString(ssoCookieDecoded));

         var userId = ssoCookie["auth_user"];
         if(userId == null) {
            return Unauthorized();
         }

         var user = _context.Users.
            Where(b => b.ID == userId.ToObject<int>()).
            FirstOrDefault();

         if(user == null) {
            return NotFound();
         }

         var response = new Models.AuthorizationResponse();
         response.role = user.role;
         response.accessToken = _userService.CreateAccessToken(user);

         return Ok(response);
      }
   }
}