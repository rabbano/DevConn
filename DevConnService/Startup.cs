using Owin;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Routing;
//установить надо Microsoft.AspNet.WebApi.Owin
//Microsoft.AspNet.WebApi.OwinSelfHost

namespace WindowsService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultGetApi",
                routeTemplate: "api/{controller}/{Connection}/{DevType}",
                defaults: new { DevType = RouteParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });

            config.Routes.MapHttpRoute(
                name: "DefaultPostApi",
                routeTemplate: "api/{controller}/{DevType}/{Connection}/{DevNum}",
                defaults: new { },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            //config.Routes.MapHttpRoute(
            //    name: "DefaultPostApi2",
            //    routeTemplate: "api/{controller}",
            //    defaults: new { },
            //    constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            //config.Routes.MapHttpRoute(
            //    name: "PostApiHz",
            //    routeTemplate: "api/{controller}/{DevType}/{Connection}/{DevNum}/Hz",
            //    defaults: new { },
            //    constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });


            app.UseWebApi(config);

            config.Filters.Add(new CustomExceptionFilter());
        }

    }
}
