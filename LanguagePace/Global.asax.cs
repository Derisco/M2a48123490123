/* Copyright(C) LanguagePace.Com 
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* Written by Travis Wiggins
<LanguagePace@Yahoo.com>,
July 24th 2017
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LanguagePace
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Fires on each request.
        /// </summary>
        protected void Application_OnBeginRequest()
        {
            // Get culture of the client
            LanguagePace.Helpers.CultureHelper.ApplyUserCulture(Request);
        }
    }
}
