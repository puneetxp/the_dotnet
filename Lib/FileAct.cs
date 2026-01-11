using System.IO;
using Microsoft.AspNetCore.Http; // Requires ASP.NET Core framework reference

namespace The.DotNet.Lib
{
    public class FileAct
    {
        private string fileField;
        private string dir;
        private string publicPath;
        private string baseUrl = "/storage"; 

        public FileAct(string fileField, string dir)
        {
            this.fileField = fileField;
            this.dir = dir;
            // Ensure dir exists
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static FileAct Init(string fileField, string prefix = "../storage")
        {
            return new FileAct(fileField, prefix);
        }

        public FileAct Public(string publicDir, string prefix = "/storage")
        {
            this.baseUrl = prefix;
            this.publicPath = Path.Combine(this.baseUrl, publicDir);
            this.dir = Path.Combine(this.dir, "public", publicDir);
            if (!Directory.Exists(this.dir)) Directory.CreateDirectory(this.dir);
            return this;
        }

        public object Upload(IFormFile file, string name = "")
        {
            if (file == null || file.Length == 0) return "Can't Upload";

            string fileName = string.IsNullOrEmpty(name) ? file.FileName : name + Path.GetExtension(file.FileName);
            string targetPath = Path.Combine(this.dir, fileName);

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return new
            {
                name = file.FileName,
                path = targetPath,
                dir = this.dir,
                @public = Path.Combine(this.publicPath, fileName).Replace("\\", "/") 
            };
        }
    }
}
