using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System;
using System.Text;

[assembly: OwinStartup(typeof(TiaCompilerCLI.Startup))]

namespace TiaCompilerCLI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure Web API
            HttpConfiguration config = new HttpConfiguration();

            // Enable attribute routing
            config.MapHttpAttributeRoutes();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(
                new System.Net.Http.Headers.MediaTypeHeaderValue("text/html")
            );
            config.Formatters.JsonFormatter.SupportedEncodings.Clear();
            config.Formatters.JsonFormatter.SupportedEncodings.Add(Encoding.UTF8);

            // Configure default route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var controllerTypes = config.Services.GetHttpControllerTypeResolver()
                .GetControllerTypes(config.Services.GetAssembliesResolver());

            foreach (var type in controllerTypes)
            {
                Console.WriteLine($"Controller founded: {type.FullName}");
            }

            // Add Web API to OWIN pipeline
            app.UseWebApi(config);
        }
    }
}