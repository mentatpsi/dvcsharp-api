using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Security.Claims;
using dvcsharp_core_api.Data;
using Microsoft.Extensions.Configuration;

namespace dvcsharp_core_api.Models
{
   public class User
   {
      public const string RoleUser = "User";
      public const string RoleSupport = "Support";
      public const string RoleAdministrator = "Administrator";
      public const string TokenSecret = "f449a71cff1d56a122c84fa478c16af9075e5b4b8527787b56580773242e40ce";

      public int ID { get; set; }

      [Required]
      public string name { get; set; }
      [Required]
      public string email { get; set; }
      [Required]
      public string role { get; set; }
      [Required]
      [System.Runtime.Serialization.IgnoreDataMember]
      public string password { get; set; }
      [Required]
      public DateTime createdAt { get; set; }
      [Required]
      public DateTime updatedAt { get; set; }

      
   }
}