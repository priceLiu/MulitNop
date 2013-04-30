//add by hz full page
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tasks;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Services.Common;
using Nop.Services.Seo;
using Nop.Core.Domain.Seo;

namespace Nop.Services.Installation
{
    public partial class CodeFirstInstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<Vendor> _vendorRepository;//add by hz
        private readonly IRepository<VendorTemplate> _vendorTemplateRepository;//add by hz

        #endregion

        #region Ctor

        public CodeFirstInstallationService(IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<TaxCategory> taxCategoryRepository,
            IRepository<Language> languageRepository,
            IRepository<Currency> currencyRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<MessageTemplate> messageTemplateRepository,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Discount> discountRepository,
            IRepository<BlogPost> blogPostRepository,
            IRepository<Topic> topicRepository,
            IRepository<NewsItem> newsItemRepository,
            IRepository<Poll> pollRepository,
            IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductTemplate> productTemplateRepository,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IRepository<ScheduleTask> scheduleTaskRepository,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            //add by hz
            IRepository<Vendor> vendorRepository,
            IRepository<VendorTemplate> vendorTemplateRepository
            //end by hz
            )
        {
            this._measureDimensionRepository = measureDimensionRepository;
            this._measureWeightRepository = measureWeightRepository;
            this._taxCategoryRepository = taxCategoryRepository;
            this._languageRepository = languageRepository;
            this._currencyRepository = currencyRepository;
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._productAttributeRepository = productAttributeRepository;
            this._categoryRepository = categoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._emailAccountRepository = emailAccountRepository;
            this._messageTemplateRepository = messageTemplateRepository;
            this._forumGroupRepository = forumGroupRepository;
            this._forumRepository = forumRepository;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._discountRepository = discountRepository;
            this._blogPostRepository = blogPostRepository;
            this._topicRepository = topicRepository;
            this._newsItemRepository = newsItemRepository;
            this._pollRepository = pollRepository;
            this._shippingMethodRepository = shippingMethodRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._productTagRepository = productTagRepository;
            this._productTemplateRepository = productTemplateRepository;
            this._categoryTemplateRepository = categoryTemplateRepository;
            this._manufacturerTemplateRepository = manufacturerTemplateRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
            //add by hz
            this._vendorRepository = vendorRepository;//add by hz
            this._vendorTemplateRepository = vendorTemplateRepository;//add by hz     
            //end by hz
        }

        #endregion

        #region Utilities

        //add by hz
        protected virtual void InstallVendors()
        {
            var vendorTemplateInGridAndLines =
                _vendorTemplateRepository.Table.Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

            var allVendors = new List<Vendor>();
            var MainVendor = new Vendor
            {
                Name = "Main Vendor",
                VendorTemplateId = vendorTemplateInGridAndLines.Id,
                PageSize = 4,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "4, 2, 8, 12",
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            _vendorRepository.Insert(MainVendor);
            allVendors.Add(MainVendor);
            

            //search engine names
            foreach (var vendor in allVendors)
            {
                _urlRecordRepository.Insert(new UrlRecord()
                {
                    EntityId = vendor.Id,
                    EntityName = "Vendor",
                    LanguageId = 0,
                    Slug = vendor.ValidateSeName("", vendor.Name, true)
                });
            }
        }
        //end by hz


        protected virtual void InstallActivityLogTypesHzVendor()
        {
            var activityLogTypes = new List<ActivityLogType>()
                                      {
                                        //add by hz
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewVendor",
                                                  Enabled = true,
                                                  Name = "Add a new vendor"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteVendor",
                                                  Enabled = true,
                                                  Name = "Delete a vendor"
                                              },
                                        new ActivityLogType
                                              {
                                                  SystemKeyword = "EditVendor",
                                                  Enabled = true,
                                                  Name = "Edit a vendor"
                                              },
                                        new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicVendor.ViewVendor",
                                                  Enabled = false,
                                                  Name = "Public vendor. View a vendor"
                                              },
                                          //end by hz

                                      };
            activityLogTypes.ForEach(alt => _activityLogTypeRepository.Insert(alt));
        }


        protected virtual void InstallVendorTemplates()
        {
            var vendorTemplates = new List<VendorTemplate>
                               {
                                   new VendorTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "VendorTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            vendorTemplates.ForEach(st => _vendorTemplateRepository.Insert(st));

        }

        protected virtual void InstallVendorCustomersAndUsers()
        {
            var crVendorManagers = new CustomerRole
            {
                Name = "Vendor Managers",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.VendorManager,
            };
            
            _customerRoleRepository.Insert(crVendorManagers);

            //admin user
            var vendorManagerUser = new Customer()
            {
                CustomerGuid = Guid.NewGuid(),
                Email = "1@vendor.com",
                Username = "1@vendor.com",
                Password = "1234",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = "",
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            var defaultAdminUserAddress = new Address()
            {
                FirstName = "David",
                LastName = "Blend",
                PhoneNumber = "12345678",
                Email = "1@vendor.com",
                FaxNumber = "",
                Company = "hz",
                Address1 = "21 West 52nd Street",
                Address2 = "",
                City = "New York",
                StateProvince = _stateProvinceRepository.Table.Where(sp => sp.Name == "New York").FirstOrDefault(),
                Country = _countryRepository.Table.Where(c => c.ThreeLetterIsoCode == "USA").FirstOrDefault(),
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            vendorManagerUser.Addresses.Add(defaultAdminUserAddress);
            vendorManagerUser.BillingAddress = defaultAdminUserAddress;
            vendorManagerUser.ShippingAddress = defaultAdminUserAddress;
            vendorManagerUser.CustomerRoles.Add(crVendorManagers);
            vendorManagerUser.CustomerRoles.Add(GetCustomerRolByName());
            //adminUser.CustomerRoles.Add();
            _customerRepository.Insert(vendorManagerUser);
            //set default customer name
            _genericAttributeService.SaveAttribute(vendorManagerUser, SystemCustomerAttributeNames.FirstName, "David");
            _genericAttributeService.SaveAttribute(vendorManagerUser, SystemCustomerAttributeNames.LastName, "Blend");

            var mainVendor = _vendorRepository.Table.Where(s => s.Name == "Main Vendor").FirstOrDefault();
            mainVendor.VendorCustomers.Add(new VendorCustomer()
            {
                Customer = _customerRepository.Table.Where(c => c.Email == "1@vendor.com").Single(),
                DisplayOrder = 1,
            });
        }

        private CustomerRole GetCustomerRolByName()
        {
            var query = from cr in _customerRoleRepository.Table
                        orderby cr.Id
                        where cr.SystemName == "Registered"
                        select cr;
            var customerRole = query.FirstOrDefault();
            return customerRole;
        }

        #endregion

        #region Methods

        public virtual void InstallDataVendor(bool installSampleData = true)
        {
            InstallVendorTemplates();//add by hz
            InstallActivityLogTypesHzVendor(); //add by hz

            if (installSampleData)
            {
                InstallVendors();//add by hz
                InstallVendorCustomersAndUsers();
            }
        }

        #endregion
    }
}