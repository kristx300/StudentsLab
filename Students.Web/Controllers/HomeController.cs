using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [Block]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();
        private const string UsersKey = "UsersKey";
        private const string AuthCook = "AuthCook";
        private static List<string> Attempts = new List<string>();
        public HomeController(IMemoryCache cache, IWebHostEnvironment environment)
        {
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
                    if (cache.Expired > DateTime.Now)
                    {
                        ViewBag.Password = cache.Password;
                        ViewBag.UserName = cache.Login;
                    }
                    else
                    {
                        _cache.Remove(cookieHash);
                    }
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
            if (string.IsNullOrWhiteSpace(viewModel.Login))
            {
                ModelState.AddModelError("Login", "Логин пустой");
                return View(viewModel);
            }
            if (string.IsNullOrWhiteSpace(viewModel.Password))
            {
                ModelState.AddModelError("Password", "Пароль пустой");
                return View(viewModel);
            }
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
                var key = $"IP{HttpContext.Connection.LocalIpAddress}";
                Attempts.Add(key);
                if (Attempts.Count(a=> a == key) >=5)
                {
                    Attempts.RemoveAll(a => a == key);
                    _cache.GetOrCreate(key, k=> DateTime.Now.AddMinutes(5));
                }
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
            if (string.IsNullOrWhiteSpace(viewModel.Login))
            {
                ModelState.AddModelError("Login", "Логин пустой");
                return View(viewModel);
            }
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

            var pass = Utility.GetPassword(viewModel.Login);
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
                Hash = Utility.GetHash(pass)
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
        
        

        [Auth]
        public IActionResult Files()
        {
            var files = Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, "files"));
            var models = new List<StudentFile>();

            foreach (var fileName in files)
            {
                var student = StudentFile.Parse(Path.GetFileName(fileName));
                if (student.CheckStudentFile() == null)
                {
                    models.Add(student);
                }
            }

            return View(models);
        }

        [Auth]
        public IActionResult File(StudentFile model)
        {
            try
            {
                var text = model.ReadFile(_environment.WebRootPath);
                ViewBag.Content = text;
                return View(model);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [Auth]
        public IActionResult Upload()
        {
            return View();
        }

        [Auth]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                try
                {
                    var student = StudentFile.Parse(uploadedFile.FileName);
                    if (student == null)
                    {
                        ModelState.AddModelError("", "Ошибка разбора имени файла, длина меньше 13 символов");
                        return View();
                    }
                    using var readStream = new StreamReader(uploadedFile.OpenReadStream());
                    var content = await readStream.ReadToEndAsync();
                    student.SaveFile(_environment.WebRootPath, content);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("",e.Message);
                    return View();
                }
            }
            return RedirectToAction(nameof(Files));
        }
    }
}
