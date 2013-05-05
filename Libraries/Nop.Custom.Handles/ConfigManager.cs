using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Nop.Data;
using System.Configuration;
using Nop.Core;
using Autofac;
using Nop.Core.Infrastructure;
using System.Web;
using Nop.Core.Fakes;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Services.Customers;
using Nop.Data;
using Nop.Core.Domain.Customers;

namespace Nop.Custom.Handles
{
    public class ConfigManager
    {
        protected NopObjectContext context;

        public void InitContext()
        {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
            context = new NopObjectContext(ConfigurationManager.ConnectionStrings["serviceDB"].ToString());
            //context.Database.Delete();
            //context.Database.Create();
        }

        protected T SaveAndLoadEntity<T>(T entity, bool disposeContext = true) where T : BaseEntity
        {

            //context.Set<T>().Add(entity);
            //context.SaveChanges();

            //object id = entity.Id;

            //if (disposeContext)
            //{
            //    context.Dispose();
            //    context = new NopObjectContext(GetTestDbName());
            //}
            
            var fromDb = context.Set<T>().Find(entity.Id);
            return fromDb;
        }

        public virtual IContainer Register2()
        {
            var builder = new ContainerBuilder();

            //data layer
            var settings = new DataSettings()
            {
                DataProvider = "sqlserver",
                DataConnectionString = ConfigurationManager.ConnectionStrings["serviceDB"].ToString()
            };

           // var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = settings;
            builder.Register(c => settings).As<DataSettings>();
            builder.Register(x => new EfDataProviderManager(x.Resolve<DataSettings>())).As<BaseDataProviderManager>().InstancePerDependency();


            builder.Register(x => (IEfDataProvider)x.Resolve<BaseDataProviderManager>().LoadDataProvider()).As<IDataProvider>().InstancePerDependency();
            builder.Register(x => (IEfDataProvider)x.Resolve<BaseDataProviderManager>().LoadDataProvider()).As<IEfDataProvider>().InstancePerDependency();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var efDataProviderManager = new EfDataProviderManager(settings);
                var dataProvider = (IEfDataProvider)efDataProviderManager.LoadDataProvider();
                dataProvider.InitConnectionFactory();

                builder.Register<IDbContext>(c => new NopObjectContext(dataProviderSettings.DataConnectionString)).InstancePerHttpRequest();
            }
            else
            {
                builder.Register<IDbContext>(c => new NopObjectContext(settings.DataConnectionString)).InstancePerHttpRequest();
            }


            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerHttpRequest();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerHttpRequest();

            IContainer container = builder.Build();

            return container;
        }

        public virtual void Register()
        {
            var settings = new DataSettings()
            {
                DataProvider = "sqlserver",
                DataConnectionString = ConfigurationManager.ConnectionStrings["serviceDB"].ToString()
            };

            var dataProviderSettings = settings;

            NopObjectContext dbContext = new NopObjectContext(ConfigurationManager.ConnectionStrings["serviceDB"].ToString());
            EfRepository<Customer> r = new Data.EfRepository<Customer>(dbContext);
            Customer d = r.GetById(1);
        }
    }
}
