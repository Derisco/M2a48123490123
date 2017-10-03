using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LanguagePace.Send;

namespace LanguagePace.Tests.Email
{
    [TestClass]
    public class EmailTest
    {
        [TestMethod]
        public void SendBasicEmail()
        {
            // Arrange

            // Act
            var success = Send.Email.Send("test@languagepace.com", "Test Email", "This is a test.", false).Result;

            // Assert
            Assert.Equals(success, true);
        }
    }
}
