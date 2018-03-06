using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace WindowsService
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public static bool VaidateUser(string username, string password)
        {
            // Check if it is valid credential  
            if (username == Common.httpUser & password == Common.httpPassword)//CheckUserInDB(username, password))  
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (Common.httpUser == "" && Common.httpPassword == "")
            {

            }
            else
            {
                if (actionContext.Request.Headers.Authorization == null)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    // Gets header parameters  
                    string authenticationString = actionContext.Request.Headers.Authorization.Parameter;
                    string originalString = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationString));

                    // Gets username and password  
                    string username = originalString.Split(':')[0];
                    string password = originalString.Split(':')[1];

                    // Validate username and password  
                    if (!VaidateUser(username, password))
                    {
                        // returns unauthorized error  
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                }
            }

            base.OnAuthorization(actionContext);
        }
    }
}
