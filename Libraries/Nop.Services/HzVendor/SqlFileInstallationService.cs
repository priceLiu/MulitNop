//add by hz full page

namespace Nop.Services.Installation
{
    public partial class SqlFileInstallationService : IInstallationService
    {
        #region Methods

        public void InstallDataVendor(bool installSampleData = true)
        {
            ExecuteSqlFile(_webHelper.MapPath("~/App_Data/Install/create_required_vendor_data.sql"));
            //vendor local resources are already installed with no resources

            if (installSampleData)
            {
                ExecuteSqlFile(_webHelper.MapPath("~/App_Data/Install/create_sample_vendor_data.sql"));
            }
        }
        #endregion
    }
}