using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebBotPrototypes.Startup))]
namespace WebBotPrototypes
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
