using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dvcsharp_core_api.Models;
using dvcsharp_core_api.Data;
using dvcsharp_core_api.Service;
using Microsoft.Extensions.Configuration;

namespace dvcsharp_core_api
{
   [Route("api/[controller]")]
   public class RegistrationsController : Controller
   {
      private readonly GenericDataContext _context;
      private readonly IConfiguration _configuration;
      private readonly IUserService _userService;

      public RegistrationsController(IConfiguration configuration,
         GenericDataContext context,
         IUserService userService)
      {
         _context = context;
         _userService = userService;
      }

      [HttpPost]
      public IActionResult Post([FromBody] RegistrationRequest registrationRequest)
      {
         if(!ModelState.IsValid)
         {
            return BadRequest(ModelState);
         }

         var existingUser = _context.Users.
            Where(b => b.email == registrationRequest.email).
            FirstOrDefault();

         if(existingUser != null) {
            ModelState.AddModelError("email", "Email address is already taken");
            return BadRequest(ModelState);
         }

         var user = new Models.User();
         user.name = registrationRequest.name;
         user.email = registrationRequest.email;
         user.role = Models.User.RoleUser;
         user.createdAt = user.updatedAt = DateTime.Now;
         _userService.UpdatePassword(ref user, registrationRequest.password);

         _context.Users.Add(user);
         _context.SaveChanges();

         return Ok(user);
      }
   }
}