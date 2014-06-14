using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(chat2.Startup))]
namespace chat2
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}