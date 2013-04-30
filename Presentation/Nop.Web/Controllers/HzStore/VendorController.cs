//add by hz full
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Services.Logging;
using Nop.Web.Framework.Events;
using Nop.Services.Events;

namespace Nop.Web.Controllers
{
    public partial class VendorController : BaseNopController
    {
		#region Fields

        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAclService _aclService;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
    
        private readonly ICacheManager _cacheManager;
        

        private readonly IVendorService _vendorService; //add by hz
        private readonly IVendorTemplateService _vendorTemplateService; //add by hz

        #endregion

		#region Constructors

        public VendorController(
            IProductService productService, 
            IProductTemplateService productTemplateService,
            IWorkContext workContext, 
            ITaxService taxService, 
            ICurrencyService currencyService,
            IPictureService pictureService, 
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
            IWebHelper webHelper, ISpecificationAttributeService specificationAttributeService,
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService, 
            ICustomerActivityService customerActivityService, 
            IAclService aclService,
            MediaSettings mediaSettings, CatalogSettings catalogSettings,
        
            ICacheManager cacheManager
            , IVendorService vendorService //add by hz
            , IVendorTemplateService vendorTemplateService //add by hz
            )
        {
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._workContext = workContext;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._webHelper = webHelper;
            this._specificationAttributeService = specificationAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._aclService = aclService;

            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;

            this._cacheManager = cacheManager;
            this._vendorService = vendorService; //add by hz
            this._vendorTemplateService = vendorTemplateService; // add by hz
        }

        #endregion

        #region Utilities
      
        [NonAction]
        protected IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            //performance optimization. let's load all variants at one go
            var allVariants = _productService.GetProductVariantsByProductIds(products.Select(x => x.Id).ToArray());


            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel()
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),
                };
                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel();

                    //var productVariants = _productService.GetProductVariantsByProductId(product.Id);
                    //we use already loaded variants
                    var productVariants = allVariants.Where(x => x.ProductId == product.Id).ToList();

                    switch (productVariants.Count)
                    {
                        case 0:
                            {
                                //no variants
                                priceModel.OldPrice = null;
                                priceModel.Price = null;
                            }
                            break;
                        default:
                            {

                                if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                {
                                    //calculate for the maximum quantity (in case if we have tier prices)
                                    decimal? minimalPrice = null;
                                    var productVariant = _priceCalculationService.GetProductVariantWithMinimalPrice(productVariants, _workContext.CurrentCustomer, true, int.MaxValue, out minimalPrice);

                                    if (!productVariant.CustomerEntersPrice)
                                    {
                                        if (productVariant.CallForPrice)
                                        {
                                            priceModel.OldPrice = null;
                                            priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                        }
                                        else if (minimalPrice.HasValue)
                                        {
                                            //calculate prices
                                            decimal taxRate = decimal.Zero;
                                            decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
                                            decimal finalPriceBase = _taxService.GetProductPrice(productVariant, minimalPrice.Value, out taxRate);

                                            decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                            decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                            //do we have tier prices configured?
                                            var tierPrices = new List<TierPrice>();
                                            if (productVariant.HasTierPrices)
                                            {
                                                tierPrices.AddRange(productVariant.TierPrices
                                                    .OrderBy(tp => tp.Quantity)
                                                    .ToList()
                                                    .FilterForCustomer(_workContext.CurrentCustomer)
                                                    .RemoveDuplicatedQuantities());
                                            }
                                            bool displayFromMessage =
                                                //When there is just one tier (with  qty 1), there are no actual savings in the list.
                                                (tierPrices.Count > 0 && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1)) ||
                                                //we have more than one variant
                                                (productVariants.Count > 1);
                                            if (displayFromMessage)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Actually it's not possible (we presume that minimalPrice always has a value)
                                            //We never should get here
                                            Debug.WriteLine(string.Format("Cannot calculate minPrice for product variant #{0}", productVariant.Id));
                                        }
                                    }
                                }
                                else
                                {
                                    //hide prices
                                    priceModel.OldPrice = null;
                                    priceModel.Price = null;
                                }
                            }
                            break;
                    }

                    //'add to cart' button
                    switch (productVariants.Count)
                    {
                        case 0:
                            {
                                // no variants
                                priceModel.DisableBuyButton = true;
                                priceModel.AvailableForPreOrder = false;
                            }
                            break;
                        case 1:
                            {

                                //only one variant
                                var productVariant = productVariants[0];
                                priceModel.DisableBuyButton = productVariant.DisableBuyButton || !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
                                if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                {
                                    priceModel.DisableBuyButton = true;
                                }
                                priceModel.AvailableForPreOrder = productVariant.AvailableForPreOrder;
                            }
                            break;
                        default:
                            {
                                //multiple variants
                                priceModel.DisableBuyButton = true;
                                priceModel.AvailableForPreOrder = false;
                            }
                            break;
                    }

                    priceModel.ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart;
                    model.ProductPrice = priceModel;
                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture

                    //If a size has been set in the view, we use it in priority
                    int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
                    //prepare picture model
                    var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured());
                    model.DefaultPictureModel = _cacheManager.Get(defaultProductPictureCacheKey, () =>
                    {
                        var picture = product.GetDefaultProductPicture(_pictureService);
                        var pictureModel = new PictureModel()
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name)
                        };
                        return pictureModel;
                    });

                    #endregion
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    //specs for comparing
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);
                }

                models.Add(model);
            }
            return models;
        }

        [NonAction]
        protected IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var model = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
                   .Select(psa =>
                   {
                       return new ProductSpecificationModel()
                       {
                           SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                           SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
                           SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name)
                       };
                   }).ToList();
                return model;
            });
        }
        
        #endregion

        //add by hz	
        #region Vendors

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Vendor(int vendorId, CatalogPagingFilteringModel command)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted)
                return RedirectToRoute("HomePage");

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a vendor before publishing
            if (!vendor.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return RedirectToRoute("HomePage");

            //ACL (access control list)
            if (!_aclService.Authorize(vendor))
                return RedirectToRoute("HomePage");

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));

            if (command.PageNumber <= 0) command.PageNumber = 1;

            var model = vendor.ToModel();




            //sorting
            model.PagingFilteringContext.AllowProductSorting = _catalogSettings.AllowProductSorting;
            if (model.PagingFilteringContext.AllowProductSorting)
            {
                foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
                {
                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + ((int)enumValue).ToString(), null);

                    var sortValue = enumValue.GetLocalizedEnum(_localizationService, _workContext);
                    model.PagingFilteringContext.AvailableSortOptions.Add(new SelectListItem()
                    {
                        Text = sortValue,
                        Value = sortUrl,
                        Selected = enumValue == (ProductSortingEnum)command.OrderBy
                    });
                }
            }



            //view mode
            model.PagingFilteringContext.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;
            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            if (model.PagingFilteringContext.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                model.PagingFilteringContext.AvailableViewModes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetResource("Vendors.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
                    Selected = viewMode == "grid"
                });
                //list
                model.PagingFilteringContext.AvailableViewModes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetResource("Vendors.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
                    Selected = viewMode == "list"
                });
            }

            //page size
            model.PagingFilteringContext.AllowCustomersToSelectPageSize = false;
            if (vendor.AllowCustomersToSelectPageSize && vendor.PageSizeOptions != null)
            {
                var pageSizes = vendor.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (vendor page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        int temp = 0;

                        if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
                    sortUrl = _webHelper.RemoveQueryString(sortUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        int temp = 0;
                        if (!int.TryParse(pageSize, out temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        model.PagingFilteringContext.PageSizeOptions.Add(new SelectListItem()
                        {
                            Text = pageSize,
                            Value = String.Format(sortUrl, pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    model.PagingFilteringContext.PageSizeOptions = model.PagingFilteringContext.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();

                    if (model.PagingFilteringContext.PageSizeOptions.Any())
                    {
                        model.PagingFilteringContext.PageSizeOptions = model.PagingFilteringContext.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        model.PagingFilteringContext.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(model.PagingFilteringContext.PageSizeOptions.FirstOrDefault().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = vendor.PageSize;
            }

            if (command.PageSize <= 0) command.PageSize = vendor.PageSize;


            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(vendor.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, vendor.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }




            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts && _vendorService.GetTotalNumberOfFeaturedProducts(vendorId) > 0)
            {
                //We use the fast GetTotalNumberOfFeaturedProducts before invoking of the slow SearchProducts
                //to ensure that we have at least one featured product
                IList<int> filterableSpecificationAttributeOptionIdsFeatured = null;
                var featuredProducts = _productService.SearchProducts(0,0,
                    vendor.Id, true, null, null, 0, null,
                    false, false, _workContext.WorkingLanguage.Id, null,
                    ProductSortingEnum.Position, 0, int.MaxValue,
                    false, out filterableSpecificationAttributeOptionIdsFeatured);
                model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();
            }



            //products
            IList<int> filterableSpecificationAttributeOptionIds = null;
            var products = _productService.SearchProducts(0,0, vendor.Id,
                _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                minPriceConverted, maxPriceConverted,
                0, string.Empty, false, false, _workContext.WorkingLanguage.Id, null,
                (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize,
                false, out filterableSpecificationAttributeOptionIds);
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            model.PagingFilteringContext.ViewMode = viewMode;

            ////Branches	
            var branches = vendor.Branches.OrderBy(b => b.DisplayOrder).ToList();
            model.Branches = branches.Select(x => x.ToModel()).ToList();

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.VENDOR_TEMPLATE_MODEL_KEY, vendor.VendorTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _vendorTemplateService.GetVendorTemplateById(vendor.VendorTemplateId);
                if (template == null)
                    template = _vendorTemplateService.GetAllVendorTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            //activity log
            _customerActivityService.InsertActivity("PublicVendor.ViewVendor", _localizationService.GetResource("ActivityLog.PublicVendor.ViewVendor"), vendor.Name);

            return View(templateViewPath, model);
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult VendorAll()
        {
            var model = new List<VendorModel>();
            var vendors = _vendorService.GetAllVendors();
            foreach (var vendor in vendors)
            {
                var modelMan = vendor.ToModel();

                //prepare picture model
                int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
                var vendorPictureCacheKey = string.Format(ModelCacheEventConsumer.VENDOR_PICTURE_MODEL_KEY, vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured());
                modelMan.PictureModel = _cacheManager.Get(vendorPictureCacheKey, () =>
                {
                    var pictureModel = new PictureModel()
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(vendor.PictureId),
                        ImageUrl = _pictureService.GetPictureUrl(vendor.PictureId, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), modelMan.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), modelMan.Name)
                    };
                    return pictureModel;
                });
                model.Add(modelMan);
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult VendorNavigation(int currentVendorId)
        {
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
	        .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
	        string cacheKey = string.Format(ModelCacheEventConsumer.VENDOR_NAVIGATION_MODEL_KEY, currentVendorId, _workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds));
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var currentVendor = _vendorService.GetVendorById(currentVendorId);

                var vendors = _vendorService.GetAllVendors();
                var model = new VendorNavigationModel()
                {
                    TotalVendors = vendors.Count
                };

                foreach (var vendor in vendors.Take(_catalogSettings.ManufacturersBlockItemsToDisplay))
                {
                    var modelMan = new VendorBriefInfoModel()
                    {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name),
                        SeName = vendor.GetSeName(),
                        IsActive = currentVendor != null && currentVendor.Id == vendor.Id,
                    };
                    model.Vendors.Add(modelMan);
                }
                return model;
            });

            return PartialView(cacheModel);
        }

        [ChildActionOnly]
        public ActionResult ProductVendors(int productId)
        {
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_VENDORS_MODEL_KEY, productId, _workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds));
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = _vendorService.GetProductVendorsByProductId(productId)
                    .Select(x =>
                    {
                        var s = x.Vendor.ToModel();
                        return s;
                    })
                    .ToList();
                return model;
            });

            return PartialView(cacheModel);
        }
        #endregion
        //end by hz
    }
}
