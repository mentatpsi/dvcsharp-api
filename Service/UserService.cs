using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using dvcsharp_core_api.Data;
using dvcsharp_core_api.Models;
using Microsoft.Extensions.Configuration;

namespace dvcsharp_core_api.Service;

public class UserService(IConfiguration configuration) : IUserService
{
   
   public void UpdatePassword(ref User user, string password)
   {
      user.password = GetHashedPassword(password);
   }

   public string CreateAccessToken(User user)
   {
      //Push Sensitive information into configuration.
      //Configurations can be encrypted.
      string secret = configuration["Authentication:SecretKey"];
      string issuer = configuration["Authentication:Issuer"];
      string audience = configuration["Authentication:Audience"];

      var claims = new[]
      {
         new Claim("name", user.email),
         new Claim("role", user.role)
      };

      var signingKey = new Microsoft.IdentityModel.
         Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

      var creds = new Microsoft.IdentityModel.
         Tokens.SigningCredentials(signingKey, 
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

      var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
         issuer: issuer,
         audience: audience,
         expires: DateTime.Now.AddMinutes(30),
         claims: claims,
         signingCredentials: creds
      );

      return (new System.IdentityModel.Tokens.
         Jwt.JwtSecurityTokenHandler().WriteToken(token));
   }

   private static string GetHashedPassword(string password)
   {
      var sha256 = SHA256.Create();
      var hash = sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(password));

      return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
   }

   public AuthorizationResponse AuthorizeCreateAccessToken(
      GenericDataContext _context, 
      AuthorizationRequest authorizationRequest)
   {
      AuthorizationResponse response = null;

      User user = _context.Users.
         Where(b => b.email == authorizationRequest.email).
         FirstOrDefault();
      
      if(user == null) {
         return response;
      }

      if(GetHashedPassword(authorizationRequest.password) != user.password) {
         return response;
      }

      response = new AuthorizationResponse();
      response.role = user.role;
      response.accessToken = CreateAccessToken(user);

      return response;
   }
}