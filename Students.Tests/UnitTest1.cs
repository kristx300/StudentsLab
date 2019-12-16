using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using Students.Web.Controllers;

namespace Students.Tests
{
    public class Tests
    {
        private string Hash;
        private string Key = "asdab25@5arfwAWD";
        private byte[] Salt;
        private IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        [SetUp]
        public void Setup()
        {
            using var derivedBytes = new Rfc2898DeriveBytes(Key, saltSize: 16, iterations: 5000, HashAlgorithmName.SHA512);
            var salt = derivedBytes.Salt;
            byte[] key = derivedBytes.GetBytes(16); // 128 bits key
            Hash = Convert.ToBase64String(key);
        }

        [Test]
        public void Test1Async()
        {
            var user = new UserCacheModel
            {
                Expired = DateTime.Now.AddMinutes(30),
                Login = "login",
                Password = "1234",
                Hash = "qwer"
            };

            var identity = _cache.GetOrCreate(user.Hash,f=> user);

            var result = _cache.TryGetValue("qwer", out UserCacheModel model);

            Assert.IsTrue(result);
            Assert.AreEqual(user.Hash,model.Hash);
            Assert.AreEqual(identity.Hash, model.Hash);
        }

        [Test]
        public void LoginTest()
        {
            var login = "naimanov";
            const int M = 12;
            Assert.Less(login.Length, 1 + (2d / 3d) * (double)M);
        }
    }
}