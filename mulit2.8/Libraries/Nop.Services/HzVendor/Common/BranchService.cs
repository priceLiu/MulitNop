// add by hz - full page
using System;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Services.Events;

namespace Nop.Services.Common
{
    /// <summary>
    /// Address service
    /// </summary>
    public partial class BranchService : IBranchService
    {
        #region Fields

        private readonly IRepository<Branch> _branchRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="branchRepository">Branch repository</param>
        /// <param name="eventPublisher">Event published</param>
        public BranchService(IRepository<Branch> branchRepository,
            IEventPublisher eventPublisher)
        {
            _branchRepository = branchRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a Branch
        /// </summary>
        /// <param name="branch">Branch</param>
        public virtual void DeleteBranch(Branch branch)
        {
            if (branch == null)
                throw new ArgumentNullException("branch");

            _branchRepository.Delete(branch);

            //event notification
            _eventPublisher.EntityDeleted(branch);
        }

        /// <summary>
        /// Gets total number of  Branches by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Number of Branches</returns>
        public virtual int GetBranchTotalByCountryId(int countryId)
        {
            if (countryId == 0)
                return 0;

            var query = from a in _branchRepository.Table
                        where a.CountryId == countryId
                        select a;
            return query.Count();
        }

        /// <summary>
        /// Gets total number of Branches by state/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <returns>Number of Branches</returns>
        public virtual int GetBranchTotalByStateProvinceId(int stateProvinceId)
        {
            if (stateProvinceId == 0)
                return 0;

            var query = from a in _branchRepository.Table
                        where a.StateProvinceId == stateProvinceId
                        select a;
            return query.Count();
        }

        /// <summary>
        /// Gets a Branch by Branch identifier
        /// </summary>
        /// <param name="branchId">Branch identifier</param>
        /// <returns>Branch</returns>
        public virtual Branch GetBranchById(int branchId)
        {
            if (branchId == 0)
                return null;

            var branch = _branchRepository.GetById(branchId);
            return branch;
        }

        /// <summary>
        /// Inserts a branch
        /// </summary>
        /// <param name="branch">Branch</param>
        public virtual void InsertBranch(Branch branch)
        {
            if (branch == null)
                throw new ArgumentNullException("branch");

            branch.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (branch.CountryId == 0)
                branch.CountryId = null;
            if (branch.StateProvinceId == 0)
                branch.StateProvinceId = null;

            _branchRepository.Insert(branch);

            //event notification
            _eventPublisher.EntityInserted(branch);
        }

        /// <summary>
        /// Updates the Branch
        /// </summary>
        /// <param name="branch">Branch</param>
        public virtual void UpdateBranch(Branch branch)
        {
            if (branch == null)
                throw new ArgumentNullException("branch");

            //some validation
            if (branch.CountryId == 0)
                branch.CountryId = null;
            if (branch.StateProvinceId == 0)
                branch.StateProvinceId = null;

            _branchRepository.Update(branch);

            //event notification
            _eventPublisher.EntityUpdated(branch);
        }

        #endregion
    }
}