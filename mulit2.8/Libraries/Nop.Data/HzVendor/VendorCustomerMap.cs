//add by hz full page
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class VendorCustomerMap : EntityTypeConfiguration<VendorCustomer>
    {
        public VendorCustomerMap()
        {
            this.ToTable("Vendor_Customer_Mapping");
            this.HasKey(sc => sc.Id);
            
            this.HasRequired(sc => sc.Customer)
                .WithMany()
                .HasForeignKey(sc => sc.CustomerId);


            this.HasRequired(sc => sc.Vendor)
                .WithMany(s => s.VendorCustomers)
                .HasForeignKey(sc => sc.VendorId);
        }
    }
}