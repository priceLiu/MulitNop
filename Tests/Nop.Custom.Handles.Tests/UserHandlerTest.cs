using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using NUnit.Framework;

namespace Nop.Custom.Handles.Tests
{
    [TestFixture]
    public class UserHandlerTest
    {
        [Test]
        public void GetCustomerTest()
        {
            //ConfigManager config = new ConfigManager();

            //config.Register();

            UserHandler handler = new UserHandler();
            handler.GetCustomerJson(1);
        }
    }
}
