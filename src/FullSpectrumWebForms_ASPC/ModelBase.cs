using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSW_ASPC
{
    public sealed class ModelBase<T> : Microsoft.AspNetCore.Mvc.RazorPages.PageModel where T : FSWPage, new()
    {
        public string CurrentPageID { get; private set; }
        public string CurrentPageAuth { get; private set; }

        public string CurrentPageTypePath { get; private set; }

        public void OnGet()
        {
            HttpContext.Request.Cookies.TryGetValue("polinetSessionId", out string fswSessionId);
            HttpContext.Request.Cookies.TryGetValue("polinetSessionAuth", out string fswSessionAuth);

            var res = FSW.ModelBase.RegisterFSWPage(new T(), fswSessionId, fswSessionAuth, out string newFSWSessionId, out string newFSWSessionAuth);

            CurrentPageID = res.id.ToString();
            CurrentPageAuth = res.auth;
            CurrentPageTypePath = typeof(T).AssemblyQualifiedName;

            if (newFSWSessionId != null && newFSWSessionAuth != null)
            {
                HttpContext.Response.Cookies.Append("polinetSessionId", newFSWSessionId);
                HttpContext.Response.Cookies.Append("polinetSessionAuth", newFSWSessionAuth);
            }
        }
    }
}
