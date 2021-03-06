﻿using Moonglade.Setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moonglade.Tests
{
    [TestFixture]
    public class SetupHelperTests
    {
        [Test]
        public void TestInstanceCreationInvalidConnectionString()
        {
            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                var helper = new SetupHelper(null);
            });

            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                var helper = new SetupHelper(string.Empty);
            });

            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                var helper = new SetupHelper(" ");
            });
        }

        [Test]
        public void TestInstanceCreationGoodConnectionString()
        {
            var connStr = "Server=(localdb)\\MSSQLLocalDB;Database=moonglade-dev;Trusted_Connection=True;";
            var helper = new SetupHelper(connStr);
            Assert.IsTrue(helper.DatabaseConnectionString == connStr);
        }
    }
}
