using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Students.Web.Models;

namespace Students.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();
        private const string UsersKey = "UsersKey";
        private const string AuthCook = "AuthCook";
        public HomeController(ILogger<HomeController> logger, IMemoryCache cache, IWebHostEnvironment environment)
        {
            _logger = logger;
            _cache = cache;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var cookie = Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    ViewBag.Password = cache.Password;
                    ViewBag.UserName = cache.Login;
                }
            }
            return View();
        }

        public IActionResult AlreadyAuthed()
        {
            return View();
        }

        public IActionResult Login()
        {
            var cookie = Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    if (cache.Expired > DateTime.Now)
                    {
                        return RedirectToAction(nameof(AlreadyAuthed));
                    }
                    else
                    {
                        _cache.Remove(cookieHash);
                    }
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel viewModel)
        {
            var cookie = Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    if (cache.Expired > DateTime.Now)
                    {
                        return RedirectToAction(nameof(AlreadyAuthed));
                    }
                    else
                    {
                        _cache.Remove(cookieHash);
                    }
                }
            }
            var users = _cache.Get<List<UserCacheModel>>(UsersKey);
            if (users == null)
            {
                users = new List<UserCacheModel>();
                _cache.GetOrCreate(UsersKey, f => users);
            }

            var user = users.FirstOrDefault(q => q.Login == viewModel.Login && q.Password == viewModel.Password);
            if (user != null)
            {
                var identity = _cache.GetOrCreate(user.Hash, f => user);
                Response.Cookies.Append(AuthCook,user.Hash);
            }
            else
            {
                ModelState.AddModelError("", "Пользователь не найден");
                return View(viewModel);
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Register()
        {
            var cookie = Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    if (cache.Expired > DateTime.Now)
                    {
                        return RedirectToAction(nameof(AlreadyAuthed));
                    }
                    else
                    {
                        _cache.Remove(cookieHash);
                    }
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel viewModel)
        {
            var cookie = Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    if (cache.Expired > DateTime.Now)
                    {
                        return RedirectToAction(nameof(AlreadyAuthed));
                    }
                    else
                    {
                        _cache.Remove(cookieHash);
                    }
                }
            }

            var users = _cache.Get<List<UserCacheModel>>(UsersKey);
            if (users == null)
            {
                users = new List<UserCacheModel>();
                _cache.GetOrCreate(UsersKey, f => users);
            }

            if (users.Any(q=> q.Login == viewModel.Login))
            {
                ModelState.AddModelError("", "Пользователь с таким логином уже существует");
                return View(viewModel);
            }

            var pass = GetPassword(viewModel.Login);
            if (pass == null)
            {
                ModelState.AddModelError("", "Превышена длина логина");
                return View(viewModel);
            }

            var user = new UserCacheModel
            {
                Expired = DateTime.Now.AddMinutes(30),
                Login = viewModel.Login,
                Password = pass,
                Hash = GetHash(pass)
            };
            users.Add(user); 
            _cache.Remove(UsersKey);
            _cache.Remove(user.Hash);
            _cache.GetOrCreate(UsersKey, f => users);
            _cache.GetOrCreate(user.Hash, f => user);
            Response.Cookies.Append(AuthCook, user.Hash);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult LogOff()
        {
            var cookie = Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    _cache.Remove(cookieHash);

                }
                Response.Cookies.Delete(AuthCook);
            }
            return RedirectToAction(nameof(Index));
        }
        
        private string GetHash(string password)
        {
            using var derivedBytes = new Rfc2898DeriveBytes(password, saltSize: 16, iterations: 5000, HashAlgorithmName.SHA512);
            var salt = derivedBytes.Salt;
            byte[] key = derivedBytes.GetBytes(16); // 128 bits key
            return Convert.ToBase64String(key);
        }

        private string GetPassword(string login)
        {
            char[] low = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).ToArray();
            char[] upper = low.Select(char.ToUpper).ToArray();
            int[] numbers = Enumerable.Range(1, 9).ToArray();
            const int M = 12;
            int N = login.Length;
            var first = (int)(Math.Pow(N, 3) % 5) + 1;
            var second = (int)(Math.Pow(N, 2) % 6) + 1 + first;
            var sb = new StringBuilder();

            for (int i = 1; i <= first; i++)
            {
                sb = sb.Append(low[_random.Next(0, low.Length)]);
            }

            for (int i = first + 1; i <= second; i++)
            {
                sb = sb.Append(upper[_random.Next(0, upper.Length)]);
            }

            for (int i = second + 1; i < M; i++)
            {
                sb = sb.Append(numbers[_random.Next(0, numbers.Length)]);
            }

            if (login.Length > 1 + (2d / 3d) * (double)M)
            {
                return null;
            }

            return string.Concat(sb.ToString().Take(M));
        }

        public IActionResult Files()
        {
            var files = Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, "files"));
            return View(files);
        }

        public IActionResult File(string guid)
        {
            var file = Path.Combine(_environment.WebRootPath, "files", guid);
            if (System.IO.File.Exists(file))
            {
                var text = System.IO.File.ReadAllText(file);
                return View(text);
            }
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                string path = "/files/" + uploadedFile.FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                await using var fileStream = System.IO.File.Create(Path.Combine(_environment.WebRootPath, path));
                await uploadedFile.CopyToAsync(fileStream);
                return RedirectToAction(nameof(Files));
            }

            return RedirectToAction(nameof(Files));
        }
    }

    public class RegisterViewModel
    {
        public string Login { get; set; }
    }

    public class LoginViewModel : RegisterViewModel
    {
        public string Password { get; set; }
    }

    public class UserCacheModel : LoginViewModel
    {
        public string Hash { get; set; }
        public DateTime Expired { get; set; }
    }
}
