using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LanguagePace.Startup))]
namespace LanguagePace
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
