using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LanguagePace.Helpers;

namespace LanguagePace.Controllers
{
    public class CultureController : Controller
    {
        // GET: /SetPreferredCulture/culture
        [AllowAnonymous]
        public ActionResult SetPreferredCulture(string culture, string returnUrl)
        {
            Response.SetPreferredCulture(culture);

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl);
        }
    }
}