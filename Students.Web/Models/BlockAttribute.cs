using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace Students.Web.Models
{
    public class BlockAttribute : Attribute,IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var _cache = context.HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;

            var key = $"IP{context.HttpContext.Connection.LocalIpAddress}";
            if (_cache.TryGetValue(key, out var obj) && obj is DateTime dt)
            {
                if (dt < DateTime.Now)
                {
                    _cache.Remove(key);
                }
                else
                {
                    context.Result = new StatusCodeResult(423);
                }
            }
        }
    }
}
