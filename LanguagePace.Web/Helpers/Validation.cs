using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguagePace.Helpers
{
    static public class Validation
    {
        /// <summary>
        ///     Checks if the string is a valid email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        static public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }



    }
}