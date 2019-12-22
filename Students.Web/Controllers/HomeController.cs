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

    public class StudentFile
    {
        public int Year { get; set; }

        public string University { get; set; }

        public string Student { get; set; }

        public string Faculty { get; set; }

        public string Form { get; set; }

        public StudentFile()
        {
        }

        public StudentFile(int year, string university, string student, string faculty, string form)
        {
            Year = year;
            University = university;
            Student = student;
            Faculty = faculty;
            Form = form;
        }

        public string CheckStudentFile()
        {
            if (Year < 1900 || Year > 2025)
            {
                return "Ошибка указания года (символы с 1 по 4)";
            }

            if (string.IsNullOrWhiteSpace(University))
            {
                return "Код вузка пустой или равен NULL (символы с 5 по 10)";
            }

            if (University.Length != 6)
            {
                return "Длина кода вуза не равна 6 (символы с 5 по 10)";
            }

            if (string.IsNullOrWhiteSpace(Student))
            {
                return "Код данных пустой или равен NULL (символ 11)";
            }

            if (Student != "S")
            {
                return "Указаны не данные студента (символ 11)";
            }

            if (string.IsNullOrWhiteSpace(Faculty))
            {
                return "Код факультета пустой или равен NULL (символ 12)";
            }

            if (Faculty.Length != 1)
            {
                return "Длина кода факультета не равна 1 (символ 12)";
            }

            if (!("MTI".Any(q=> q.ToString() == Faculty)))
            {
                return "Код не равен допустимому значению \"M\" \"T\" \"I\" (символ 12)";
            }

            if (string.IsNullOrWhiteSpace(Form))
            {
                return "Код формы обучения пустой или равен NULL (символ 13)";
            }

            if (Form.Length != 1)
            {
                return "Длина кода формы обучения не равна 1 (символ 13)";
            }

            if (!("DZ".Any(q => q.ToString() == Form)))
            {
                return "Код не равен допустимому значению \"D\" \"Z\" (символ 13)";
            }
            return null;
        }

        /*
Первые четыре символа являются годом. 
Следующие шесть символов код вуза. 
Десятый символ S – указывает на то, что это данные студента. 
Одиннадцатый символ: 
М – студент машиностроительного факультета; 
Т - студент технического факультета; 
I – студент экономического факультета. 
Двенадцатый символ:
D – студент дневного обучения; 
Z – студент заочного обучения. 
         */

        public string ReadFile(string root)
        {
            var checkStatus = CheckStudentFile();
            if (checkStatus == null)
            {
                var file = Path.Combine(root, "files", $"{Year}{University}{Student}{Faculty}{Form}.txt");
                if (System.IO.File.Exists(file))
                {
                    var text = System.IO.File.ReadAllText(file);
                    return text;
                }
                else
                {
                    throw new FileNotFoundException("Файл по указанной моделе не найден");
                }
            }
            else
            {
                throw new ArgumentException("Ошибка проверки класса " + checkStatus);
            }
        }

        public void SaveFile(string root,string content)
        {
            var checkStatus = CheckStudentFile();

            if (checkStatus == null)
            {

                var file = Path.Combine(root, "files", $"{Year}{University}{Student}{Faculty}{Form}.txt");
                if (System.IO.File.Exists(file))
                {
                    throw new Exception("Файл уже существует");
                }
                File.WriteAllText(file, content);
            }
            else
            {
                throw new ArgumentException("Ошибка проверки класса " + checkStatus);
            }
        }

        public static StudentFile Parse(string fileName)
        {
            if (fileName.Length < 13)
            {
                return null;
            }
            var model = new StudentFile
            {
                University = fileName.Substring(4,6),
                Student = fileName.Substring(10,1),
                Faculty = fileName.Substring(11,1),
                Form = fileName.Substring(12,1)
            };
            int.TryParse(fileName.Substring(0, 4), out var year);
            model.Year = year;
            return model;
        }
    }
}
