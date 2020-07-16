using System.IO;
using autapse_backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace autapse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Location : Controller
    {
        private string BaseDirectory = Program.Config.BaseDirectory;

        public string[] Index()
        {
            return GetDirectories(BaseDirectory);
        }

        [HttpPost("absolute")]
        public string[] DirectoriesFromPathAbsolute([FromBody] string path)
        {
            return GetDirectories(path);
        }

        [HttpPost("relative")]
        public string[] DirectoriesFromPathRelative([FromBody] string path)
        {
            return GetDirectories(BaseDirectory + Path.DirectorySeparatorChar + path);
        }

        private string[] GetDirectories(string path)
        {
            var dirs = Directory.GetDirectories(path);
            var retDirs = new string[dirs.Length];
            for (int i = 0; i < dirs.Length; i++)
            {
                retDirs[i] = Path.GetRelativePath(BaseDirectory, dirs[i]);
            }

            return retDirs;
        }

        private string[] GetFiles(string path)
        {
            var dirs = Directory.GetFiles(path);
            var retDirs = new string[dirs.Length];
            for (var i = 0; i < dirs.Length; i++)
            {
                retDirs[i] = Path.GetRelativePath(BaseDirectory, dirs[i]);
            }

            return retDirs;
        }
    }
}