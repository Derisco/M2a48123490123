/* Copyright(C) LanguagePace.Com 
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* Written by Travis Wiggins, Erik Hattervig
* <LanguagePace@Yahoo.com>,
* July 27th 2017
*/

using System;
using Microsoft.AspNet.Identity;

namespace LanguagePace.Identity
{
    public class IdentityUser : IUser
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public IdentityUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Constructor that takes user name as argument
        /// </summary>
        /// <param name="userName"></param>
        public IdentityUser(string userName) : this()
        {
            UserName = userName;
        }

        /// <summary>
        ///     Asp UserID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     User's Primary Email address, must be unique
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     UserName, must be unique
        /// </summary>
        public string UserName { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        public virtual string Bio { get; set; }

        public virtual int DefaultLanguage { get; set; }

        /// <summary>
        ///     If the user's email has be confermed by email
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        ///     Salted/hashed form of password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a users credentials
        ///     have changed (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        ///     Phone number, used for two factor SMN messages
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        ///     If two factor is enabled for the user.
        /// </summary>
        public virtual string TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DatTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }
    }
}
