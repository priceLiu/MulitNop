//add by hz full page
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Common;
using Nop.Admin.Models.Customers;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public partial class VendorController : BaseNopController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IVendorService _vendorService;
        private readonly IVendorTemplateService _vendorTemplateService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IExportManager _exportManager;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAclService _aclService; 
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;

        private readonly IBranchService _branchService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        private readonly int customerVendorId;//add by hz
        #endregion
        
        #region Constructors

        public VendorController(ICategoryService categoryService, IVendorService vendorService,
            IVendorTemplateService vendorTemplateService, IProductService productService,
            IUrlRecordService urlRecordService, IPictureService pictureService,
            ILanguageService languageService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
            IExportManager exportManager, IWorkContext workContext,
            ICustomerActivityService customerActivityService, IAclService aclService, IPermissionService permissionService,
            AdminAreaSettings adminAreaSettings, CatalogSettings catalogSettings,
            ICustomerService customerService, CustomerSettings customerSettings
            , IBranchService branchService
            , ICountryService countryService, IStateProvinceService stateProvinceService)
        {
            this._categoryService = categoryService;
            this._vendorTemplateService = vendorTemplateService;
            this._vendorService = vendorService;
            this._productService = productService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._exportManager = exportManager;
            this._workContext = workContext;
            this._customerActivityService = customerActivityService;
            this._aclService = aclService;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
            this._catalogSettings = catalogSettings;
            this._customerService = customerService;
            this._customerSettings = customerSettings;

            this._branchService = branchService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;

            customerVendorId = _vendorService.GetVendorIdByCustomerId(_workContext.CurrentCustomer.Id);//add by hz
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected void UpdateLocales(Vendor vendor, VendorModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(vendor,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(vendor,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);

                //search engine name
                var seName = vendor.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(vendor, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected void UpdatePictureSeoNames(Vendor vendor)
        {
            var picture = _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));
        }

        [NonAction]
        protected void PrepareTemplatesModel(VendorModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _vendorTemplateService.GetAllVendorTemplates();
            foreach (var template in templates)
            {
                model.AvailableVendorTemplates.Add(new SelectListItem()
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        [NonAction]
        protected CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel()
            {
                Id = customer.Id,
                Email = !String.IsNullOrEmpty(customer.Email) ? customer.Email : (customer.IsGuest() ? _localizationService.GetResource("Admin.Customers.Guest") : "Unknown"),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                //CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoles.ToList()),
                Active = customer.Active,
                //CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                //LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        [NonAction]
        private void PrepareAclModel(VendorModel model, Vendor vendor, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (vendor != null)
                {
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(vendor);
                }
                else
                {
                    model.SelectedCustomerRoleIds = new int[0];
                }
            }
        }

        [NonAction]
        protected void SaveVendorAcl(Vendor vendor, VendorModel model)
        {
            var existingAclRecords = _aclService.GetAclRecords(vendor);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Where(acl => acl.CustomerRoleId == customerRole.Id).Count() == 0)
                        _aclService.InsertAclRecord(vendor, customerRole.Id);
                }
                else
                {
                    //removed role
                    var aclRecordToDelete = existingAclRecords.Where(acl => acl.CustomerRoleId == customerRole.Id).FirstOrDefault();
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

        [NonAction]
        public void UpdateLocalesBranch(Branch branch, BranchModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(branch,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(branch,
                                                           x => x.City,
                                                           localized.City,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(branch,
                                                           x => x.Address1,
                                                           localized.Address1,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(branch,
                                                           x => x.Address2,
                                                           localized.Address2,
                                                           localized.LanguageId);
            }
        }

        #endregion
        
        #region List

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();
            var model = new VendorListModel();
            model.IsVendorManager = (customerVendorId > 0) ? true : false;
            var vendors = _vendorService.GetAllVendors(null, 0, _adminAreaSettings.GridPageSize, customerVendorId, true );
            model.Vendors = new GridModel<VendorModel>
            {
                Data = vendors.Select(x => x.ToModel()),
                Total = vendors.TotalCount
            };
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, VendorListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var vendors = _vendorService.GetAllVendors(model.SearchVendorName,
                command.Page - 1, command.PageSize, customerVendorId, true);
            var gridModel = new GridModel<VendorModel>
            {
                Data = vendors.Select(x => x.ToModel()),
                Total = vendors.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();
            if(_vendorService.GetVendorIdByCustomerId(_workContext.CurrentCustomer.Id) > 0)
                return AccessDeniedView();

            var model = new VendorModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //templates
            PrepareTemplatesModel(model);
            //ACL
            PrepareAclModel(model, null, false);
            //default values
            model.PageSize = 4;
            model.Published = true;

            model.AllowCustomersToSelectPageSize = true;
            model.PageSizeOptions = _catalogSettings.DefaultManufacturerPageSizeOptions;//add by hz to do
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(VendorModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();
            if (customerVendorId > 0)
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var vendor = model.ToEntity();
                vendor.CreatedOnUtc = DateTime.UtcNow;
                vendor.UpdatedOnUtc = DateTime.UtcNow;
                _vendorService.InsertVendor(vendor);
                //search engine name
                model.SeName = vendor.ValidateSeName(model.SeName, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, model.SeName, 0);
                //locales
                UpdateLocales(vendor, model);
                //update picture seo file name
                UpdatePictureSeoNames(vendor);
                //ACL (customer roles)
                SaveVendorAcl(vendor, model);
                //activity log
                _customerActivityService.InsertActivity("AddNewVendor", _localizationService.GetResource("ActivityLog.AddNewVendor"), vendor.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Vendors.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = vendor.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //ACL
            PrepareAclModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();
        
            if(customerVendorId != id && customerVendorId > 0)
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");

            var model = vendor.ToModel();
            model.IsVendorManager = (customerVendorId > 0) ? true : false;
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = vendor.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = vendor.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = vendor.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = vendor.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = vendor.GetSeName(languageId, false, false);
            });
            //templates
            PrepareTemplatesModel(model);
            //ACL
            PrepareAclModel(model, vendor, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(VendorModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(model.Id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                int prevPictureId = vendor.PictureId;
                vendor = model.ToEntity(vendor);
                vendor.UpdatedOnUtc = DateTime.UtcNow;
                _vendorService.UpdateVendor(vendor);
                //search engine name
                model.SeName = vendor.ValidateSeName(model.SeName, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, model.SeName, 0);
                //locales
                UpdateLocales(vendor, model);
                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != vendor.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(vendor);
                //acl
                SaveVendorAcl(vendor, model);

                //activity log
                _customerActivityService.InsertActivity("EditVendor", _localizationService.GetResource("ActivityLog.EditVendor"), vendor.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Vendors.Updated"));
                return continueEditing ? RedirectToAction("Edit", vendor.Id) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
            //Acl
            PrepareAclModel(model, vendor, true);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null)
                //No vendor found with the specified id
                return RedirectToAction("List");

            _vendorService.DeleteVendor(vendor);

            //activity log
            _customerActivityService.InsertActivity("DeleteVendor", _localizationService.GetResource("ActivityLog.DeleteVendor"), vendor.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Vendors.Deleted"));
            return RedirectToAction("List");
        }
        
        #endregion

        #region Export / Import

        public ActionResult ExportXml()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            try
            {
                var vendors = _vendorService.GetAllVendors(0,true);
                var xml = _exportManager.ExportVendorsToXml(vendors);
                return new XmlDownloadResult(xml, "vendors.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command, int vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVendors = _vendorService.GetProductVendorsByVendorId(vendorId,
                command.Page - 1, command.PageSize, true);

            var model = new GridModel<VendorModel.VendorProductModel>
            {
                Data = productVendors
                .Select(x =>
                {
                    return new VendorModel.VendorProductModel()
                    {
                        Id = x.Id,
                        VendorId = x.VendorId,
                        ProductId = x.ProductId,
                        ProductName = _productService.GetProductById(x.ProductId).Name,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder1 = x.DisplayOrder
                    };
                }),
                Total = productVendors.TotalCount
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductUpdate(GridCommand command, VendorModel.VendorProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVendor = _vendorService.GetProductVendorById(model.Id);
            if (productVendor == null)
                throw new ArgumentException("No product vendor mapping found with the specified id");

            productVendor.IsFeaturedProduct = model.IsFeaturedProduct;
            productVendor.DisplayOrder = model.DisplayOrder1;
            _vendorService.UpdateProductVendor(productVendor);

            return ProductList(command, productVendor.VendorId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVendor = _vendorService.GetProductVendorById(id);
            if (productVendor == null)
                throw new ArgumentException("No product vendor mapping found with the specified id");

            var vendorId = productVendor.VendorId;
            _vendorService.DeleteProductVendor(productVendor);

            return ProductList(command, vendorId);
        }

        public ActionResult ProductAddPopup(int vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(0, 0,
                customerVendorId, //add by hz
                null, null, null, 0, string.Empty, false, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, _adminAreaSettings.GridPageSize,
                false, out filterableSpecificationAttributeOptionIds, true);

            var model = new VendorModel.AddVendorProductModel();
            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(showHidden: true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _vendorService.GetAllVendors(0,true))
                model.AvailableVendors.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAddPopupList(GridCommand command, VendorModel.AddVendorProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var gridModel = new GridModel();
            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(model.SearchCategoryId,
                model.SearchVendorId,
                customerVendorId,//add by hz
                null, null, null, 0, model.SearchProductName, false, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, command.Page - 1, command.PageSize,
                false, out filterableSpecificationAttributeOptionIds, true);
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductAddPopup(string btnId, string formId, VendorModel.AddVendorProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingProductvendors = _vendorService.GetProductVendorsByVendorId(model.VendorId, 0, int.MaxValue, true);
                        if (existingProductvendors.FindProductVendor(id, model.VendorId) == null)
                        {
                            _vendorService.InsertProductVendor(
                                new ProductVendor()
                                {
                                    VendorId = model.VendorId,
                                    ProductId = id,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            model.Products = new GridModel<ProductModel>();
            return View(model);
        }

        #endregion

        #region Customers

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CustomerList(GridCommand command, int vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var vendorCustomers = _vendorService.GetVendorCustomersByVendorId(vendorId,
                command.Page - 1, command.PageSize, true);

            var model = new GridModel<VendorModel.VendorCustomerModel>
            {
                Data = vendorCustomers
                .Select(x =>
                {
                    return new VendorModel.VendorCustomerModel()
                    {
                        Id = x.Id,
                        VendorId = x.VendorId,
                         CustomerId = x.CustomerId,
                        CustomerName = x.Customer.Email, 
                        DisplayOrder1 = x.DisplayOrder
                    };
                }),
                Total = vendorCustomers.TotalCount
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CustomerUpdate(GridCommand command, VendorModel.VendorCustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var vendorCustomer = _vendorService.GetVendorCustomerById(model.Id);
            if (vendorCustomer == null)
                throw new ArgumentException("No customer vendor mapping found with the specified id");

            vendorCustomer.DisplayOrder = model.DisplayOrder1;
            _vendorService.UpdateVendorCustomer(vendorCustomer);

            return CustomerList(command, vendorCustomer.VendorId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CustomerDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var vendorCustomer = _vendorService.GetVendorCustomerById(id);
            if (vendorCustomer == null)
                throw new ArgumentException("No vendor Customer mapping found with the specified id");

            var vendorId = vendorCustomer.VendorId;
            _vendorService.DeleteVendorCustomer(vendorCustomer);

            return CustomerList(command, vendorId);
        }

        public ActionResult CustomerAddPopup(int vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            //load registered customers by default
            var customerRoleVendorMangerId = new[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.VendorManager).Id };
            var listModel = new VendorModel.AddVendorCustomerModel()
            {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
            };
            var customers = _customerService.GetAllCustomers(null, null, customerRoleVendorMangerId, null,
                null, null, null, 0, 0, null, null, null,
                false, null, 0, _adminAreaSettings.GridPageSize);
            //customer list
            listModel.Customers = new GridModel<CustomerModel>
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };
            return View(listModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CustomerAddPopupList(GridCommand command, VendorModel.AddVendorCustomerModel model,
           [ModelBinderAttribute(typeof(CommaSeparatedModelBinder))] int[] searchCustomerRoleIds)
        {
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            var customerRoleVendorMangerId = new[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.VendorManager).Id };
            var customers = _customerService.GetAllCustomers(null, null,
                customerRoleVendorMangerId, model.SearchEmail, model.SearchUsername,
                model.SearchFirstName, model.SearchLastName,
                0, 0,
                null, null, null,
                false, null, command.Page - 1, command.PageSize);
            var gridModel = new GridModel<CustomerModel>
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult CustomerAddPopup(string btnId, string formId, VendorModel.AddVendorCustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (model.SelectedCustomerIds != null)
            {
                foreach (int id in model.SelectedCustomerIds)
                {
                    var customer = _customerService.GetCustomerById(id);
                    if (customer != null)
                    {
                        var existingVendorCustomers = _vendorService.GetVendorCustomersByVendorId(model.VendorId, 0, int.MaxValue, true);
                        if (existingVendorCustomers.FindVendorCustomer(id, model.VendorId) == null)
                        {
                            _vendorService.InsertVendorCustomer(
                                new VendorCustomer()
                                {
                                    VendorId = model.VendorId,
                                    CustomerId = id,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            model.Customers = new GridModel<CustomerModel>();
            return View(model);
        }

        #endregion

        #region Braches

        [GridAction]
        public ActionResult BranchesSelect(int vendorId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor)//add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id", "vendorId");

            var branches = vendor.Branches.OrderByDescending(b => b.CreatedOnUtc).ThenByDescending(b => b.Id).ToList();

            var gridModel = new GridModel<BranchModel>
            {
                Data = branches.Select(x =>
                {
                    var model = x.ToModel();
                    if (x.Country != null)
                        model.CountryName = x.Country.Name;
                    if (x.StateProvince != null)
                        model.StateProvinceName = x.StateProvince.Name;
                    return model;
                }),
                Total = branches.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction]
        public ActionResult BranchDelete(int vendorId, int branchId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor)//add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id", "vendorId");


            var branch = vendor.Branches.Where(b => b.Id == branchId).FirstOrDefault();
            vendor.RemoveBranch(branch);
            _vendorService.UpdateVendor(vendor);
            //now delete the address record

            _branchService.DeleteBranch(branch);

            return BranchesSelect(vendorId, command);
        }

        public ActionResult BranchCreate(int vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor)//add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id", "vendorId");

            var model = new VendorBranchModel();
            model.Branch = new BranchModel();
            //locales
            AddLocales(_languageService, model.Branch.Locales);
            model.VendorId = vendorId;
            //countries
            model.Branch.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.Branch.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.Branch.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult BranchCreate(VendorBranchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor)//add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(model.VendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            if (ModelState.IsValid)
            {
                var branch = model.Branch.ToEntity();
                branch.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (branch.CountryId == 0)
                    branch.CountryId = null;
                if (branch.StateProvinceId == 0)
                    branch.StateProvinceId = null;
                vendor.AddBranch(branch);
                _vendorService.UpdateVendor(vendor);

                //locales
                UpdateLocalesBranch(branch, model.Branch);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Vendors.Branches.Added"));
                return RedirectToAction("BranchEdit", new { branchId = branch.Id, vendorId = model.VendorId });
            }

            //If we got this far, something failed, redisplay form
            model.VendorId = vendor.Id;
            //countries
            model.Branch.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.Branch.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Branch.CountryId) });
            //states
            var states = model.Branch.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Branch.CountryId.Value, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Branch.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Branch.StateProvinceId) });
            }
            else
                model.Branch.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            return View(model);
        }

        public ActionResult BranchEdit(int branchId, int vendorId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor)//add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var branch = _branchService.GetBranchById(branchId);
            if (branch == null)
                throw new ArgumentException("No branch found with the specified id", "branchId");

            var model = new VendorBranchModel();

            model.VendorId = vendorId;
            model.Branch = branch.ToModel();
            //locales
            AddLocales(_languageService, model.Branch.Locales, (locale, languageId) =>
            {
                locale.Name = branch.GetLocalized(x => x.Name, languageId, false, false);
                locale.City = branch.GetLocalized(x => x.City, languageId, false, false);
                locale.Address1 = branch.GetLocalized(x => x.Address1, languageId, false, false);
                locale.Address2 = branch.GetLocalized(x => x.Address2, languageId, false, false);
            });
            //countries
            model.Branch.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.Branch.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == branch.CountryId) });
            //states
            var states = model.Branch.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Branch.CountryId.Value, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Branch.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == branch.StateProvinceId) });
            }
            else
                model.Branch.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult BranchEdit(VendorBranchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor)//add by hz
                )
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(model.VendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var branch = _branchService.GetBranchById(model.Branch.Id);
            if (branch == null)
                throw new ArgumentException("No branch found with the specified id");

            if (ModelState.IsValid)
            {
                branch = model.Branch.ToEntity(branch);
                _branchService.UpdateBranch(branch);

                //locales
                UpdateLocalesBranch(branch, model.Branch);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Vendors.Branches.Updated"));
                return RedirectToAction("BranchEdit", new { branchId = model.Branch.Id, vendorId = model.VendorId });
            }

            //If we got this far, something failed, redisplay form
            model.VendorId = vendor.Id;
            model.Branch = branch.ToModel();
            //countries
            model.Branch.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.Branch.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == branch.CountryId) });
            //states
            var states = branch.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(branch.Country.Id, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Branch.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == branch.StateProvinceId) });
            }
            else
                model.Branch.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        #endregion
    }
}
