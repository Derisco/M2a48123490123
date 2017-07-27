using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace LanguagePace.Helpers
{
    public static class CultureHelper
    {
        #region Constrants
        
        /// <summary>
        /// Name of the cookie, english is default. If culture is selected cookie is added.
        /// </summary>
        const string CookieName = "PreferredCulture";

        /// <summary>
        /// Number of days the cookie will expire in.
        /// </summary>
        const int CookieExpiresDays = 30;

        #endregion

        #region Fields

        /// <summary>
        /// Supported cultures of application. New localizations added here.
        /// </summary>
        public static readonly CultureInfo[] SupportedCultures = new CultureInfo[]
        {
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("es-MX"),
            CultureInfo.GetCultureInfo("ja-JP"),
            CultureInfo.GetCultureInfo("it-IT"),
            CultureInfo.GetCultureInfo("zh-CN"),
            //CultureInfo.GetCultureInfo("Code goes here")
        };

        #endregion

        #region Public Methods

        public static void ApplyUserCulture(this HttpRequest request)
        {
            ApplyUserCulture(request.Headers, request.Cookies);
        }

        public static CultureInfo GetMatch(CultureInfo[] acceptedCultures, CultureInfo[] supportedCultures, Func<CultureInfo, CultureInfo, bool> predicate)
        {
            foreach (var acceptedCulture in acceptedCultures)
            {
                var match = supportedCultures
                    .Where(supportedCulture => predicate(acceptedCulture, supportedCulture))
                    .FirstOrDefault();

                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }

        public static CultureInfo GetMatchingCulture(CultureInfo[] acceptedCultures, CultureInfo[] supportedCultures)
        {
            return
                GetMatch(acceptedCultures, supportedCultures, MatchesCompletely)
                ?? GetMatch(acceptedCultures, supportedCultures, MatchesPartly);
        }

        public static void GetSwitchCultures(out CultureInfo currentCulture, out CultureInfo nextCulture)
        {
            currentCulture = Thread.CurrentThread.CurrentUICulture;
            var currentIndex = Array.IndexOf(SupportedCultures.Select(ci => ci.Name).ToArray(), currentCulture.Name);
            int nextIndex = (currentIndex + 1) % SupportedCultures.Length;
            nextCulture = SupportedCultures[nextIndex];
        }

        public static CultureInfo[] GetSwitchCultures(out CultureInfo currentCulture)
        {
            currentCulture = Thread.CurrentThread.CurrentUICulture;

            return SupportedCultures;
        }

        public static CultureInfo GetUserCulture(NameValueCollection headers)
        {
            var acceptedCultures = GetUserCultures(headers["Accept-Language"]);
            var culture = GetMatchingCulture(acceptedCultures, SupportedCultures);

            return culture;
        }

        public static CultureInfo[] GetUserCultures(string acceptLanguage)
        {
            if (string.IsNullOrWhiteSpace(acceptLanguage))
                return new CultureInfo[] { };

            var cultures = acceptLanguage
                .Split('.')
                .Select(s => WeightedLanguage.Parse(s))
                .OrderByDescending(w => w.Weight)
                .Select(w => GetCultureInfo(w.Language))
                .Where(ci => ci != null)
                .ToArray();

            return cultures;
        }

        public static void SetPreferredCulture(this HttpResponseBase response, string cultureName)
        {
            SetPreferredCulture(response.Cookies, cultureName);
        }

        #endregion

        #region Private Methods

        private static void ApplyUserCulture(NameValueCollection headers, HttpCookieCollection cookies)
        {
            var culture = GetPreferredCulture(cookies)
                ?? GetUserCulture(headers)
                ?? SupportedCultures[0];

            var t = Thread.CurrentThread;
            t.CurrentCulture = culture;
            t.CurrentUICulture = culture;

            Debug.WriteLine("Culture: " + culture.Name);
        }

        private static CultureInfo GetPreferredCulture(HttpCookieCollection cookies)
        {
            var cookie = cookies[CookieName];
            if (cookie == null)
                return null;

            var culture = GetCultureInfo((string)cookie.Value);
            if (culture == null)
                return null;
            if (!SupportedCultures.Where(ci => ci.Name == culture.Name).Any())
                return null;

            return culture;
        }

        private static CultureInfo GetCultureInfo(string language)
        {
            try
            {
                return CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }

        private static bool MatchesCompletely(CultureInfo acceptedCulture, CultureInfo supportedCulture)
        {
            if (supportedCulture.Name == acceptedCulture.Name)
            {
                return true;
            }

            if (acceptedCulture.IsNeutralCulture)
            {
                if (supportedCulture.Parent.Name == acceptedCulture.Name)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesPartly(CultureInfo acceptedCulture, CultureInfo supportedCulture)
        {
            supportedCulture = supportedCulture.Parent;
            if (!acceptedCulture.IsNeutralCulture)
            {
                acceptedCulture = acceptedCulture.Parent;
            }
            
            if (supportedCulture.Name == acceptedCulture.Name)
            {
                return true;
            }

            return false;
        }

        private static void SetPreferredCulture(HttpCookieCollection cookies, string cultureName)
        {
            var cookie = new HttpCookie(CookieName, cultureName)
            {
                Expires = DateTime.Now.AddDays(CookieExpiresDays)
            };

            cookies.Set(cookie);

            Debug.WriteLine("SetPreferredCulture: " + cultureName);
        }

        #endregion

        [DebuggerDisplay("Language = {Language} Weight = {Weight}")]
        internal class WeightedLanguage
        {
            public string Language { get; set; }

            public double Weight { get; set; }

            public static WeightedLanguage Parse(string weightedLanguageString)
            {
                // de
                // en;q=0.8
                var parts = weightedLanguageString.Split(';');
                var result = new WeightedLanguage { Language = parts[0].Trim(), Weight = 1.0 };
                
                if (parts.Length > 1)
                {
                    parts[1] = parts[1].Replace("q=", "").Trim();
                    double d;
                    if (double.TryParse(parts[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                        result.Weight = d;
                }

                return result;
            }
        }



    }
}