using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace The.DotNet.Lib
{
    // Define a basic User model helper for auth
    public class User : Model
    {
        public User(IDB db) : base(db)
        {
            this.Table = "users";
            this.Name = "user";
        }
    }

    public class Auth
    {
        public static async Task<object> Register(UserManager<IdentityUser> userManager, string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Response.Unprocessable(new Dictionary<string, string> { { "error", "Email and Password are required" } });
            }

            var user = new IdentityUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return new { message = "User registered", email = user.Email };
            }

            return Response.Unprocessable(result.Errors);
        }

        public static async Task<object> Login(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            string email,
            string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Response.Unauthorized("Email and Password are required");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return Response.Unauthorized("Invalid credentials");

            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) return Response.Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user.Email ?? "");

            return new { token = token, email = user.Email };
        }

        public static string GenerateJwtToken(string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey123ForTestingPurposesOnly"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
