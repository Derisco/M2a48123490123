/* Copyright (C) LanguagePace.com
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* proprietary and confidential
* Written by Travis Wiggins, Erik Hattervig
August 23rd 2017
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LanguagePace.Identity;
using LanguagePace.MySQL;

namespace LanguagePace.Tests.Data
{
    [TestClass]
    public class AccountDataTest
    {
        [TestMethod]
        public void CreateUser()
        {
            var newUser = new IdentityUser()
            {
                Id = "TestUser",
                Email = "test@languagepace.com",
                UserName = "TestUser",
                PasswordHash = "Test",
                FirstName = "Test",
                LastName = "McTest",
                DefaultLanguage = 1
            };

            // Hard clean this test user from db if it is there.
            var db = new MySQLDatabase("LanguagePaceDB");
            db.Execute("delete from languagepace.user where Id = 'TestUser';", null);

            var userStore = new UserStore<IdentityUser>();

            // Call creat user
            userStore.CreateAsync(newUser);

            // See if user is in DB
            var user = userStore.FindByEmailAsync("test@languagepace.com").Result;

            // Hard clean user from DB
            db.Execute("delete from languagepace.user where Id = 'TestUser';", null);

            // See if a user came back from DB
            Assert.IsNotNull(user);

        }
    }
}
