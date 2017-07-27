/* Copyright(C) LanguagePace.Com 
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* Written by Travis Wiggins
<LanguagePace@Yahoo.com>,
July 24th 2017
*/

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
