using System;
using System.Collections.Generic;
using System.Threading;

namespace The.DotNet.Lib
{
    public class Session
    {
        private static readonly AsyncLocal<Dictionary<string, object>> _current = new();

        public static Dictionary<string, object> Current
        {
            get
            {
                if (_current.Value == null)
                {
                    _current.Value = new Dictionary<string, object>();
                }
                return _current.Value;
            }
        }

        public static void Set(string key, object value)
        {
            Current[key] = value;
        }

        public static object? Get(string key)
        {
            return Current.ContainsKey(key) ? Current[key] : null;
        }

        public static void Destroy()
        {
            Current.Clear();
        }
        
        public static object Create(dynamic auth)
        {
             object? userId = null;
             if (auth != null)
             {
                 try { userId = auth.id; } catch { }
                 if (userId == null)
                 {
                     try { userId = auth["id"]; } catch { }
                 }
             }
             Set("user_id", userId ?? 0);
             return Response.Json(auth);
        }
    }
}
