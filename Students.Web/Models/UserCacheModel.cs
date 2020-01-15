using System;

namespace Students.Web.Models
{
    public class UserCacheModel : LoginViewModel
    {
        public string Hash { get; set; }
        public DateTime Expired { get; set; }
    }
}