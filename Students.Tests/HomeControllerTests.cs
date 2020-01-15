using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using Students.Web.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing.Template;
using Students.Web.Models;

namespace Students.Tests
{
    public partial class HomeControllerTests
    {
        private readonly string ExecutingFolder = Path.Combine(Environment.CurrentDirectory, "files");
        private const string UsersKey = "UsersKey";
        private const string AuthCook = "AuthCook";
        private const string AuthoredUserHash = "FaBtgeX2D9w7rwXI9st/lw==";
        private readonly UserCacheModel AuthoredUser = new UserCacheModel
        {
            Expired = DateTime.Now.AddMinutes(30),
            Hash = AuthoredUserHash,
            Login = "naimanov",
            Password = "yugFJJLS438"
        };

        private const string ExpiredAuthoredUserHash = "kzZZNYD5709M0i8ASEkc+Q==";
        private readonly UserCacheModel ExpiredAuthoredUser = new UserCacheModel
        {
            Expired = DateTime.Now.AddMinutes(-360),
            Hash = ExpiredAuthoredUserHash,
            Login = "naimanexp",
            Password = "xppoiMJUW17"
        };

        private const string UserLogin = "naimanov";
        private const string UserLoginNew = "na@manov";
        private const string UserLoginNewLong = "na@manovkeyval";
        private const string UserPassword = "yugFJJLS438";
        private const string UserPasswordIncorrect = "yugFJJLS499";

        [Test]
        [TestCase(AuthCook, "")]
        [TestCase(AuthCook, ExpiredAuthoredUserHash)]
        public void Index_ShouldReturnEmptyLoginAndPassword_IfNoAuth(string key, string val)
        {
            var rcc= new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });

            var h = Create(rcc);

            var result = h.Index();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ViewResult), result);
            Assert.IsNull(h.ViewBag.Password);
            Assert.IsNull(h.ViewBag.UserName);
        }

        [Test]
        [TestCase(AuthCook, AuthoredUserHash)]
        public void Index_ShouldReturnLoginAndPassword_IfAuth(string key,string val)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });

            var h = Create(rcc);

            var result = h.Index();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ViewResult), result);
            Assert.IsNotNull(h.ViewBag.Password);
            Assert.IsNotNull(h.ViewBag.UserName);
            Assert.AreEqual(AuthoredUser.Password, h.ViewBag.Password);
            Assert.AreEqual(AuthoredUser.Login, h.ViewBag.UserName);
        }


        [Test]
        [TestCase(AuthCook, AuthoredUserHash)]
        public void Login_ShouldReturnRedirectToAlreadyAuthed_IfAuth(string key, string val)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });

            var h = Create(rcc);

            var result = h.Login();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
            Assert.AreEqual(nameof(HomeController.AlreadyAuthed), (result as RedirectToActionResult).ActionName);
        }

        [Test]
        [TestCase(AuthCook, "")]
        [TestCase(AuthCook, ExpiredAuthoredUserHash)]
        public void Login_ShouldReturnViewResult_IfNoAuth(string key, string val)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });

            var h = Create(rcc);

            var result = h.Login();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        [Test]
        [TestCase(AuthCook, AuthoredUserHash)]
        public void Register_ShouldReturnRedirectToAlreadyAuthed_IfAuth(string key, string val)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() {{key, val}});

            var h = Create(rcc);

            var result = h.Register();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
            Assert.AreEqual(nameof(HomeController.AlreadyAuthed), (result as RedirectToActionResult).ActionName);
        }

        [Test]
        [TestCase(AuthCook, "")]
        [TestCase(AuthCook, ExpiredAuthoredUserHash)]
        public void Register_ShouldReturnViewResult_IfNoAuth(string key, string val)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });


            var h = Create(rcc);

            var result = h.Register();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        [Test]
        [TestCase(AuthCook, AuthoredUserHash, UserLogin, UserPassword)]
        [TestCase(AuthCook, AuthoredUserHash, UserLogin, UserPasswordIncorrect)]
        [TestCase(AuthCook, AuthoredUserHash, UserLoginNew, UserPassword)]
        [TestCase(AuthCook, AuthoredUserHash, UserLoginNew, UserPasswordIncorrect)]
        public void LoginPost_ShouldReturnRedirectToAlreadyAuthed_IfAuth(string key, string val, string login, string password)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });
            
            var h = Create(rcc);

            var result = h.Login(new LoginViewModel {Login = login, Password = password});

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
            Assert.AreEqual(nameof(HomeController.AlreadyAuthed), (result as RedirectToActionResult).ActionName);
        }

        [Test]
        [TestCase(AuthCook, "", UserLogin, UserPassword)]
        [TestCase(AuthCook, ExpiredAuthoredUserHash, UserLogin, UserPassword)]
        public void LoginPost_ShouldReturnRedirectToIndex_IfNoAuthAndCorrectData(string key, string val, string login, string password)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });

            var h = Create(rcc);

            var result = h.Login(new LoginViewModel {Login = login, Password = password});

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
            Assert.AreEqual(nameof(HomeController.Index), (result as RedirectToActionResult).ActionName);
        }

        [Test]
        [TestCase(AuthCook, "", UserLogin, UserPasswordIncorrect)]
        [TestCase(AuthCook, ExpiredAuthoredUserHash, UserLogin, UserPasswordIncorrect)]
        [TestCase(AuthCook, "", UserLoginNew, UserPasswordIncorrect)]
        [TestCase(AuthCook, ExpiredAuthoredUserHash, UserLoginNew, UserPasswordIncorrect)]
        public void LoginPost_ShouldReturnViewResult_IfNoAuthAndIncorrectData(string key, string val,string login,string password)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });
            
            var h = Create(rcc);

            var result = h.Login(new LoginViewModel
            {
                Login = login,
                Password = password
            });

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        [Test]
        [TestCase(AuthCook, "", UserLogin)]
        [TestCase(AuthCook, AuthoredUserHash, UserLogin)]
        [TestCase(AuthCook, ExpiredAuthoredUserHash, UserLogin)]

        [TestCase(AuthCook, "", UserLoginNew)]
        [TestCase(AuthCook, AuthoredUserHash, UserLoginNew)]
        [TestCase(AuthCook, ExpiredAuthoredUserHash, UserLoginNew)]

        [TestCase(AuthCook, "", UserLoginNewLong)]
        [TestCase(AuthCook, AuthoredUserHash, UserLoginNewLong)]
        [TestCase(AuthCook, ExpiredAuthoredUserHash, UserLoginNewLong)]
        public void RegisterPost(string key, string val, string login)
        {
            var rcc = new RequestCookieCollection(new Dictionary<string, string>() { { key, val } });

            var h = Create(rcc);

            var result = h.Register(new RegisterViewModel()
            {
                Login = login
            });

            Assert.IsNotNull(result);

            if (val == AuthoredUserHash && key == AuthCook)
            {
                Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
                Assert.AreEqual(nameof(HomeController.AlreadyAuthed), (result as RedirectToActionResult).ActionName);
            }
            else if (login == UserLogin)
            {
                Assert.IsInstanceOf(typeof(ViewResult), result);
                Assert.AreEqual(h.ModelState.Count, 1);
            }
            else if (login == UserLoginNewLong)
            {
                Assert.IsInstanceOf(typeof(ViewResult), result);
                Assert.AreEqual(h.ModelState.Count, 1);
            }
            else
            {
                Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
                Assert.AreEqual(nameof(HomeController.Index), (result as RedirectToActionResult).ActionName);
            }
        }

        private HomeController Create(RequestCookieCollection rcc)
        {
            var _cache = new MemoryCache(new MemoryCacheOptions());
            _cache.GetOrCreate(AuthoredUser.Hash, f => AuthoredUser);
            _cache.GetOrCreate(ExpiredAuthoredUser.Hash, f => ExpiredAuthoredUser);

            var users = new List<UserCacheModel>
            {
                AuthoredUser,ExpiredAuthoredUser
            };

            _cache.GetOrCreate(UsersKey, f => users);
            var hostingEnvironment = Mock.Of<IWebHostEnvironment>();
            hostingEnvironment.WebRootPath = ExecutingFolder;

            var requestMock = new Mock<HttpRequest>();
            requestMock.SetupGet(prop => prop.Cookies).Returns(rcc);

            var responseMock = new Mock<HttpResponse>();
            responseMock.SetupGet(prop => prop.Cookies).Returns(new RequestCookieCollection());

            var connectionMock = new Mock<ConnectionInfo>();
            connectionMock.SetupGet(prop => prop.LocalIpAddress).Returns(IPAddress.Parse("192.168.1.1"));

            var contextMock = new Mock<HttpContext>(() => new DefaultHttpContext());
            contextMock.SetupGet(prop => prop.Request).Returns(requestMock.Object);
            contextMock.SetupGet(prop => prop.Connection).Returns(connectionMock.Object);
            contextMock.SetupGet(prop => prop.Response).Returns(responseMock.Object);

            var h = new HomeController(_cache, hostingEnvironment)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = contextMock.Object,
                }
            };
            return h;
        }
    }
}