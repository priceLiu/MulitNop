//add by hz full page
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;


namespace Nop.Web.HzVendor
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<VendorService>().As<IVendorService>().InstancePerHttpRequest();
            builder.RegisterType<VendorTemplateService>().As<IVendorTemplateService>().InstancePerHttpRequest();
            builder.RegisterType<BranchService>().As<IBranchService>().InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
