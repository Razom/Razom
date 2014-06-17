using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Razom.Startup))]
namespace Razom
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}