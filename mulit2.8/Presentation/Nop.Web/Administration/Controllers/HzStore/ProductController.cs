﻿//add by hz page
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;
using Nop.Services.Seo;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ProductController : BaseNopController
    {
		#region Fields

        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly ICopyProductService _copyProductService;
        private readonly IPdfService _pdfService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IVendorService _vendorService;//add by hz
        private readonly int customerVendorId;//add by hz
        #endregion

		#region Constructors

        public ProductController(IProductService productService, 
            IProductTemplateService productTemplateService,
            ICategoryService categoryService, IManufacturerService manufacturerService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService, IWorkContext workContext, ILanguageService languageService, 
            ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
            ISpecificationAttributeService specificationAttributeService, IPictureService pictureService,
            ITaxCategoryService taxCategoryService, IProductTagService productTagService,
            ICopyProductService copyProductService, IPdfService pdfService,
            IExportManager exportManager, IImportManager importManager,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService, IAclService aclService,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            IMeasureService measureService, MeasureSettings measureSettings,
            PdfSettings pdfSettings, AdminAreaSettings adminAreaSettings
            ,IVendorService vendorService//add by hz
            )
        {
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._specificationAttributeService = specificationAttributeService;
            this._pictureService = pictureService;
            this._taxCategoryService = taxCategoryService;
            this._productTagService = productTagService;
            this._copyProductService = copyProductService;
            this._pdfService = pdfService;
            this._exportManager = exportManager;
            this._importManager = importManager;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._pdfSettings = pdfSettings;
            this._adminAreaSettings = adminAreaSettings;
            this._vendorService = vendorService;//add by hz
            customerVendorId = _vendorService.GetVendorIdByCustomerId(_workContext.CurrentCustomer.Id);//add by hz
        }

        #endregion 

        #region Utitilies

        [NonAction]
        private void UpdateLocales(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.ShortDescription,
                                                               localized.ShortDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.FullDescription,
                                                               localized.FullDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaKeywords,
                                                               localized.MetaKeywords,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaDescription,
                                                               localized.MetaDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaTitle,
                                                               localized.MetaTitle,
                                                               localized.LanguageId);

                //search engine name
                var seName = product.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(product, seName, localized.LanguageId);
            }
        }

        [NonAction]
        private void UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in product.ProductPictures)
                _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }

        [NonAction]
        protected void UpdateProductTagTotals(Product product)
        {
            var productTags = product.ProductTags;
            foreach (var productTag in productTags)
                _productTagService.UpdateProductTagTotals(productTag);
        }
        
        [NonAction]
        private void PrepareTemplatesModel(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _productTemplateService.GetAllProductTemplates();
            foreach (var template in templates)
            {
                model.AvailableProductTemplates.Add(new SelectListItem()
                {
                    Text =  template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        [NonAction]
        private void PrepareAddSpecificationAttributeModel(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.AddSpecificationAttributeModel == null)
                model.AddSpecificationAttributeModel = new ProductModel.AddProductSpecificationAttributeModel();
            
            //attributes
            var specificationAttributes = _specificationAttributeService.GetSpecificationAttributes();
            for (int i = 0; i < specificationAttributes.Count; i++)
            {
                var sa = specificationAttributes[i];
                model.AddSpecificationAttributeModel.AvailableAttributes.Add(new SelectListItem() { Text = sa.Name, Value = sa.Id.ToString() });
                if (i == 0)
                {
                    //attribute options
                    foreach (var sao in _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(sa.Id))
                        model.AddSpecificationAttributeModel.AvailableOptions.Add(new SelectListItem() { Text = sao.Name, Value = sao.Id.ToString() });
                }
            }
        }

        [NonAction]
        private void PrepareAddProductPictureModel(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.AddPictureModel == null)
                model.AddPictureModel = new ProductModel.ProductPictureModel();
        }

        [NonAction]
        private void PrepareCategoryMapping(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableCategories = _categoryService.GetAllCategories(showHidden: true).Count;
        }
        
        [NonAction]
        private void PrepareManufacturerMapping(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableManufacturers = _manufacturerService.GetAllManufacturers(true).Count;
        }

        //add by hz
        [NonAction]
        private void PrepareVendorMapping(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableVendors = _vendorService.GetAllVendors(0,true).Count;
        }
        //end by hz

        [NonAction]
        private void PrepareAclModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (product != null)
                {
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(product);
                }
                else
                {
                    model.SelectedCustomerRoleIds = new int[0];
                }
            }
        }

        [NonAction]
        protected void SaveProductAcl(Product product, ProductModel model)
        {
            var existingAclRecords = _aclService.GetAclRecords(product);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Where(acl => acl.CustomerRoleId == customerRole.Id).Count() == 0)
                        _aclService.InsertAclRecord(product, customerRole.Id);
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
        private void PrepareVariantsModel(ProductModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product != null)
            {
                var variants = _productService.GetProductVariantsByProductId(product.Id, true);
                foreach (var variant in variants)
                {
                    var variantModel = variant.ToModel();
                    if (String.IsNullOrEmpty(variantModel.Name))
                        variantModel.Name = "Unnamed";
                    model.ProductVariantModels.Add(variantModel);
                }
            }
        }

        [NonAction]
        private void PrepareProductPictureThumbnailModel(ProductModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product != null)
            {
                if (_adminAreaSettings.DisplayProductPictures)
                {
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                    model.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);
                }
            }
        }

        [NonAction]
        private void PrepareCopyProductModel(ProductModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product != null)
            {
                model.CopyProductModel.Id = product.Id;
                model.CopyProductModel.Name = "Copy of " + product.Name;
                model.CopyProductModel.Published = true;
                model.CopyProductModel.CopyImages = true;
            }
        }

        [NonAction]
        private void PrepareTags(ProductModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product != null)
            {
                var result = new StringBuilder();
                for (int i = 0; i < product.ProductTags.Count; i++)
                {
                    var pt = product.ProductTags.ToList()[i];
                    result.Append(pt.Name);
                    if (i != product.ProductTags.Count - 1)
                        result.Append(", ");
                }
                model.ProductTags = result.ToString();
            }
        }

        [NonAction]
        private void UpdateLocales(ProductTag productTag, ProductTagModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productTag,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        private void FirstVariant_UpdateLocales(ProductVariant variant, ProductVariantModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(variant,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(variant,
                                                               x => x.Description,
                                                               localized.Description,
                                                               localized.LanguageId);
            }
        }
        
        [NonAction]
        private void FirstVariant_PrepareProductVariantModel(ProductVariantModel model, ProductVariant variant, bool setPredefinedValues)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = variant != null && !setPredefinedValues && tc.Id == variant.TaxCategoryId });

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;


            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.DisplayOrder = 1;

                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.Published = true;
            }

            //little hack here in order to hide some of properties of the first product variant
            //we do it because they dublicate some properties of a product
            model.HideNameAndDescriptionProperties = true;
            model.HidePublishedProperty = true;
            model.HideDisplayOrderProperty = true;
        }

        [NonAction]
        private string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (!String.IsNullOrWhiteSpace(productTags))
            {
                string[] values = productTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string val1 in values)
                    if (!String.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
            }
            return result.ToArray();
        }

        [NonAction]
        private void SaveProductTags(Product product, string[] productTags)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //product tags
            var existingProductTags = product.ProductTags.OrderByDescending(pt => pt.ProductCount).ToList();
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                bool found = false;
                foreach (string newProductTag in productTags)
                {
                    if (existingProductTag.Name.Equals(newProductTag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    productTagsToRemove.Add(existingProductTag);
                }
            }
            foreach (var productTag in productTagsToRemove)
            {
                product.ProductTags.Remove(productTag);
                //ensure product is saved before updating totals
                _productService.UpdateProduct(product);
                _productTagService.UpdateProductTagTotals(productTag);
            }
            foreach (string productTagName in productTags)
            {
                ProductTag productTag = null;
                var productTag2 = _productTagService.GetProductTagByName(productTagName);
                if (productTag2 == null)
                {
                    //add new product tag
                    productTag = new ProductTag()
                    {
                        Name = productTagName,
                        ProductCount = 0
                    };
                    _productTagService.InsertProductTag(productTag);
                }
                else
                {
                    productTag = productTag2;
                }
                if (!product.ProductTagExists(productTag.Id))
                {
                    product.ProductTags.Add(productTag);
                    //ensure product is saved before updating totals
                    _productService.UpdateProduct(product);
                }
                //update product tag totals 
                _productTagService.UpdateProductTagTotals(productTag);
            }
        }
        #endregion

        #region Methods

        #region Product list / create / edit / delete

        //list products
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
   
            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(0, 0,
                customerVendorId, //add by hz
                null, null, null, 0, string.Empty, false, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, _adminAreaSettings.GridPageSize,
                false, out filterableSpecificationAttributeOptionIds, true);

            var model = new ProductListModel();
            model.DisplayProductPictures = _adminAreaSettings.DisplayProductPictures;
            model.DisplayPdfDownloadCatalog = _pdfSettings.Enabled;
            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x =>
                {
                    var productModel = x.ToModel();
                    PrepareProductPictureThumbnailModel(productModel, x);
                    PrepareVariantsModel(productModel, x);
                    return productModel;
                }),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(showHidden: true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            //add by hz
            //vendors
            model.IsVendorManager = (customerVendorId > 0) ? true : false;
            model.AvailableVendors.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _vendorService.GetAllVendors(0, true))
                model.AvailableVendors.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString() });
           
                //end by hz

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command, ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var gridModel = new GridModel();
            int vendorToSearch = (customerVendorId > 0) ? customerVendorId : model.SearchVendorId;//add by hz
            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(model.SearchCategoryId,
                model.SearchManufacturerId,
                vendorToSearch,//add by hz
                null, null, null, 0, model.SearchProductName, false, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, command.Page - 1, command.PageSize,
                false, out filterableSpecificationAttributeOptionIds, true);
            gridModel.Data = products.Select(x =>
                                                 {
                                                     var productModel = x.ToModel();
                                                     PrepareProductPictureThumbnailModel(productModel, x);
                                                     return productModel;
                                                 });
            gridModel.Total = products.TotalCount;
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public ActionResult GoToSku(ProductListModel model)
        {
            string sku = model.GoDirectlyToSku;
            var pv = _productService.GetProductVariantBySku(sku);
            if (pv != null)
                return RedirectToAction("Edit", "ProductVariant", new { id = pv.Id });
            
            //not found
            return List();
        }

        //create product
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var model = new ProductModel();

            //product
            AddLocales(_languageService, model.Locales);
            PrepareTemplatesModel(model);
            PrepareAddSpecificationAttributeModel(model);
            PrepareAddProductPictureModel(model);
            PrepareCategoryMapping(model);
            PrepareManufacturerMapping(model);
            PrepareVendorMapping(model);//add by hz
            PrepareAclModel(model, null, false);
            //default values
            model.Published = true;
            model.AllowCustomerReviews = true;
            //first product variant
            model.FirstProductVariantModel = new ProductVariantModel();
            AddLocales(_languageService, model.FirstProductVariantModel.Locales);
            FirstVariant_PrepareProductVariantModel(model.FirstProductVariantModel, null, true);
            model.IsVendorManager = (customerVendorId > 0) ? true : false;//add by hz
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //product
                var product = model.ToEntity();
                product.CreatedOnUtc = DateTime.UtcNow;
                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.InsertProduct(product);
                //search engine name
                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);
                //locales
                UpdateLocales(product, model);
                //tags
                SaveProductTags(product, ParseProductTags(model.ProductTags));
                //ACL (customer roles)
                SaveProductAcl(product, model);

                //default product variant
                var variant = model.FirstProductVariantModel.ToEntity();
                variant.ProductId = product.Id;
                variant.Published = true;
                variant.DisplayOrder = 1;
                variant.CreatedOnUtc = DateTime.UtcNow;
                variant.UpdatedOnUtc = DateTime.UtcNow;
                _productService.InsertProductVariant(variant);
                FirstVariant_UpdateLocales(variant, model.FirstProductVariantModel);
                
                //tags (after variant because it can effect product count)
	            SaveProductTags(product, ParseProductTags(model.ProductTags));

                //add by hz
                if (customerVendorId > 0)
                {
                    var existingProductvendors = _vendorService.GetProductVendorsByVendorId(customerVendorId, 0, int.MaxValue, true);
                        if (existingProductvendors.FindProductVendor(product.Id, customerVendorId) == null)
                        {
                            _vendorService.InsertProductVendor(
                                new ProductVendor()
                                {
                                    VendorId = customerVendorId,
                                    ProductId = product.Id,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                });
                        }

                }
                //end by hz

                //activity log
                _customerActivityService.InsertActivity("AddNewProduct", _localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name);
                
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //product
            PrepareTemplatesModel(model);
            PrepareAddSpecificationAttributeModel(model);
            PrepareAddProductPictureModel(model);
            PrepareCategoryMapping(model);
            PrepareManufacturerMapping(model);
            PrepareVendorMapping(model);//add by hz
            PrepareAclModel(model, null, true);
            //first product variant
            FirstVariant_PrepareProductVariantModel(model.FirstProductVariantModel, null, false);
            return View(model);
        }

        //edit product
        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();
            //add by hz
            int ProductVendorId = _vendorService.GetVendorIdByProductId(id);
            if (customerVendorId != ProductVendorId && customerVendorId > 0)
                return AccessDeniedView();
            //end by hz

            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted)
                //No product found with the specified id
                return RedirectToAction("List");

            var model = product.ToModel();
            model.IsVendorManager = (customerVendorId > 0) ? true : false;//add by hz
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
                {
                    locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                    locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                    locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                    locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                    locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                    locale.SeName = product.GetSeName(languageId, false, false);
                });

            PrepareTags(model, product);
            PrepareCopyProductModel(model, product);
            PrepareVariantsModel(model, product);
            PrepareTemplatesModel(model);
            PrepareAddSpecificationAttributeModel(model);
            PrepareAddProductPictureModel(model);
            PrepareCategoryMapping(model);
            PrepareManufacturerMapping(model);
            PrepareVendorMapping(model);//add by hz
            PrepareAclModel(model, product, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                //No product found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //product
                product = model.ToEntity(product);
                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(product);
                //search engine name
                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);
                //locales
                UpdateLocales(product, model);
                //ACL (customer roles)
                SaveProductAcl(product, model);
                //picture seo names
                UpdatePictureSeoNames(product);

                //activity log
                _customerActivityService.InsertActivity("EditProduct", _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);
                
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareTags(model, product);
            PrepareCopyProductModel(model, product);
            PrepareVariantsModel(model, product);
            PrepareTemplatesModel(model);
            PrepareAddSpecificationAttributeModel(model);
            PrepareAddProductPictureModel(model);
            PrepareCategoryMapping(model);
            PrepareManufacturerMapping(model);
            PrepareVendorMapping(model);//add by hz
            PrepareAclModel(model, product, true);
            return View(model);
        }

        //delete product
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            _productService.DeleteProduct(product);
            //update product tag totals
            UpdateProductTagTotals(product);

            //activity log
            _customerActivityService.InsertActivity("DeleteProduct", _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);
                
            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Deleted"));
            return RedirectToAction("List");
        }

        public ActionResult DeleteSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    _productService.DeleteProduct(product);
                    //update product tag totals
                    UpdateProductTagTotals(product);
                }
            }

            return RedirectToAction("List");
        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult GetVariants(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var variants = _productService.GetProductVariantsByProductId(productId, true);

            var gridModel = new GridModel<ProductVariantModel>()
            {
                Data = variants.Select(x =>
                {
                    var variantModel = x.ToModel();
                    if (String.IsNullOrEmpty(variantModel.Name))
                        variantModel.Name = "Unnamed";
                    return variantModel;
                }),
                Total = variants.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult CopyProduct(ProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(copyModel.Id);
                var newProduct = _copyProductService.CopyProduct(originalProduct, 
	            copyModel.Name, copyModel.Published, copyModel.CopyImages);	                 
                SuccessNotification("The product has been copied successfully");
                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion
        
        #region Product categories

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductCategoryList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productCategories = _categoryService.GetProductCategoriesByProductId(productId, true);
            var productCategoriesModel = productCategories
                .Select(x =>
                {
                    return new ProductModel.ProductCategoryModel()
                    {
                        Id = x.Id,
                        Category = _categoryService.GetCategoryById(x.CategoryId).GetCategoryBreadCrumb(_categoryService),
                        ProductId = x.ProductId,
                        CategoryId = x.CategoryId,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder  = x.DisplayOrder
                    };
                })
                .ToList();

            var model = new GridModel<ProductModel.ProductCategoryModel>
            {
                Data = productCategoriesModel,
                Total = productCategoriesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductCategoryInsert(GridCommand command, ProductModel.ProductCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();
            var productId = model.ProductId;
	        var categoryId = Int32.Parse(model.Category); //use Category property (not CategoryId) because appropriate property is stored in it
	               
	        var existingProductCategories = _categoryService.GetProductCategoriesByCategoryId(categoryId, 0, int.MaxValue, true);
            if (existingProductCategories.FindProductCategory(productId, categoryId) == null)
            {
                var productCategory = new ProductCategory()
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    IsFeaturedProduct = model.IsFeaturedProduct,
                    DisplayOrder = model.DisplayOrder
                };
                _categoryService.InsertProductCategory(productCategory);
            }
            return ProductCategoryList(command, productId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductCategoryUpdate(GridCommand command, ProductModel.ProductCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productCategory = _categoryService.GetProductCategoryById(model.Id);
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            //use Category property (not CategoryId) because appropriate property is stored in it
            productCategory.CategoryId = Int32.Parse(model.Category);
            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            productCategory.DisplayOrder = model.DisplayOrder;
            _categoryService.UpdateProductCategory(productCategory);

            return ProductCategoryList(command, productCategory.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductCategoryDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productCategory = _categoryService.GetProductCategoryById(id);
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            var productId = productCategory.ProductId;
            _categoryService.DeleteProductCategory(productCategory);

            return ProductCategoryList(command, productId);
        }

        #endregion

        #region Product manufacturers

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductManufacturerList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(productId, true);
            var productManufacturersModel = productManufacturers
                .Select(x =>
                {
                    return new ProductModel.ProductManufacturerModel()
                    {
                        Id = x.Id,
                        Manufacturer = _manufacturerService.GetManufacturerById(x.ManufacturerId).Name,
                        ProductId = x.ProductId,
                        ManufacturerId = x.ManufacturerId,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder = x.DisplayOrder
                    };
                })
                .ToList();

            var model = new GridModel<ProductModel.ProductManufacturerModel>
            {
                Data = productManufacturersModel,
                Total = productManufacturersModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductManufacturerInsert(GridCommand command, ProductModel.ProductManufacturerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();
            var productId = model.ProductId;
	        var manufacturerId = Int32.Parse(model.Manufacturer); //use Manufacturer property (not ManufacturerId) because appropriate property is stored in it
	  
	        var existingProductmanufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, 0, int.MaxValue, true);
            if (existingProductmanufacturers.FindProductManufacturer(productId, manufacturerId) == null)
            {
                var productManufacturer = new ProductManufacturer()
                {
                   ProductId = productId,
                   ManufacturerId = manufacturerId,
                   IsFeaturedProduct = model.IsFeaturedProduct,
                   DisplayOrder = model.DisplayOrder
                };
                _manufacturerService.InsertProductManufacturer(productManufacturer);
            }
            return ProductManufacturerList(command, productId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductManufacturerUpdate(GridCommand command, ProductModel.ProductManufacturerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productManufacturer = _manufacturerService.GetProductManufacturerById(model.Id);
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            //use Manufacturer property (not ManufacturerId) because appropriate property is stored in it
            productManufacturer.ManufacturerId = Int32.Parse(model.Manufacturer);
            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            productManufacturer.DisplayOrder = model.DisplayOrder;
            _manufacturerService.UpdateProductManufacturer(productManufacturer);

            return ProductManufacturerList(command, productManufacturer.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductManufacturerDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productManufacturer = _manufacturerService.GetProductManufacturerById(id);
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            var productId = productManufacturer.ProductId;
            _manufacturerService.DeleteProductManufacturer(productManufacturer);

            return ProductManufacturerList(command, productId);
        }
        
        #endregion

        //add by hz
        #region Product vendors

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVendorList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVendors = _vendorService.GetProductVendorsByProductId(productId, true);
            var productVendorsModel = productVendors
                .Select(x =>
                {
                    return new ProductModel.ProductVendorModel()
                    {
                        Id = x.Id,
                        Vendor = _vendorService.GetVendorById(x.VendorId).Name,
                        ProductId = x.ProductId,
                        VendorId = x.VendorId,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder = x.DisplayOrder
                    };
                })
                .ToList();

            var model = new GridModel<ProductModel.ProductVendorModel>
            {
                Data = productVendorsModel,
                Total = productVendorsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVendorInsert(GridCommand command, ProductModel.ProductVendorModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();
            var productId = model.ProductId;
            var vendorId = Int32.Parse(model.Vendor); //use Vendor property (not VendorId) because appropriate property is stored in it

            var existingProductvendors = _vendorService.GetProductVendorsByVendorId(vendorId, 0, int.MaxValue, true);
            if (existingProductvendors.FindProductVendor(productId, vendorId) == null)
            {
                var productVendor = new ProductVendor()
                {
                    ProductId = productId,
                    VendorId = vendorId,
                    IsFeaturedProduct = model.IsFeaturedProduct,
                    DisplayOrder = model.DisplayOrder
                };
                _vendorService.InsertProductVendor(productVendor);
            }
            return ProductVendorList(command, productId);
        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVendorUpdate(GridCommand command, ProductModel.ProductVendorModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVendor = _vendorService.GetProductVendorById(model.Id);
            if (productVendor == null)
                throw new ArgumentException("No product vendor mapping found with the specified id");

            //use Vendor property (not VendorId) because appropriate property is stored in it
            productVendor.VendorId = Int32.Parse(model.Vendor);
            productVendor.IsFeaturedProduct = model.IsFeaturedProduct;
            productVendor.DisplayOrder = model.DisplayOrder;
            _vendorService.UpdateProductVendor(productVendor);

            return ProductVendorList(command, productVendor.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVendorDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVendor = _vendorService.GetProductVendorById(id);
            if (productVendor == null)
                throw new ArgumentException("No product vendor mapping found with the specified id");

            var productId = productVendor.ProductId;
            _vendorService.DeleteProductVendor(productVendor);

            return ProductVendorList(command, productId);
        }

        #endregion
        //end by hz

        #region Related products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RelatedProductList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId, true);
            var relatedProductsModel = relatedProducts
                .Select(x =>
                {
                    return new ProductModel.RelatedProductModel()
                    {
                        Id = x.Id,
                        ProductId1 = x.ProductId1,
                        ProductId2 = x.ProductId2,
                        Product2Name = _productService.GetProductById(x.ProductId2).Name,
                        DisplayOrder = x.DisplayOrder
                    };
                })
                .ToList();

            var model = new GridModel<ProductModel.RelatedProductModel>
            {
                Data = relatedProductsModel,
                Total = relatedProductsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }
        
        [GridAction(EnableCustomBinding = true)]
        public ActionResult RelatedProductUpdate(GridCommand command, ProductModel.RelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var relatedProduct = _productService.GetRelatedProductById(model.Id);
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            relatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateRelatedProduct(relatedProduct);

            return RelatedProductList(command, relatedProduct.ProductId1);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RelatedProductDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var relatedProduct = _productService.GetRelatedProductById(id);
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            var productId = relatedProduct.ProductId1;
            _productService.DeleteRelatedProduct(relatedProduct);

            return RelatedProductList(command, productId);
        }
        
        public ActionResult RelatedProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(0, 0,
                customerVendorId,//add by hz
                null, null, null, 0, string.Empty, false, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, _adminAreaSettings.GridPageSize,
                false, out filterableSpecificationAttributeOptionIds, true);

            var model = new ProductModel.AddRelatedProductModel();
            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(showHidden: true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            ////add by hz
            ////vendors
            //model.AvailableVendors.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            //foreach (var m in _vendorService.GetAllVendors(0,true))
            //    model.AvailableVendors.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            ////end by hz

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RelatedProductAddPopupList(GridCommand command, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var gridModel = new GridModel();
            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(model.SearchCategoryId, model.SearchManufacturerId,
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
        public ActionResult RelatedProductAddPopup(string btnId, string formId, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingRelatedProducts = _productService.GetRelatedProductsByProductId1(model.ProductId);
                        if (existingRelatedProducts.FindRelatedProduct(model.ProductId, id) == null)
                        {
                            _productService.InsertRelatedProduct(
                                new RelatedProduct()
                                {
                                    ProductId1 = model.ProductId,
                                    ProductId2 = id,
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

        #region Cross-sell products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CrossSellProductList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var crossSellProducts = _productService.GetCrossSellProductsByProductId1(productId, true);
            var crossSellProductsModel = crossSellProducts
                .Select(x =>
                {
                    return new ProductModel.CrossSellProductModel()
                    {
                        Id = x.Id,
                        ProductId1 = x.ProductId1,
                        ProductId2 = x.ProductId2,
                        Product2Name = _productService.GetProductById(x.ProductId2).Name,
                    };
                })
                .ToList();

            var model = new GridModel<ProductModel.CrossSellProductModel>
            {
                Data = crossSellProductsModel,
                Total = crossSellProductsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CrossSellProductDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var crossSellProduct = _productService.GetCrossSellProductById(id);
            if (crossSellProduct == null)
                throw new ArgumentException("No cross-sell product found with the specified id");

            var productId = crossSellProduct.ProductId1;
            _productService.DeleteCrossSellProduct(crossSellProduct);

            return CrossSellProductList(command, productId);
        }

        public ActionResult CrossSellProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(0, 0,
                customerVendorId,//add by hz
                null, null, null, 0, string.Empty, false, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, _adminAreaSettings.GridPageSize,
                false, out filterableSpecificationAttributeOptionIds, true);

            var model = new ProductModel.AddCrossSellProductModel();
            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(showHidden: true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            ////add by hz
            ////vendors
            //model.AvailableVendors.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            //foreach (var m in _vendorService.GetAllVendors(0,true))
            //    model.AvailableVendors.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            ////add by hz

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CrossSellProductAddPopupList(GridCommand command, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var gridModel = new GridModel();
            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(model.SearchCategoryId,
                model.SearchManufacturerId,
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
        public ActionResult CrossSellProductAddPopup(string btnId, string formId, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingCrossSellProducts = _productService.GetCrossSellProductsByProductId1(model.ProductId);
                        if (existingCrossSellProducts.FindCrossSellProduct(model.ProductId, id) == null)
                        {
                            _productService.InsertCrossSellProduct(
                                new CrossSellProduct()
                                {
                                    ProductId1 = model.ProductId,
                                    ProductId2 = id,
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

        #region Product pictures

        public ActionResult ProductPictureAdd(int pictureId, int displayOrder, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            _productService.InsertProductPicture(new ProductPicture()
            {
                PictureId = pictureId,
                ProductId = productId,
                DisplayOrder = displayOrder,
            });

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(product.Name));
            

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductPictureList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productPictures = _productService.GetProductPicturesByProductId(productId);
            var productPicturesModel = productPictures
                .Select(x =>
                {
                    return new ProductModel.ProductPictureModel()
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        PictureId = x.PictureId,
                        PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                        DisplayOrder = x.DisplayOrder
                    };
                })
                .ToList();

            var model = new GridModel<ProductModel.ProductPictureModel>
            {
                Data = productPicturesModel,
                Total = productPicturesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductPictureUpdate(ProductModel.ProductPictureModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productPicture = _productService.GetProductPictureById(model.Id);
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");

            productPicture.DisplayOrder = model.DisplayOrder;
            _productService.UpdateProductPicture(productPicture);

            return ProductPictureList(command, productPicture.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductPictureDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productPicture = _productService.GetProductPictureById(id);
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");

            var productId = productPicture.ProductId;
            _productService.DeleteProductPicture(productPicture);

            var picture = _pictureService.GetPictureById(productPicture.PictureId);
            _pictureService.DeletePicture(picture);
            
            return ProductPictureList(command, productId);
        }

        #endregion

        #region Product specification attributes

        public ActionResult ProductSpecificationAttributeAdd(int specificationAttributeOptionId, 
            string customValue, bool allowFiltering, bool showOnProductPage,
            int displayOrder, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var psa = new ProductSpecificationAttribute()
            {
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                ProductId = productId,
                CustomValue = customValue,
                AllowFiltering = allowFiltering,
                ShowOnProductPage = showOnProductPage,
                DisplayOrder = displayOrder,
            };
            _specificationAttributeService.InsertProductSpecificationAttribute(psa);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductSpecAttrList(GridCommand command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productrSpecs = _specificationAttributeService.GetProductSpecificationAttributesByProductId(productId);

            var productrSpecsModel = productrSpecs
                .Select(x =>
                {
                    var psaModel = new ProductSpecificationAttributeModel()
                    {
                        Id = x.Id,
                        SpecificationAttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOption.Name,
                        CustomValue = x.CustomValue,
                        AllowFiltering = x.AllowFiltering,
                        ShowOnProductPage = x.ShowOnProductPage,
                        DisplayOrder = x.DisplayOrder
                    };
                    return psaModel;
                })
                .ToList();

            var model = new GridModel<ProductSpecificationAttributeModel>
            {
                Data = productrSpecsModel,
                Total = productrSpecsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductSpecAttrUpdate(int psaId, ProductSpecificationAttributeModel model,
            GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(psaId);
            psa.CustomValue = model.CustomValue;
            psa.AllowFiltering = model.AllowFiltering;
            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            _specificationAttributeService.UpdateProductSpecificationAttribute(psa);

            return ProductSpecAttrList(command, psa.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductSpecAttrDelete(int psaId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(psaId);
            if (psa == null)
                throw new ArgumentException("No specification attribute found with the specified id");

            var productId = psa.ProductId;
            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);

            return ProductSpecAttrList(command, productId);
        }

        #endregion

        #region Product tags

        public ActionResult ProductTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductTags(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var tags = _productTagService.GetAllProductTags(true)
                .Select(x =>
                {
                    return new ProductTagModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ProductCount = x.ProductCount
                    };
                })
                .ForCommand(command);

            var model = new GridModel<ProductTagModel>
            {
                Data = tags.PagedForCommand(command),
                Total = tags.Count()
            };
            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductTagDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var tag = _productTagService.GetProductTagById(id);
            if (tag == null)
                throw new ArgumentException("No product tag found with the specified id");
            _productTagService.DeleteProductTag(tag);

            return ProductTags(command);
        }

        //edit
        public ActionResult EditProductTag(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productTag = _productTagService.GetProductTagById(id);
            if (productTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            var model = new ProductTagModel()
            {
                Id = productTag.Id,
                Name = productTag.Name,
                ProductCount = productTag.ProductCount
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productTag.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProductTag(string btnId, string formId, ProductTagModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productTag = _productTagService.GetProductTagById(model.Id);
            if (productTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productTag.Name = model.Name;
                _productTagService.UpdateProductTag(productTag);
                //locales
                UpdateLocales(productTag, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Export / Import

        public ActionResult DownloadCatalogAsPdf()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            try
            {
                IList<int> filterableSpecificationAttributeOptionIds = null;
                var products = _productService.SearchProducts(0, 0,
                    customerVendorId,//add by hz
                    null, null, null, 0, string.Empty, false, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue,
                    false, out filterableSpecificationAttributeOptionIds, true);


                byte[] bytes = null;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintProductsToPdf(stream, products, _workContext.WorkingLanguage);
                    bytes = stream.ToArray();
                }
                return File(bytes, "application/pdf", "pdfcatalog.pdf");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public ActionResult ExportXmlAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            try
            {
                IList<int> filterableSpecificationAttributeOptionIds = null;
                var products = _productService.SearchProducts(0, 0,
                    customerVendorId,//add by hz
                    null, null, null, 0, string.Empty, false, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue,
                    false, out filterableSpecificationAttributeOptionIds, true);

                var xml = _exportManager.ExportProductsToXml(products);
                return new XmlDownloadResult(xml, "products.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public ActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }

            var xml = _exportManager.ExportProductsToXml(products);
            return new XmlDownloadResult(xml, "products.xml");
        }

        public ActionResult ExportExcelAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            try
            {
                IList<int> filterableSpecificationAttributeOptionIds = null;
                var products = _productService.SearchProducts(0, 0,
                    customerVendorId,//add by hz
                    null, null, null, 0, string.Empty, false, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue,
                    false, out filterableSpecificationAttributeOptionIds, true);
                
                byte[] bytes = null;
                using (var stream = new MemoryStream())
                {
                    _exportManager.ExportProductsToXlsx(stream, products);
                    bytes = stream.ToArray();
                }
                return File(bytes, "text/xls", "products.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public ActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                 && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }

            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                _exportManager.ExportProductsToXlsx(stream, products);
                bytes = stream.ToArray();
            }
            return File(bytes, "text/xls", "products.xlsx");
        }

        [HttpPost]
        public ActionResult ImportExcel(FormCollection form)//add by hz to do
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            try
            {
                var file = Request.Files["importexcelfile"];
                if (file != null && file.ContentLength > 0)
                {
                    _importManager.ImportProductsFromXlsx(file.InputStream);
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }

        }

        #endregion

        #endregion
    }
}
