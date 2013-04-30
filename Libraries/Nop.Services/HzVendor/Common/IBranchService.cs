// add by hz - full page
using Nop.Core.Domain.Common;

namespace Nop.Services.Common
{
    /// <summary>
    /// Branch service interface
    /// </summary>
    public partial interface IBranchService
    {
        /// <summary>
        /// Deletes an barnch
        /// </summary>
        /// <param name="branch">Branch</param>
        void DeleteBranch(Branch branch);

        /// <summary>
        /// Gets total number of branches by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Number of branches</returns>
        int GetBranchTotalByCountryId(int countryId);

        /// <summary>
        /// Gets total number of branches by state/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <returns>Number of branches</returns>
        int GetBranchTotalByStateProvinceId(int stateProvinceId);

        /// <summary>
        /// Gets an branch by branch identifier
        /// </summary>
        /// <param name="branchId">branch identifier</param>
        /// <returns>Branch</returns>
        Branch GetBranchById(int branchId);

        /// <summary>
        /// Inserts a branch
        /// </summary>
        /// <param name="branch">Branch</param>
        void InsertBranch(Branch branch);

        /// <summary>
        /// Updates the branch
        /// </summary>
        /// <param name="branch">Branch</param>
        void UpdateBranch(Branch branch);
    }
}