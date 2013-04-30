//add by hz full page
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// VENDOR template service
    /// </summary>
    public partial class VendorTemplateService : IVendorTemplateService
    {
        #region Constants
        private const string VENDORTEMPLATES_BY_ID_KEY = "Nop.vendortemplate.id-{0}";
        private const string VENDORTEMPLATES_ALL_KEY = "Nop.vendortemplate.all";
        private const string VENDORTEMPLATES_PATTERN_KEY = "Nop.vendortemplate.";

        #endregion

        #region Fields

        private readonly IRepository<VendorTemplate> _vendorTemplateRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="vendorTemplateRepository">Vendor template repository</param>
        /// <param name="eventPublisher">Event published</param>
        public VendorTemplateService(ICacheManager cacheManager,
            IRepository<VendorTemplate> vendorTemplateRepository,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _vendorTemplateRepository = vendorTemplateRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete vendor template
        /// </summary>
        /// <param name="vendorTemplate">Vendor template</param>
        public virtual void DeleteVendorTemplate(VendorTemplate vendorTemplate)
        {
            if (vendorTemplate == null)
                throw new ArgumentNullException("vendorTemplate");

            _vendorTemplateRepository.Delete(vendorTemplate);

            _cacheManager.RemoveByPattern(VENDORTEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(vendorTemplate);
        }

        /// <summary>
        /// Gets all vendor templates
        /// </summary>
        /// <returns>Vendor templates</returns>
        public virtual IList<VendorTemplate> GetAllVendorTemplates()
        {
            string key = VENDORTEMPLATES_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from pt in _vendorTemplateRepository.Table
                            orderby pt.DisplayOrder
                            select pt;

                var templates = query.ToList();
                return templates;
            });
        }
 
        /// <summary>
        /// Gets a vendor template
        /// </summary>
        /// <param name="vendorTemplateId">Vendor template identifier</param>
        /// <returns>Vendor template</returns>
        public virtual VendorTemplate GetVendorTemplateById(int vendorTemplateId)
        {
            if (vendorTemplateId == 0)
                return null;

            string key = string.Format(VENDORTEMPLATES_BY_ID_KEY, vendorTemplateId);
            return _cacheManager.Get(key, () =>
            {
                var template = _vendorTemplateRepository.GetById(vendorTemplateId);
                return template;
            });
        }

        /// <summary>
        /// Inserts vendor template
        /// </summary>
        /// <param name="vendorTemplate">Vendor template</param>
        public virtual void InsertVendorTemplate(VendorTemplate vendorTemplate)
        {
            if (vendorTemplate == null)
                throw new ArgumentNullException("vendorTemplate");

            _vendorTemplateRepository.Insert(vendorTemplate);

            //cache
            _cacheManager.RemoveByPattern(VENDORTEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(vendorTemplate);
        }

        /// <summary>
        /// Updates the vendor template
        /// </summary>
        /// <param name="vendorTemplate">Vendor template</param>
        public virtual void UpdateVendorTemplate(VendorTemplate vendorTemplate)
        {
            if (vendorTemplate == null)
                throw new ArgumentNullException("vendorTemplate");

            _vendorTemplateRepository.Update(vendorTemplate);

            //cache
            _cacheManager.RemoveByPattern(VENDORTEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(vendorTemplate);
        }
        
        #endregion
    }
}
