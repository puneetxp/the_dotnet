using System;
using System.Security.Cryptography;
using System.Text;

namespace The.DotNet.Lib
{
    public class Auth
    {
        public static object Login(string email, string password)
        {
            // Pseudo:
            // var user = User.Find(email, "email");
            // if (user != null && VerifyHash(password, user.password)) 
            //    return Session.Create(user);
            return Response.NotFound("User Not Found");
        }

        public static object Register(string name, string email, string password)
        {
             // var existing = User.Find(email, "email");
             // if (existing != null) return Response.Unprocessable(new { email = "Taken" });
             // var user = User.Create(new { name, email, password = Hash(password) });
             // return Session.Create(user);
             return Response.Json("Register logic placeholder");
        }

        public static string Hash(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
