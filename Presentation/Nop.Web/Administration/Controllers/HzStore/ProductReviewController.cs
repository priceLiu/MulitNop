//add by hz page
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core;//add by hz
using Nop.Core.Data;//add by hz
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ProductReviewController : BaseNopController
    {
        #region Fields

        private readonly ICustomerContentService _customerContentService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;//add by hz
        private readonly IVendorService _vendorService;//add by hz
        private readonly int customerVendorId;//add by hz
        private readonly IRepository<ProductVendor> _productVendorRepository;

        #endregion Fields

        #region Constructors

        public ProductReviewController(ICustomerContentService customerContentService,
            IProductService productService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, IPermissionService permissionService
            ,IWorkContext workContext//add by hz
            , IVendorService vendorService//add by hz
            ,IRepository<ProductVendor> productVendorRepository
            )
        {
            this._customerContentService = customerContentService;
            this._productService = productService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._vendorService = vendorService;//add by hz
            customerVendorId = _vendorService.GetVendorIdByCustomerId(_workContext.CurrentCustomer.Id);//add by hz
            _productVendorRepository = productVendorRepository;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");

            model.Id = productReview.Id;
            model.ProductId = productReview.ProductId;
            model.ProductName = productReview.Product.Name;
            model.CustomerId = productReview.CustomerId;
            var customer = productReview.Customer;
	        model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.IpAddress = productReview.IpAddress;
            model.Rating = productReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
            if (!excludeProperties)
            {
                model.Title = productReview.Title;
                if (formatReviewText)
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(productReview.ReviewText, false, true, false, false, false, false);
                else
                    model.ReviewText = productReview.ReviewText;
                model.IsApproved = productReview.IsApproved;
            }
        }

        #endregion

        #region Methods

        //list
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

            var model = new ProductReviewListModel();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, ProductReviewListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var productReviews = _customerContentService.GetAllCustomerContent<ProductReview>(0, null,
                createdOnFromValue, createdToFromValue);
            //add by hz

            if (customerVendorId > 0)
            {
                var temp = from p in productReviews
                           join ps in _productVendorRepository.Table on p.ProductId equals ps.ProductId
                           where ps.VendorId == customerVendorId
                           select p;
                productReviews = temp.ToList();
                //add by hz to do
            }
            //end by hz
            var gridModel = new GridModel<ProductReviewModel>
            {
                Data = productReviews.PagedForCommand(command).Select(x =>
                {
                    var m = new ProductReviewModel();
                    PrepareProductReviewModel(m, x, false, true);
                    return m;
                }),
                Total = productReviews.Count,
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        //edit
        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            //add by hz
            if (customerVendorId > 0 && _vendorService.GetVendorIdByProductId(productReview.ProductId) != customerVendorId)
                return AccessDeniedView();
            //end by hz

            var model = new ProductReviewModel();
            PrepareProductReviewModel(model, productReview, false, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(ProductReviewModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productReview = _customerContentService.GetCustomerContentById(model.Id) as ProductReview;
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productReview.Title = model.Title;
                productReview.ReviewText = model.ReviewText;
                productReview.IsApproved = model.IsApproved;
                productReview.UpdatedOnUtc = DateTime.UtcNow;
                _customerContentService.UpdateCustomerContent(productReview);
                
                //update product totals
                _productService.UpdateProductReviewTotals(productReview.Product);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", productReview.Id) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            PrepareProductReviewModel(model, productReview, true, false);
            return View(model);
        }
        
        //delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
            if (productReview == null)
                //No product review found with the specified id
                return RedirectToAction("List");
            
            //add by hz
            if (customerVendorId > 0 && _vendorService.GetVendorIdByProductId(productReview.ProductId) != customerVendorId)
                return AccessDeniedView();
            //end by hz

            var product = productReview.Product;
            _customerContentService.DeleteCustomerContent(productReview);
            //update product totals
            _productService.UpdateProductReviewTotals(product);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductReviews.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult ApproveSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
                    if (productReview != null)
                    {
                        productReview.IsApproved = true;
                        _customerContentService.UpdateCustomerContent(productReview);
                        //update product totals
                        _productService.UpdateProductReviewTotals(productReview.Product);
                    }
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult DisapproveSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog)
                && !_permissionService.Authorize(StandardPermissionProvider.ManageVendor) //add by hz
                )
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
                    if (productReview != null)
                    {
                        productReview.IsApproved = false;
                        _customerContentService.UpdateCustomerContent(productReview);
                        //update product totals
                        _productService.UpdateProductReviewTotals(productReview.Product);
                    }
                }
            }

            return Json(new { Result = true });
        }




        #endregion
    }
}
