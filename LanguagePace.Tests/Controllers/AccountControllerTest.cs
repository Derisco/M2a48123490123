/* Copyright (C) LanguagePace.com
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* proprietary and confidential
* Written by Travis Wiggins, Erik Hattervig
August 23rd 2017
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using LanguagePace.Controllers;
using LanguagePace.Models;
using LanguagePace.Identity;
using LanguagePace.MySQL;

namespace LanguagePace.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void RegisterView()
        {
            // Arrange
            AccountController controller = new AccountController();

            // Act
            ViewResult result = controller.Register() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RegisterAction()
        {
            // Arrange
            AccountController controller = new AccountController();

            RegisterViewModel model = new RegisterViewModel()
            {
                Email = "test@languagepace.com",
                Username = "testUser",
                Password = "PasswordTest1!",
                ConfirmPassword = "PasswordTest1!",
                DefaultLanguage = 1,
                FirstName = "Test",
                LastName = "McTest"
            };

            // Act

            MySQLDatabase db = new MySQLDatabase("LanguagePaceDB");
            db.Execute("delete from languagepace.user where email='test@languagepace.com'", null);

            try
            {
                ViewResult result = controller.Register(model).Result as ViewResult;

                UserStore<IdentityUser> userStore = new UserStore<IdentityUser>(new MySQLDatabase("LanguagePaceDB"));
                IdentityUser user = userStore.FindByEmailAsync("test@languagepace.com").Result;

                // Assert
                Assert.IsNotNull(result);
                Assert.IsNotNull(user);
            }
            finally
            {
                db.Execute("delete from languagepace.user where email='test@languagepace.com'", null);

            }
        }

        [TestMethod]
        public void ForgotPasswordView()
        {
            // Arrange
            AccountController controller = new AccountController();

            // Act
            ViewResult result = controller.ForgotPassword() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

    }
}
