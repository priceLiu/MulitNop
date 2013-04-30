//add by hz full page
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Services.Events;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Vendor service
    /// </summary>
    public partial class VendorService : IVendorService
    {
        #region Constants
        private const string VENDORS_BY_ID_KEY = "Nop.vendor.id-{0}";
        private const string PRODUCTVENDORS_ALLBYVENDORID_KEY = "Nop.productvendor.allbyvendorid-{0}-{1}-{2}-{3}-{4}";
        private const string PRODUCTVENDORS_ALLBYPRODUCTID_KEY = "Nop.productvendor.allbyproductid-{0}-{1}-{2}";
        private const string PRODUCTVENDORS_BY_ID_KEY = "Nop.productvendor.id-{0}";
        private const string VENDORS_PATTERN_KEY = "Nop.vendor.";
        private const string PRODUCTVENDORS_PATTERN_KEY = "Nop.productvendor.";
        private const string CUSTOMERVENDORS_PATTERN_KEY = "Nop.customervendor.";
        private const string CUSTOMERVENDORS_ALLBYVENDORID_KEY = "Nop.customervendor.allbyvendorid-{0}-{1}-{2}-{3}";
        private const string CUSTOMERVENDORS_ALLBYCUSTOMERID_KEY = "Nop.productvendor.allbyproductid-{0}-{1}";
        private const string CUSTOMERVENDORS_BY_ID_KEY = "Nop.customervendor.id-{0}";
        #endregion

        #region Fields

        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<ProductVendor> _productVendorRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IWorkContext _workContext; 
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<VendorCustomer> _vendorCustomerRepository;
        private readonly IRepository<Customer> _customerRepository;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="vendorRepository">Category repository</param>
        /// <param name="productVendorRepository">ProductCategory repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="eventPublisher">Event published</param>
        public VendorService(ICacheManager cacheManager,
            IRepository<Vendor> vendorRepository,
            IRepository<ProductVendor> productVendorRepository,
            IRepository<Product> productRepository,
            IRepository<AclRecord> aclRepository,
            IWorkContext workContext,
            IEventPublisher eventPublisher,
            IRepository<VendorCustomer> vendorCustomerRepository,
            IRepository<Customer> customerRepository)
        {
            this._cacheManager = cacheManager;
            this._vendorRepository = vendorRepository;
            this._productVendorRepository = productVendorRepository;
            this._productRepository = productRepository;
            this._aclRepository = aclRepository;
            this._workContext = workContext;
            this._eventPublisher = eventPublisher;
            this._vendorCustomerRepository = vendorCustomerRepository;
            this._customerRepository = customerRepository;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Deletes a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void DeleteVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");
            
            vendor.Deleted = true;
            UpdateVendor(vendor);
        }

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendor collection</returns>
        public virtual IList<Vendor> GetAllVendors(int customerVendorId = 0,bool showHidden = false)
        {
            return GetAllVendors(null,customerVendorId, showHidden);
        }

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendor collection</returns>
        public virtual IList<Vendor> GetAllVendors(string vendorName,int customerVendorId=0, bool showHidden = false)
        {
            var query = _vendorRepository.Table;
            if (!showHidden)
                query = query.Where(s => s.Published);
            if (!String.IsNullOrWhiteSpace(vendorName))
                query = query.Where(s => s.Name.Contains(vendorName));
            if (customerVendorId > 0)
                query = query.Where(s => s.Id == customerVendorId);
            query = query.Where(s => !s.Deleted);
            query = query.OrderBy(s => s.DisplayOrder);

            //ACL (access control list)
            if (!showHidden)
            {
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                    .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

                query = from s in query
                        join acl in _aclRepository.Table on s.Id equals acl.EntityId into s_acl
                        from acl in s_acl.DefaultIfEmpty()
                        where !s.SubjectToAcl || (acl.EntityName == "Vendor" && allowedCustomerRolesIds.Contains(acl.CustomerRoleId))
                        select s;

                //only distinct vendors (group by ID)
                query = from s in query
                        group s by s.Id
                            into sGroup
                            orderby sGroup.Key
                            select sGroup.FirstOrDefault();
                query = query.OrderBy(s => s.DisplayOrder);
            }

            var vendors = query.ToList();
            return vendors;
        }
        
        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        public virtual IPagedList<Vendor> GetAllVendors(string vendorName,
            int pageIndex, int pageSize,int customerVendorId, bool showHidden = false)
        {
            var vendors = GetAllVendors(vendorName,customerVendorId, showHidden);
            return new PagedList<Vendor>(vendors, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a vendor
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        public virtual Vendor GetVendorById(int vendorId)
        {
            if (vendorId == 0)
                return null;

            string key = string.Format(VENDORS_BY_ID_KEY, vendorId);
            return _cacheManager.Get(key, () =>
            {
                var vendor = _vendorRepository.GetById(vendorId);
                return vendor;
            });
        }

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void InsertVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            _vendorRepository.Insert(vendor);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(vendor);
        }

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual void UpdateVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            _vendorRepository.Update(vendor);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(vendor);
        }

        /// <summary>
        /// Deletes a product vendor mapping
        /// </summary>
        /// <param name="productVendor">Product vendor mapping</param>
        public virtual void DeleteProductVendor(ProductVendor productVendor)
        {
            if (productVendor == null)
                throw new ArgumentNullException("productVendor");

            _productVendorRepository.Delete(productVendor);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productVendor);
        }

        /// <summary>
        /// Gets product vendor collection
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product vendor collection</returns>
        public virtual IPagedList<ProductVendor> GetProductVendorsByVendorId(int vendorId, 
            int pageIndex, int pageSize, bool showHidden = false)
        {
            if (vendorId == 0)
                return new PagedList<ProductVendor>(new List<ProductVendor>(), pageIndex, pageSize);

            string key = string.Format(PRODUCTVENDORS_ALLBYVENDORID_KEY, showHidden, vendorId, pageIndex, pageSize, _workContext.CurrentCustomer.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from ps in _productVendorRepository.Table
                            join p in _productRepository.Table on ps.ProductId equals p.Id
                            where ps.VendorId == vendorId &&
                                  !p.Deleted &&
                                  (showHidden || p.Published)
                            orderby ps.DisplayOrder
                            select ps;

                //ACL (access control list)
                if (!showHidden)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                        .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

                    query = from ps in query
                            join s in _vendorRepository.Table on ps.VendorId equals s.Id
                            join acl in _aclRepository.Table on s.Id equals acl.EntityId into s_acl
                            from acl in s_acl.DefaultIfEmpty()
                            where
                                !s.SubjectToAcl ||
                                (acl.EntityName == "Vendor" && allowedCustomerRolesIds.Contains(acl.CustomerRoleId))
                            select ps;

                    //only distinct vendors (group by ID)
                    query = from ps in query
                            group ps by ps.Id
                                into psGroup
                                orderby psGroup.Key
                                select psGroup.FirstOrDefault();
                    query = query.OrderBy(ps => ps.DisplayOrder);
                }


                var productVendors = new PagedList<ProductVendor>(query, pageIndex, pageSize);
                return productVendors;
            });
        }

        /// <summary>
        /// Gets a product vendor mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product vendor mapping collection</returns>
        public virtual IList<ProductVendor> GetProductVendorsByProductId(int productId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductVendor>();

            string key = string.Format(PRODUCTVENDORS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from ps in _productVendorRepository.Table
                            join s in _vendorRepository.Table on ps.VendorId equals s.Id
                            where ps.ProductId == productId &&
                                !s.Deleted &&
                                (showHidden || s.Published)
                            orderby ps.DisplayOrder
                            select ps;
                
                //ACL (access control list)
                if (!showHidden)
                {
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                        .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

                    query = from ps in query
                            join s in _vendorRepository.Table on ps.VendorId equals s.Id
                            join acl in _aclRepository.Table on s.Id equals acl.EntityId into s_acl
                            from acl in s_acl.DefaultIfEmpty()
                            where !s.SubjectToAcl || (acl.EntityName == "Vendor" && allowedCustomerRolesIds.Contains(acl.CustomerRoleId))
                            select ps;

                    //only distinct vendors (group by ID)
                    query = from ps in query
                            group ps by ps.Id
                                into sGroup
                                orderby sGroup.Key
                                select sGroup.FirstOrDefault();
                    query = query.OrderBy(ps => ps.DisplayOrder);
                }      

                var productVendors = query.ToList();
                return productVendors;
            });
        }

        /// <summary>
        /// Gets a  vendor id 
        /// </summary>
        /// <param name="productId">product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>vendor id</returns>
        public virtual int GetVendorIdByProductId(int productId, bool showHidden = false)
        {
            if (productId == 0)
                return 0;

            var query = from ps in _productVendorRepository.Table
                        join s in _vendorRepository.Table on
                            ps.VendorId equals s.Id
                        where ps.ProductId == productId &&
                              !s.Deleted &&
                              (showHidden || s.Published)
                        select ps.VendorId;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Get a total number of featured products by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Number of featured products</returns>
        public virtual int GetTotalNumberOfFeaturedProducts(int vendorId)
        {
            if (vendorId == 0)
                return 0;

            var query = from pm in _productVendorRepository.Table
                        where pm.VendorId == vendorId &&
                              pm.IsFeaturedProduct
                        select pm;
            var result = query.Count();
            return result;
        }
        /// <summary>
        /// Gets a product vendor mapping 
        /// </summary>
        /// <param name="productVendorId">Product vendor mapping identifier</param>
        /// <returns>Product vendor mapping</returns>
        public virtual ProductVendor GetProductVendorById(int productVendorId)
        {
            if (productVendorId == 0)
                return null;

            string key = string.Format(PRODUCTVENDORS_BY_ID_KEY, productVendorId);
            return _cacheManager.Get(key, () =>
            {
                return _productVendorRepository.GetById(productVendorId);
            });
        }

        /// <summary>
        /// Inserts a product vendor mapping
        /// </summary>
        /// <param name="productVendor">Product vendor mapping</param>
        public virtual void InsertProductVendor(ProductVendor productVendor)
        {
            if (productVendor == null)
                throw new ArgumentNullException("productVendor");

            _productVendorRepository.Insert(productVendor);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productVendor);
        }

        /// <summary>
        /// Updates the product vendor mapping
        /// </summary>
        /// <param name="productVendor">Product vendor mapping</param>
        public virtual void UpdateProductVendor(ProductVendor productVendor)
        {
            if (productVendor == null)
                throw new ArgumentNullException("productVendor");

            _productVendorRepository.Update(productVendor);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productVendor);
        }

        //customers

        /// <summary>
        /// Gets vendor customers collection
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product vendor collection</returns>
        public IPagedList<VendorCustomer> GetVendorCustomersByVendorId(int vendorId,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            if (vendorId == 0)
                return new PagedList<VendorCustomer>(new List<VendorCustomer>(), pageIndex, pageSize);

            string key = string.Format(CUSTOMERVENDORS_ALLBYVENDORID_KEY, showHidden, vendorId, pageIndex, pageSize); 
            return _cacheManager.Get(key, () =>
            {
                var query = from sc in _vendorCustomerRepository.Table
                            join c in _customerRepository.Table on sc.CustomerId equals c.Id
                            where sc.VendorId == vendorId &&
                                  !c.Deleted &&
                                  (showHidden || c.Active)
                            orderby sc.DisplayOrder
                            select sc;
                var vendorCustomers = new PagedList<VendorCustomer>(query, pageIndex, pageSize);
                return vendorCustomers;
            });
        }

        /// <summary>
        /// Gets a  vendor customer mapping collection
        /// </summary>
        /// <param name="customerId">customer identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>vendor customer mapping collection</returns>
        public virtual IList<VendorCustomer> GetVendorsCustomerByCustomerId(int customerId, bool showHidden = false)
        {
            if (customerId == 0)
                return new List<VendorCustomer>();

            string key = string.Format(CUSTOMERVENDORS_ALLBYCUSTOMERID_KEY, showHidden, customerId);
            return _cacheManager.Get(key, () =>
            {
                var query = from sc in _vendorCustomerRepository.Table
                            join s in _vendorRepository.Table on
                                sc.VendorId equals s.Id
                            where sc.CustomerId == customerId &&
                                  !s.Deleted &&
                                  (showHidden || s.Published)
                            orderby sc.DisplayOrder
                            select sc;
                var vendorCustomers = query.ToList();
                return vendorCustomers;
            });
        }

        /// <summary>
        /// Gets a  vendor id 
        /// </summary>
        /// <param name="customerId">customer identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>vendor id</returns>
        public virtual int GetVendorIdByCustomerId(int customerId, bool showHidden = false)
        {
            if (customerId == 0)
                return 0;

            var query = from sc in _vendorCustomerRepository.Table
                        join s in _vendorRepository.Table on
                            sc.VendorId equals s.Id
                        where sc.CustomerId == customerId &&
                              !s.Deleted &&
                              (showHidden || s.Published)
                        select sc.VendorId;
            return query.FirstOrDefault();
        }


        /// <summary>
        /// Gets a customer vendor mapping 
        /// </summary>
        /// <param name="vendorCustomerId">Vendor Customer mapping identifier</param>
        /// <returns>Vendor Customer mapping</returns>
        public VendorCustomer GetVendorCustomerById(int vendorCustomerId)
        {
            if (vendorCustomerId == 0)
                return null;

            string key = string.Format(CUSTOMERVENDORS_BY_ID_KEY, vendorCustomerId);
            return _cacheManager.Get(key, () =>
            {
                return _vendorCustomerRepository.GetById(vendorCustomerId);
            });
        }

        /// <summary>
        /// Updates the vendor customer mapping
        /// </summary>
        /// <param name="vendorCustomer">Vendor Customer mapping</param>
        public void UpdateVendorCustomer(VendorCustomer vendorCustomer)
        {
            if (vendorCustomer == null)
                throw new ArgumentNullException("vendorCustomer");

            _vendorCustomerRepository.Update(vendorCustomer);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(vendorCustomer);
        }

        /// <summary>
        /// Deletes a vendor customer mapping
        /// </summary>
        /// <param name="vendorCustomer">vendor customer mapping</param>
        public void DeleteVendorCustomer(VendorCustomer vendorCustomer)
        {
            if (vendorCustomer == null)
                throw new ArgumentNullException("vendorCustomer");

            _vendorCustomerRepository.Delete(vendorCustomer);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(vendorCustomer);
        }

        /// <summary>
        /// Inserts a customer vendor mapping
        /// </summary>
        /// <param name="vendorCustomer">vendor customer mapping</param>
        public void InsertVendorCustomer(VendorCustomer vendorCustomer)
        {
            if (vendorCustomer == null)
                throw new ArgumentNullException("vendorCustomer");

            _vendorCustomerRepository.Insert(vendorCustomer);

            //cache
            _cacheManager.RemoveByPattern(VENDORS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CUSTOMERVENDORS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(vendorCustomer);
        }


        //checkout

        /// <summary>
        /// Get vendor shoping cart items 
        /// </summary>
        /// <param name="shopingcartItems">Shoping cart items</param>
        /// <returns>Vendor Order</returns>
        public virtual IList<VendorOrder> getVendorSoppingCartItems(IList<ShoppingCartItem> shoppingCartItems)
        {
            var vendorOrders = new List<VendorOrder>();

            foreach (var item in shoppingCartItems)
            {
                var query = from ps in _productVendorRepository.Table
                            where ps.ProductId  == item.ProductVariant.Product.Id
                            select ps.Vendor;
                var vendor = query.FirstOrDefault(); 

                int index = vendorOrders.FindIndex(s => s.vendor.Id == vendor.Id);
                if (index >= 0)
                {
                    vendorOrders[index].ShoppingCartItems.Add(item);
                }
                else
                {
                    var newVendorOrder = new VendorOrder();
                    newVendorOrder.vendor = vendor;
                    newVendorOrder.ShoppingCartItems.Add(item);
                    vendorOrders.Add(newVendorOrder);
                }
            }
            return vendorOrders;
        }
        #endregion
    }
}
