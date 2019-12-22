using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Students.Web.Controllers;

namespace Students.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string AuthCook = "AuthCook";
        public AuthAttribute()
        {

        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            IMemoryCache _cache = (IMemoryCache)context.HttpContext.RequestServices.GetService(typeof(IMemoryCache));

            var cookie = context.HttpContext.Request.Cookies.TryGetValue(AuthCook, out var cookieHash);
            if (cookie)
            {
                var cache = _cache.Get<UserCacheModel>(cookieHash);
                if (cache != null)
                {
                    if (cache.Expired < DateTime.Now)
                    {
                        context.Result = new RedirectToActionResult(nameof(HomeController.Login), "Home",null);
                    }
                }
                else
                {
                    context.Result = new RedirectToActionResult(nameof(HomeController.Login), "Home", null);
                }
            }
            else
            {
                context.Result = new RedirectToActionResult(nameof(HomeController.Login), "Home", null);
            }
        }
    }
}
