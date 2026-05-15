using Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{

    public class AccessControl
    {

        public class UserAccess : AuthorizeAttribute
        {
            private Access RequiredAccess { get; set; }

            public UserAccess(Access Access = Access.Anonymous) : base()
            {
                RequiredAccess = Access;
            }

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                bool ajaxRequest = false;
                if (httpContext != null && httpContext.Request != null)
                {
                    ajaxRequest = httpContext.Request.IsAjaxRequest();
                }
                try
                {
                    if (global::Models.User.ConnectedUser == null)
                    {
                        if (!ajaxRequest)
                            httpContext.Response.Redirect("/Accounts/Login?message=Accès non autorisé!&success=false");
                        return false;
                    }
                    else
                    {
                        if (global::Models.User.ConnectedUser.Access < RequiredAccess || global::Models.User.ConnectedUser.Blocked)
                        {
                            if (!ajaxRequest)
                                httpContext.Response.Redirect("/Accounts/Login?message=Accès non autorisé!&success=false");
                            return false;
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
