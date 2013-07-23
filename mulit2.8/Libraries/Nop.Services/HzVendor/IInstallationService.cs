//add  by hz full page
namespace Nop.Services.Installation
{
    public partial interface IInstallationService
    {
        void InstallDataVendor(bool installSampleData = true);
    }
}
