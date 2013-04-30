//add by hz full page
//add by hz isHzVendor method
//ExportProductsToXml , ExportProductsToXlsx
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
public partial class ExportManager : IExportManager
    {
        #region Fields
        private readonly IVendorService _vendorService;//add by hz

        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            StoreInformationSettings vendorInformationSettings
            , IVendorService vendorService //add by hz
            )
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._storeInformationSettings = vendorInformationSettings;
            this._vendorService = vendorService;//add by hz
        }

        #endregion

        #region Methods

        //add by hz
        /// <summary>
        /// Export vendor list to xml
        /// </summary>
        /// <param name="vendors">Vendors</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportVendorsToXml(IList<Vendor> vendors)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Vendors");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var vendor in vendors)
            {
                xmlWriter.WriteStartElement("Vendor");

                xmlWriter.WriteElementString("VendorId", null, vendor.Id.ToString());
                xmlWriter.WriteElementString("Name", null, vendor.Name);
                xmlWriter.WriteElementString("Description", null, vendor.Description);
                xmlWriter.WriteElementString("VendorTemplateId", null, vendor.VendorTemplateId.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, vendor.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, vendor.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, vendor.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, vendor.GetSeName(0));
                xmlWriter.WriteElementString("PictureId", null, vendor.PictureId.ToString());
                xmlWriter.WriteElementString("PageSize", null, vendor.PageSize.ToString());
                xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, vendor.AllowCustomersToSelectPageSize.ToString());
                xmlWriter.WriteElementString("PageSizeOptions", null, vendor.PageSizeOptions);
                xmlWriter.WriteElementString("PriceRanges", null, vendor.PriceRanges);
                xmlWriter.WriteElementString("Published", null, vendor.Published.ToString());
                xmlWriter.WriteElementString("Deleted", null, vendor.Deleted.ToString());
                xmlWriter.WriteElementString("DisplayOrder", null, vendor.DisplayOrder.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, vendor.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, vendor.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("Products");
                var productVendors = _vendorService.GetProductVendorsByVendorId(vendor.Id, 0, int.MaxValue, true);
                if (productVendors != null)
                {
                    foreach (var productVendor in productVendors)
                    {
                        var product = productVendor.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductVendor");
                            xmlWriter.WriteElementString("ProductVendorId", null, productVendor.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productVendor.ProductId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productVendor.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productVendor.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();


                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }
        //end by hz
       

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportProductsToXml(IList<Product> products, bool IsHzVendor)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Products");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var product in products)
            {
                xmlWriter.WriteStartElement("Product");

                xmlWriter.WriteElementString("ProductId", null, product.Id.ToString());
                xmlWriter.WriteElementString("Name", null, product.Name);
                xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
                xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
                xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
                xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
                xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, product.GetSeName(0));
                xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
                xmlWriter.WriteElementString("Published", null, product.Published.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("ProductVariants");
                var productVariants = _productService.GetProductVariantsByProductId(product.Id, true);
                if (productVariants != null)
                {
                    foreach (var productVariant in productVariants)
                    {
                        xmlWriter.WriteStartElement("ProductVariant");
                        xmlWriter.WriteElementString("ProductVariantId", null, productVariant.Id.ToString());
                        xmlWriter.WriteElementString("ProductId", null, productVariant.ProductId.ToString());
                        xmlWriter.WriteElementString("Name", null, productVariant.Name);
                        xmlWriter.WriteElementString("SKU", null, productVariant.Sku);
                        xmlWriter.WriteElementString("Description", null, productVariant.Description);
                        xmlWriter.WriteElementString("AdminComment", null, productVariant.AdminComment);
                        xmlWriter.WriteElementString("ManufacturerPartNumber", null, productVariant.ManufacturerPartNumber);
                        xmlWriter.WriteElementString("Gtin", null, productVariant.Gtin);
                        xmlWriter.WriteElementString("IsGiftCard", null, productVariant.IsGiftCard.ToString());
                        xmlWriter.WriteElementString("GiftCardType", null, productVariant.GiftCardType.ToString());
                        xmlWriter.WriteElementString("RequireOtherProducts", null, productVariant.RequireOtherProducts.ToString());
                        xmlWriter.WriteElementString("RequiredProductVariantIds", null, productVariant.RequiredProductVariantIds);
                        xmlWriter.WriteElementString("AutomaticallyAddRequiredProductVariants", null, productVariant.AutomaticallyAddRequiredProductVariants.ToString());
                        xmlWriter.WriteElementString("IsDownload", null, productVariant.IsDownload.ToString());
                        xmlWriter.WriteElementString("DownloadId", null, productVariant.DownloadId.ToString());
                        xmlWriter.WriteElementString("UnlimitedDownloads", null, productVariant.UnlimitedDownloads.ToString());
                        xmlWriter.WriteElementString("MaxNumberOfDownloads", null, productVariant.MaxNumberOfDownloads.ToString());
                        if (productVariant.DownloadExpirationDays.HasValue)
                            xmlWriter.WriteElementString("DownloadExpirationDays", null, productVariant.DownloadExpirationDays.ToString());
                        else
                            xmlWriter.WriteElementString("DownloadExpirationDays", null, string.Empty);
                        xmlWriter.WriteElementString("DownloadActivationType", null, productVariant.DownloadActivationType.ToString());
                        xmlWriter.WriteElementString("HasSampleDownload", null, productVariant.HasSampleDownload.ToString());
                        xmlWriter.WriteElementString("SampleDownloadId", null, productVariant.SampleDownloadId.ToString());
                        xmlWriter.WriteElementString("HasUserAgreement", null, productVariant.HasUserAgreement.ToString());
                        xmlWriter.WriteElementString("UserAgreementText", null, productVariant.UserAgreementText);
                        xmlWriter.WriteElementString("IsRecurring", null, productVariant.IsRecurring.ToString());
                        xmlWriter.WriteElementString("RecurringCycleLength", null, productVariant.RecurringCycleLength.ToString());
                        xmlWriter.WriteElementString("RecurringCyclePeriodId", null, productVariant.RecurringCyclePeriodId.ToString());
                        xmlWriter.WriteElementString("RecurringTotalCycles", null, productVariant.RecurringTotalCycles.ToString());
                        xmlWriter.WriteElementString("IsShipEnabled", null, productVariant.IsShipEnabled.ToString());
                        xmlWriter.WriteElementString("IsFreeShipping", null, productVariant.IsFreeShipping.ToString());
                        xmlWriter.WriteElementString("AdditionalShippingCharge", null, productVariant.AdditionalShippingCharge.ToString());
                        xmlWriter.WriteElementString("IsTaxExempt", null, productVariant.IsTaxExempt.ToString());
                        xmlWriter.WriteElementString("TaxCategoryId", null, productVariant.TaxCategoryId.ToString());
                        xmlWriter.WriteElementString("ManageInventoryMethodId", null, productVariant.ManageInventoryMethodId.ToString());
                        xmlWriter.WriteElementString("StockQuantity", null, productVariant.StockQuantity.ToString());
                        xmlWriter.WriteElementString("DisplayStockAvailability", null, productVariant.DisplayStockAvailability.ToString());
                        xmlWriter.WriteElementString("DisplayStockQuantity", null, productVariant.DisplayStockQuantity.ToString());
                        xmlWriter.WriteElementString("MinStockQuantity", null, productVariant.MinStockQuantity.ToString());
                        xmlWriter.WriteElementString("LowStockActivityId", null, productVariant.LowStockActivityId.ToString());
                        xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, productVariant.NotifyAdminForQuantityBelow.ToString());
                        xmlWriter.WriteElementString("BackorderModeId", null, productVariant.BackorderModeId.ToString());
                        xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, productVariant.AllowBackInStockSubscriptions.ToString());
                        xmlWriter.WriteElementString("OrderMinimumQuantity", null, productVariant.OrderMinimumQuantity.ToString());
                        xmlWriter.WriteElementString("OrderMaximumQuantity", null, productVariant.OrderMaximumQuantity.ToString());
                        xmlWriter.WriteElementString("AllowedQuantities", null, productVariant.AllowedQuantities);
                        xmlWriter.WriteElementString("DisableBuyButton", null, productVariant.DisableBuyButton.ToString());
                        xmlWriter.WriteElementString("DisableWishlistButton", null, productVariant.DisableWishlistButton.ToString());
                        xmlWriter.WriteElementString("CallForPrice", null, productVariant.CallForPrice.ToString());
                        xmlWriter.WriteElementString("Price", null, productVariant.Price.ToString());
                        xmlWriter.WriteElementString("OldPrice", null, productVariant.OldPrice.ToString());
                        xmlWriter.WriteElementString("ProductCost", null, productVariant.ProductCost.ToString());
                        xmlWriter.WriteElementString("SpecialPrice", null, productVariant.SpecialPrice.HasValue ? productVariant.SpecialPrice.ToString() : "");
                        xmlWriter.WriteElementString("SpecialPriceStartDateTimeUtc", null, productVariant.SpecialPriceStartDateTimeUtc.HasValue ? productVariant.SpecialPriceStartDateTimeUtc.ToString() : "");
                        xmlWriter.WriteElementString("SpecialPriceEndDateTimeUtc", null, productVariant.SpecialPriceEndDateTimeUtc.HasValue ? productVariant.SpecialPriceEndDateTimeUtc.ToString() : "");
                        xmlWriter.WriteElementString("CustomerEntersPrice", null, productVariant.CustomerEntersPrice.ToString());
                        xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, productVariant.MinimumCustomerEnteredPrice.ToString());
                        xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, productVariant.MaximumCustomerEnteredPrice.ToString());
                        xmlWriter.WriteElementString("Weight", null, productVariant.Weight.ToString());
                        xmlWriter.WriteElementString("Length", null, productVariant.Length.ToString());
                        xmlWriter.WriteElementString("Width", null, productVariant.Width.ToString());
                        xmlWriter.WriteElementString("Height", null, productVariant.Height.ToString());
                        xmlWriter.WriteElementString("PictureId", null, productVariant.PictureId.ToString());
                        xmlWriter.WriteElementString("Published", null, productVariant.Published.ToString());
                        xmlWriter.WriteElementString("Deleted", null, productVariant.Deleted.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productVariant.DisplayOrder.ToString());
                        xmlWriter.WriteElementString("CreatedOnUtc", null, productVariant.CreatedOnUtc.ToString());
                        xmlWriter.WriteElementString("UpdatedOnUtc", null, productVariant.UpdatedOnUtc.ToString());

                        xmlWriter.WriteStartElement("ProductDiscounts");
                        var discounts = productVariant.AppliedDiscounts;
                        foreach (var discount in discounts)
                        {
                            xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
                        }
                        xmlWriter.WriteEndElement();


                        xmlWriter.WriteStartElement("TierPrices");
                        var tierPrices = productVariant.TierPrices;
                        foreach (var tierPrice in tierPrices)
                        {
                            xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
                            xmlWriter.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
                            xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
                            xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("ProductAttributes");
                        var productVariantAttributes = productVariant.ProductVariantAttributes;
                        foreach (var productVariantAttribute in productVariantAttributes)
                        {
                            xmlWriter.WriteStartElement("ProductVariantAttribute");
                            xmlWriter.WriteElementString("ProductVariantAttributeId", null, productVariantAttribute.Id.ToString());
                            xmlWriter.WriteElementString("ProductAttributeId", null, productVariantAttribute.ProductAttributeId.ToString());
                            xmlWriter.WriteElementString("TextPrompt", null, productVariantAttribute.TextPrompt);
                            xmlWriter.WriteElementString("IsRequired", null, productVariantAttribute.IsRequired.ToString());
                            xmlWriter.WriteElementString("AttributeControlTypeId", null, productVariantAttribute.AttributeControlTypeId.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttribute.DisplayOrder.ToString());



                            xmlWriter.WriteStartElement("ProductVariantAttributeValues");
                            var productVariantAttributeValues = productVariantAttribute.ProductVariantAttributeValues;
                            foreach (var productVariantAttributeValue in productVariantAttributeValues)
                            {
                                xmlWriter.WriteElementString("ProductVariantAttributeValueId", null, productVariantAttributeValue.Id.ToString());
                                xmlWriter.WriteElementString("Name", null, productVariantAttributeValue.Name);
                                xmlWriter.WriteElementString("ColorSquaresRgb", null, productVariantAttributeValue.ColorSquaresRgb);
                                xmlWriter.WriteElementString("PriceAdjustment", null, productVariantAttributeValue.PriceAdjustment.ToString());
                                xmlWriter.WriteElementString("WeightAdjustment", null, productVariantAttributeValue.WeightAdjustment.ToString());
                                xmlWriter.WriteElementString("IsPreSelected", null, productVariantAttributeValue.IsPreSelected.ToString());
                                xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttributeValue.DisplayOrder.ToString());
                            }
                            xmlWriter.WriteEndElement();


                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                        
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductPictures");
                var productPictures = product.ProductPictures;
                foreach (var productPicture in productPictures)
                {
                    xmlWriter.WriteStartElement("ProductPicture");
                    xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
                    xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                
                xmlWriter.WriteStartElement("ProductCategories");
                var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                if (productCategories != null)
                {
                    foreach (var productCategory in productCategories)
                    {
                        xmlWriter.WriteStartElement("ProductCategory");
                        xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                        xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductManufacturers");
                var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
                if (productManufacturers != null)
                {
                    foreach (var productManufacturer in productManufacturers)
                    {
                        xmlWriter.WriteStartElement("ProductManufacturer");
                        xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                        xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                //add by hz
                xmlWriter.WriteStartElement("ProductVendors");
                var productVendors = _vendorService.GetProductVendorsByProductId(product.Id);
                if (productVendors != null)
                {
                    foreach (var productVendor in productVendors)
                    {
                        xmlWriter.WriteStartElement("ProductVendor");
                        xmlWriter.WriteElementString("ProductVendorId", null, productVendor.Id.ToString());
                        xmlWriter.WriteElementString("VendorId", null, productVendor.VendorId.ToString());
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productVendor.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productVendor.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();
                //end by hz

                xmlWriter.WriteStartElement("ProductSpecificationAttributes");
                var productSpecificationAttributes = product.ProductSpecificationAttributes;
                foreach (var productSpecificationAttribute in productSpecificationAttributes)
                {
                    xmlWriter.WriteStartElement("ProductSpecificationAttribute");
                    xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
                    xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());
                    xmlWriter.WriteElementString("CustomValue", null, productSpecificationAttribute.CustomValue);
                    xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
                    xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual void ExportProductsToXlsx(Stream stream, IList<Product> products, bool IsHzVendor)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Products");
                //Create Headers and format them 
                var properties = new string[]
                {
                    "Name",
                    "ShortDescription",
                    "FullDescription",
                    "ProductTemplateId",
                    "ShowOnHomePage",
                    "MetaKeywords",
                    "MetaDescription",
                    "MetaTitle",
                    "SeName",
                    "AllowCustomerReviews",
                    "Published",
                    "ProductVariantName",
                    "SKU",
                    "ManufacturerPartNumber",
                    "Gtin",
                    "IsGiftCard",
                    "GiftCardTypeId",
                    "RequireOtherProducts",
                    "RequiredProductVariantIds",
                    "AutomaticallyAddRequiredProductVariants",
                    "IsDownload",
                    "DownloadId",
                    "UnlimitedDownloads",
                    "MaxNumberOfDownloads",
                    "DownloadActivationTypeId",
                    "HasSampleDownload",
                    "SampleDownloadId",
                    "HasUserAgreement",
                    "UserAgreementText",
                    "IsRecurring",
                    "RecurringCycleLength",
                    "RecurringCyclePeriodId",
                    "RecurringTotalCycles",
                    "IsShipEnabled",
                    "IsFreeShipping",
                    "AdditionalShippingCharge",
                    "IsTaxExempt",
                    "TaxCategoryId",
                    "ManageInventoryMethodId",
                    "StockQuantity",
                    "DisplayStockAvailability",
                    "DisplayStockQuantity",
                    "MinStockQuantity",
                    "LowStockActivityId",
                    "NotifyAdminForQuantityBelow",
                    "BackorderModeId",
                    "AllowBackInStockSubscriptions",
                    "OrderMinimumQuantity",
                    "OrderMaximumQuantity",
                    "AllowedQuantities",
                    "DisableBuyButton",
                    "DisableWishlistButton",
                    "CallForPrice",
                    "Price",
                    "OldPrice",
                    "ProductCost",
                    "SpecialPrice",
                    "SpecialPriceStartDateTimeUtc",
                    "SpecialPriceEndDateTimeUtc",
                    "CustomerEntersPrice",
                    "MinimumCustomerEnteredPrice",
                    "MaximumCustomerEnteredPrice",
                    "Weight",
                    "Length",
                    "Width",
                    "Height",
                    "CreatedOnUtc",
                    "CategoryIds",
                    "ManufacturerIds",
                    "VendorIds",//add by hz
                    "Picture1",
                    "Picture2",
                    "Picture3",
                };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var p in products)
                {
                    var productVariants = _productService.GetProductVariantsByProductId(p.Id, true);
                    foreach (var pv in productVariants)
                    {
                        int col = 1;

                        worksheet.Cells[row, col].Value = p.Name;
                        col++;

                        worksheet.Cells[row, col].Value = p.ShortDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.FullDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.ProductTemplateId;
                        col++;

                        worksheet.Cells[row, col].Value = p.ShowOnHomePage;
                        col++;

                        worksheet.Cells[row, col].Value = p.MetaKeywords;
                        col++;

                        worksheet.Cells[row, col].Value = p.MetaDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.MetaTitle;
                        col++;

                        worksheet.Cells[row, col].Value = p.GetSeName(0);
                        col++;

                        worksheet.Cells[row, col].Value = p.AllowCustomerReviews;
                        col++;

                        worksheet.Cells[row, col].Value = p.Published;
                        col++;
                        
                        worksheet.Cells[row, col].Value = pv.Name;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Sku;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ManufacturerPartNumber;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Gtin;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsGiftCard;
                        col++;

                        worksheet.Cells[row, col].Value = pv.GiftCardTypeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RequireOtherProducts;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RequiredProductVariantIds;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AutomaticallyAddRequiredProductVariants;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsDownload;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DownloadId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.UnlimitedDownloads;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MaxNumberOfDownloads;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DownloadActivationTypeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HasSampleDownload;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SampleDownloadId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HasUserAgreement;
                        col++;

                        worksheet.Cells[row, col].Value = pv.UserAgreementText;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsRecurring;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringCycleLength;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringCyclePeriodId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringTotalCycles;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsShipEnabled;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsFreeShipping;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AdditionalShippingCharge;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsTaxExempt;
                        col++;

                        worksheet.Cells[row, col].Value = pv.TaxCategoryId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ManageInventoryMethodId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.StockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayStockAvailability;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayStockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MinStockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.LowStockActivityId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.NotifyAdminForQuantityBelow;
                        col++;

                        worksheet.Cells[row, col].Value = pv.BackorderModeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AllowBackInStockSubscriptions;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OrderMinimumQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OrderMaximumQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AllowedQuantities;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisableBuyButton;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisableWishlistButton;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CallForPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Price;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OldPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ProductCost;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPriceStartDateTimeUtc;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPriceEndDateTimeUtc;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CustomerEntersPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MinimumCustomerEnteredPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MaximumCustomerEnteredPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Weight;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Length;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Width;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Height;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CreatedOnUtc.ToOADate();
                        col++;

                        //category identifiers
                        string categoryIds = null;
                        foreach (var pc in _categoryService.GetProductCategoriesByProductId(p.Id))
                        {
                            categoryIds += pc.CategoryId;
                            categoryIds += ";";
                        }
                        worksheet.Cells[row, col].Value = categoryIds;
                        col++;

                        //manufacturer identifiers
                        string manufacturerIds = null;
                        foreach (var pm in _manufacturerService.GetProductManufacturersByProductId(p.Id))
                        {
                            manufacturerIds += pm.ManufacturerId;
                            manufacturerIds += ";";
                        }
                        worksheet.Cells[row, col].Value = manufacturerIds;
                        col++;

                        //add by hz
                        //vendor identifiers
                        string vendorIds = null;
                        foreach (var ps in _vendorService.GetProductVendorsByProductId(p.Id))
                        {
                            vendorIds += ps.VendorId;
                            vendorIds += ";";
                        }
                        worksheet.Cells[row, col].Value = vendorIds;
                        col++;
                        //end by hz

                        //pictures (up to 3 pictures)
                        string picture1 = null;
                        string picture2 = null;
                        string picture3 = null;
                        var pictures = _pictureService.GetPicturesByProductId(p.Id, 3);
                        for (int i = 0; i < pictures.Count; i++)
                        {
                            string pictureLocalPath = _pictureService.GetThumbLocalPath(pictures[i]);
                            switch (i)
                            {
                                case 0:
                                    picture1 = pictureLocalPath;
                                    break;
                                case 1:
                                    picture2 = pictureLocalPath;
                                    break;
                                case 2:
                                    picture3 = pictureLocalPath;
                                    break;
                            }
                        }
                        worksheet.Cells[row, col].Value = picture1;
                        col++;
                        worksheet.Cells[row, col].Value = picture2;
                        col++;
                        worksheet.Cells[row, col].Value = picture3;
                        col++;

                        row++;
                    }
                }








                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Products";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} products", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        #endregion
    }
}
