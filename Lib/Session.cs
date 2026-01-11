using System;

namespace The.DotNet.Lib
{
    public class Session
    {
        // Mock session 
        // In real app, inject IHttpContextAccessor
        public static Dictionary<string, object> Current = new Dictionary<string, object>();

        public static void Set(string key, object value)
        {
            Current[key] = value;
        }

        public static object Get(string key)
        {
            return Current.ContainsKey(key) ? Current[key] : null;
        }

        public static void Destroy()
        {
            Current.Clear();
        }
        
        public static object Create(dynamic auth)
        {
             // Mock creating session
             Set("user_id", auth.id);
             return Response.Json(auth);
        }
    }
}
