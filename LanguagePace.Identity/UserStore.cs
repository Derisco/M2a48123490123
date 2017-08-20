/* Copyright(C) LanguagePace.Com 
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* Written by Travis Wiggins, Erik Hattervig
* <LanguagePace@Yahoo.com>,
* July 29th 2017
*/

using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LanguagePace.MySQL;

namespace LanguagePace.Identity
{
    public class UserStore<TUser> : IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserTwoFactorStore<TUser, string>,
        IUserLockoutStore<TUser, string>,
        IUserStore<TUser>
        where TUser : IdentityUser
    {

        private string DBConnectionStringName = "LanguagePaceDB";

        public MySQLDatabase Database { get; private set; }

        public IQueryable<TUser> Users
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Default Constuctor
        /// </summary>
        public UserStore()
        {
            new UserStore<TUser>(new MySQLDatabase(DBConnectionStringName));
        }

        /// <summary>
        ///     Constructor that takes MySQLDatabase
        /// </summary>
        /// <param name="database"></param>
        public UserStore(MySQLDatabase database)
        {
            Database = database;
        }

        /// <summary>
        ///     Insert a new TUser in the User Table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task CreateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            // Insert into userTable   TODO: Update Stored Proc to take all fields.
            var result = Database.ExecuteStoredProcedure("user_Insert",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "email", user.Email },
                    { "passwordHash", user.PasswordHash },
                    { "firstName", user.FirstName },
                    { "lastName", user.LastName },
                    { "defalutLanguage", user.DefaultLanguage }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Returns a TUser instance based on userId
        /// </summary>
        /// <param name="userId">The user's ASP Id</param>
        /// <returns></returns>
        public Task<TUser> FindByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId");
            }

            var result = Database.QueryStoredProcedure("user_SelectById",
                new Dictionary<string, object>() { { "userId", userId } });

            return Task.FromResult((TUser)Helpers.Hydrate<IdentityUser>(result).FirstOrDefault());

        }

        /// <summary>
        ///     Returns a TUser instance based on userName
        /// </summary>
        /// <param name="userName">The user's userName</param>
        /// <returns></returns>
        public Task<TUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }

            var result = Database.QueryStoredProcedure("user_SelectByUserName",
                new Dictionary<string, object>() { { "userName", userName } });

            return Task.FromResult((TUser)Helpers.Hydrate<IdentityUser>(result).FirstOrDefault());
        }

        /// <summary>
        ///     Returns a TUser instance based on email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<TUser> FindByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("Null or emty argument: email");
            }

            var result = Database.QueryStoredProcedure("user_SelectByEmail",
                new Dictionary<string, object>() { { "email", email } });

            return Task.FromResult((TUser)Helpers.Hydrate<IdentityUser>(result).FirstOrDefault());
        }

        /// <summary>
        ///     Updates the UsersTable profile information with the TUser instance values, update based on user Id.
        ///     Will only update the following:
        ///     <list type="bullet">
        ///         <item><term>FirstName</term></item>
        ///         <item><term>LastName</term></item>
        ///         <item><term>Bio</term></item>
        ///         <item><term>DefaultLanguage</term></item>
        ///     </list>
        /// </summary>
        /// <param name="user">TUser object being updated.</param>
        /// <returns></returns>
        public Task UpdateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_UpdateProfileById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "firstName", user.FirstName },
                    { "lastName", user.LastName },
                    { "bio", user.Bio },
                    { "defaultLanguage", user.DefaultLanguage }
                });
            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Disposes of the database object.
        /// </summary>
        public void Dispose()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

        /// <summary>
        ///     Inserts a claim to the UserClaimsTable for the given user
        /// </summary>
        /// <param name="user">User to have claim added</param>
        /// <param name="claim">Claim to be added</param>
        /// <returns></returns>
        public Task AddClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("Null argument: user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            Database.ExecuteStoredProcedure("userclaim_Insert",
                new Dictionary<string, object>
                {
                    { "userId", user.Id },
                    { "claimType", claim.Type },
                    { "claimValue", claim.Value }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Returns all claims for a given user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("userclaim_SelectByUser",
                new Dictionary<string, object>() { { "userId", user.Id } });

            // Claim and ClaimsIdentity are types with no default constructers, Hydrate won't work here
            var claims = new ClaimsIdentity();
            foreach (var entry in result)
            {
                claims.AddClaim(new Claim(entry["ClaimType"], entry["ClaimValue"]));
            }

            return Task.FromResult<IList<Claim>>(claims.Claims.ToList());
        }

        /// <summary>
        ///     Removes a claim from a user
        /// </summary>
        /// <param name="user">User to have claim removed</param>
        /// <param name="claim">Claim to be removed</param>
        /// <returns></returns>
        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            Database.ExecuteStoredProcedure("userclaim_DeleteByClaim",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "claimType", claim.Type },
                    { "claimValue", claim.Value }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Inserts a Login in the UserLoginsTable for a given User, 
        ///     (Logins for google, facebook, ect)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            Database.ExecuteStoredProcedure("userlogin_Insert",
                new Dictionary<string, object>()
                {
                    { "loginProvider", login.LoginProvider },
                    { "providerKey", login.ProviderKey },
                    { "userId", user.Id }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Returns a TUser based on the Login info
        ///     (Used for google, facebook, ect Logins)
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var result = Database.QueryStoredProcedure("userlogin_SelectByLoginProviderAndProviderKey",
                new Dictionary<string, object>()
                {
                    { "loginProvider", login.LoginProvider },
                    { "providerKey", login.ProviderKey }
                });
            // If a result exsits, there is a login match
            if (result.Count > 0)
            {
                // Getting the user from the UserLoginInfo result
                TUser user = FindByIdAsync(result.FirstOrDefault()["UserId"]).Result;
                return Task.FromResult<TUser>(user);
            }

            return Task.FromResult<TUser>(null);
        }

        /// <summary>
        ///     Returns list of UserLoginInfo for a given TUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var results = Database.QueryStoredProcedure("userlogin_SelectByUserId",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (results.Count > 0)
            {
                var logins = new List<UserLoginInfo>();
                foreach (var entry in results)
                {
                    logins.Add(new UserLoginInfo(entry["LoginProvider"], entry["ProviderKey"]));
                }

                return Task.FromResult<IList<UserLoginInfo>>(logins);
            }

            return Task.FromResult<IList<UserLoginInfo>>(null);
        }

        /// <summary>
        ///     Deletes a login from the UserLoginsTable for a given TUser
        /// </summary>
        /// <param name="user">User to have login removed</param>
        /// <param name="login">Login to be removed</param>
        /// <returns></returns>
        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            Database.ExecuteStoredProcedure("userlogin_DeleteByUserIdAndLoginProviderAndProviderKey",
                new Dictionary<string, object>()
                {
                    { "loginProvider", login.LoginProvider },
                    { "providerKey", login.ProviderKey },
                    { "userId", user.Id }
                });

            return Task.FromResult<Object>(null);
        }

        /// <summary>
        ///     Inserts an entry in the UserRoles table
        /// </summary>
        /// <param name="user">User to have role added</param>
        /// <param name="roleName">Name of the role to be added to user</param>
        /// <returns></returns>
        public Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (String.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            var result = Database.QueryStoredProcedure("role_SelectByName",
                new Dictionary<string, object>() { { "roleName", roleName } });

            IdentityRole role = null;
            if (result.Count == 0) // No Role exsits
            {
                // Add role to database
                role = new IdentityRole(roleName);
                Database.ExecuteStoredProcedure("role_Insert",
                    new Dictionary<string, object>
                    {
                        { "roleId", role.Id },
                        { "roleName", role.Name }
                    });
            }
            else
            {
                // Creating object from database
                role = new IdentityRole(result.First()["Name"], result.First()["Id"]);
            }

            Database.ExecuteStoredProcedure("userRole_Insert",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "roleId", role.Id }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Returns the roles for a given TUser
        /// </summary>
        /// <param name="user">User roles are being receved for</param>
        /// <returns>List of Role's names</returns>
        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("userRole_SelectByUserId",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count() > 0)
            {
                var roles = new List<string>();
                foreach (var entry in result)
                {
                    roles.Add(entry["Name"]);
                }

                return Task.FromResult<IList<string>>(roles);
            }

            return Task.FromResult<IList<string>>(null);
        }

        /// <summary>
        ///     Verifies if a user is in a role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task<bool> IsInRoleAsync(TUser user, string role)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            var roles = GetRolesAsync(user).Result;

            if (roles != null && roles.Contains(role))
                return Task.FromResult<bool>(true);
            return Task.FromResult<bool>(true);
        }

        /// <summary>
        ///     Removes a user from a role. Unimplemented, need stored proc
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task RemoveFromRoleAsync(TUser user, string role)
        {
            //TODO: Add stored proc
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Deletes a user
        /// </summary>
        /// <param name="user">User being deleted</param>
        /// <returns></returns>
        public Task DeleteAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_DeleteById",
                new Dictionary<string, object> { { "userId", user.Id } });

            return Task.FromResult<Object>(null);
        }

        /// <summary>
        ///     Returns the PasswordHash for a given TUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetPasswordHashAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            return Task.FromResult<string>(result.FirstOrDefault()["PasswordHash"]);
        }

        /// <summary>
        ///     Verifies if user has password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> HasPasswordAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            var hasPassword = !string.IsNullOrEmpty(result.FirstOrDefault()["PasswordHash"]);

            return Task.FromResult<bool>(Boolean.Parse(hasPassword.ToString()));
        }

        /// <summary>
        ///     Sets the password hash for a given TUser
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(passwordHash))
            {
                throw new ArgumentNullException("passwordHash");
            }

            Database.ExecuteStoredProcedure("user_UpdatePasswordById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "newPasswordHash", passwordHash }
                });

            return Task.FromResult<Object>(null);
        }

        /// <summary>
        ///     Sets the security stamp for a TUser
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stamp"></param>
        /// <returns></returns>
        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(stamp))
            {
                throw new ArgumentNullException("stamp");
            }

            Database.ExecuteStoredProcedure("user_UpdateSecurityStampById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "securityStamp", stamp }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Gets the security stamp for a TUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetSecurityStampAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                return Task.FromResult<string>(null);

            return Task.FromResult<string>(result.FirstOrDefault()["SecurityStamp"]);
        }

        /// <summary>
        ///     Sets the email for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task SetEmailAsync(TUser user, string email)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            Database.ExecuteStoredProcedure("user_UpdateEmailById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "email", email }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Get the email for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetEmailAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                return Task.FromResult<string>(null);

            return Task.FromResult<string>(result.FirstOrDefault()["Email"]);
        }

        /// <summary>
        ///     Returns if the email for a user is confirmed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                throw new Exception("User does not exsist");

            return Task.FromResult<bool>(Boolean.Parse(result.FirstOrDefault()["EmailConfermed"]));
        }

        /// <summary>
        ///     Sets the email confermed for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_UpdateEmalConfirmedById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "emailConfirmed", confirmed }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Sets the phoneNumber for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentNullException("phoneNumber");
            }

            Database.ExecuteStoredProcedure("user_UpdatPhoneNumberById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "phoneNumber", phoneNumber }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Gets the phoneNumber of a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                return Task.FromResult<string>(null);

            return Task.FromResult<string>(result.FirstOrDefault()["PhoneNumber"]);
        }

        /// <summary>
        ///     Gets if the PhoneNumber is confirmed for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                throw new Exception("User does not exsist");

            return Task.FromResult<bool>(Boolean.Parse(result.FirstOrDefault()["PhoneNumberConfirmed"]));
        }

        /// <summary>
        ///     Sets the PhoneNumberConfirmed for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_UpdatePhoneNumberConfirmedById",
                new Dictionary<string, object>()
                {
                    { "usedId", user.Id },
                    { "phoneNumberConfirmed", confirmed }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Sets if TwoFactor Athentication is enabled for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("userUpdateTwoFactorEnabledById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "twoFactorEnable", enabled }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Gets if TwoFactor Athentication is enabled for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                throw new Exception("User does not exsist");

            return Task.FromResult<bool>(Boolean.Parse(result.FirstOrDefault()["TwoFactorEnabled"]));
        }

        /// <summary>
        ///     Gets the Lockout End Date for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                throw new Exception("User does not exsist");

            return Task.FromResult<DateTimeOffset>(DateTimeOffset.Parse(result.FirstOrDefault()["PhoneNumberConfirmed"]));
        }

        /// <summary>
        ///     Sets the Lockout End Date for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <returns></returns>
        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_UpdateLockoutEndDateById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "lockoutEndDate", lockoutEnd }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Increments the Access Failed Count for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>The count after the increment</returns>
        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_IncrementAccessFailedCountById",
                new Dictionary<string, object>() { { "userId", user.Id } });

            return Task.FromResult<int>(int.Parse(result.FirstOrDefault()["AccessFailedCount"]));
        }

        /// <summary>
        ///     Resets the Access Failed Count for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task ResetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_ResetAccessFailedCountById",
                new Dictionary<string, object>() { { "userId", user.Id } });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Gets the Access Failed Count for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectAccessFailedCountById",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                throw new Exception("User does not exsist");

            return Task.FromResult<int>(int.Parse(result.FirstOrDefault()["AccessFailedCount"]));
        }

        /// <summary>
        ///     Gets if the user has lockout enabled
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = Database.QueryStoredProcedure("user_SelectByIdLogin",
                new Dictionary<string, object>() { { "userId", user.Id } });

            if (result.Count == 0)
                throw new Exception("User does not exsist");

            return Task.FromResult<bool>(Boolean.Parse(result.FirstOrDefault()["LockoutEnabled"]));
        }

        /// <summary>
        ///     Sets if the user has lockout enabled
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Database.ExecuteStoredProcedure("user_UpdateLockoutEnabledById",
                new Dictionary<string, object>()
                {
                    { "userId", user.Id },
                    { "lockoutEnabled", enabled }
                });

            return Task.FromResult<object>(null);
        }
    }
}
