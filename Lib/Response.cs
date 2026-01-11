using System.Collections.Generic;

namespace The.DotNet.Lib
{
    public class Response
    {
        public static object Json(object data)
        {
            // In a real API Controller, you'd return IActionResult
            // This helper might structure the response envelope
            return data;
        }

        public static object NotFound(object data = null)
        {
             return new { error = "Not Found", details = data };
        }

        public static object Unprocessable(object data)
        {
            return new { error = "Unprocessable Entity", details = data };
        }

        public static object Unauthorized(string message = "You Are Not Authorized")
        {
            return new { error = "Unauthorized", message };
        }
    }
}
