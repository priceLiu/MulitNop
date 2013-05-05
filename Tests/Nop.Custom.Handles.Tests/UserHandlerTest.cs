using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            ConfigManager config = new ConfigManager();

            config.Register();

            //IRepository<Customer> _customerRepo;
            //IRepository<CustomerRole> _customerRoleRepo;
            //IRepository<GenericAttribute> _genericAttributeRepo;
            //IGenericAttributeService _genericAttributeService;
            //ICustomerService _customerService;
            //CustomerSettings _customerSettings;
            //IEventPublisher _eventPublisher;

            //_customerService = new CustomerService(new NopNullCache(), _customerRepo, _customerRoleRepo,
            //    _genericAttributeRepo, _genericAttributeService, _eventPublisher, _customerSettings);
        }
    }
}
