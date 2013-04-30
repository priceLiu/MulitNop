//add by hz full page
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService : IProductService
    {
        
        #region Methods

        #region Products

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>//add by hz
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product collection</returns>
        public virtual IPagedList<Product> SearchProducts(int categoryId, int manufacturerId,
            int vendorId, //add by hz
            bool? featuredProducts,
            decimal? priceMin, decimal? priceMax, int productTagId,
            string keywords, bool searchDescriptions, bool searchProductTags, int languageId,
            IList<int> filteredSpecs, ProductSortingEnum orderBy,
            int pageIndex, int pageSize,
            bool loadFilterableSpecificationAttributeOptionIds, out IList<int> filterableSpecificationAttributeOptionIds,
            bool showHidden = false)
        {
            var categoryIds = new List<int>();
            if (categoryId > 0)
                categoryIds.Add(categoryId);

            return SearchProducts(categoryIds, manufacturerId,
                vendorId, //add by hz
                featuredProducts,
                priceMin, priceMax, productTagId, 
                keywords, searchDescriptions,searchProductTags, languageId,
                filteredSpecs, orderBy, pageIndex, pageSize,
                loadFilterableSpecificationAttributeOptionIds, out filterableSpecificationAttributeOptionIds, 
                showHidden);
        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>//add by hz
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product collection</returns>
        public virtual IPagedList<Product> SearchProducts(IList<int> categoryIds, 
            int manufacturerId, 
            int vendorId, //add by hz
            bool? featuredProducts,
            decimal? priceMin, decimal? priceMax, int productTagId,
            string keywords, bool searchDescriptions, bool searchProductTags, int languageId,
            IList<int> filteredSpecs, ProductSortingEnum orderBy,
            int pageIndex, int pageSize,
            bool loadFilterableSpecificationAttributeOptionIds, out IList<int> filterableSpecificationAttributeOptionIds,
            bool showHidden = false)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages(false).Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
            

            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //vendord procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                #region Use vendord procedure
                
                //pass category identifiers as comma-delimited string
                string commaSeparatedCategoryIds = "";
                if (categoryIds != null)
                {
                    for (int i = 0; i < categoryIds.Count; i++)
                    {
                        commaSeparatedCategoryIds += categoryIds[i].ToString();
                        if (i != categoryIds.Count - 1)
                        {
                            commaSeparatedCategoryIds += ",";
                        }
                    }
                }


                //pass customer role identifiers as comma-delimited string
                string commaSeparatedAllowedCustomerRoleIds = "";
                for (int i = 0; i < allowedCustomerRolesIds.Count; i++)
                {
                    commaSeparatedAllowedCustomerRoleIds += allowedCustomerRolesIds[i].ToString();
                    if (i != allowedCustomerRolesIds.Count - 1)
                    {
                        commaSeparatedAllowedCustomerRoleIds += ",";
                    }
                }


                //pass specification identifiers as comma-delimited string
                string commaSeparatedSpecIds = "";
                if (filteredSpecs != null)
                {
                    ((List<int>)filteredSpecs).Sort();
                    for (int i = 0; i < filteredSpecs.Count; i++)
                    {
                        commaSeparatedSpecIds += filteredSpecs[i].ToString();
                        if (i != filteredSpecs.Count - 1)
                        {
                            commaSeparatedSpecIds += ",";
                        }
                    }
                }

                //some databases don't support int.MaxValue
                if (pageSize == int.MaxValue)
                    pageSize = int.MaxValue - 1;
                
                //prepare parameters
                var pCategoryIds = _dataProvider.GetParameter();
                pCategoryIds.ParameterName = "CategoryIds";
                pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
                pCategoryIds.DbType = DbType.String;
                
                var pManufacturerId = _dataProvider.GetParameter();
                pManufacturerId.ParameterName = "ManufacturerId";
                pManufacturerId.Value = manufacturerId;
                pManufacturerId.DbType = DbType.Int32;

                //add by hz
                var pVendorId = _dataProvider.GetParameter();
                pVendorId.ParameterName = "VendorId";
                pVendorId.Value = vendorId;
                pVendorId.DbType = DbType.Int32;
                //end by hz

                var pProductTagId = _dataProvider.GetParameter();
                pProductTagId.ParameterName = "ProductTagId";
                pProductTagId.Value = productTagId;
                pProductTagId.DbType = DbType.Int32;

                var pFeaturedProducts = _dataProvider.GetParameter();
                pFeaturedProducts.ParameterName = "FeaturedProducts";
                pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
                pFeaturedProducts.DbType = DbType.Boolean;

                var pPriceMin = _dataProvider.GetParameter();
                pPriceMin.ParameterName = "PriceMin";
                pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
                pPriceMin.DbType = DbType.Decimal;
                
                var pPriceMax = _dataProvider.GetParameter();
                pPriceMax.ParameterName = "PriceMax";
                pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
                pPriceMax.DbType = DbType.Decimal;

                var pKeywords = _dataProvider.GetParameter();
                pKeywords.ParameterName = "Keywords";
                pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
                pKeywords.DbType = DbType.String;

                var pSearchDescriptions = _dataProvider.GetParameter();
                pSearchDescriptions.ParameterName = "SearchDescriptions";
                pSearchDescriptions.Value = searchDescriptions;
                pSearchDescriptions.DbType = DbType.Boolean;

                var pSearchProductTags = _dataProvider.GetParameter();
                pSearchProductTags.ParameterName = "SearchProductTags";
                pSearchProductTags.Value = searchProductTags;
                pSearchProductTags.DbType = DbType.Boolean;

                var pUseFullTextSearch = _dataProvider.GetParameter();
                pUseFullTextSearch.ParameterName = "UseFullTextSearch";
                pUseFullTextSearch.Value = _commonSettings.UseFullTextSearch;
                pUseFullTextSearch.DbType = DbType.Boolean;

                var pFullTextMode = _dataProvider.GetParameter();
                pFullTextMode.ParameterName = "FullTextMode";
                pFullTextMode.Value = (int)_commonSettings.FullTextMode;
                pFullTextMode.DbType = DbType.Int32;

                var pFilteredSpecs = _dataProvider.GetParameter();
                pFilteredSpecs.ParameterName = "FilteredSpecs";
                pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
                pFilteredSpecs.DbType = DbType.String;

                var pLanguageId = _dataProvider.GetParameter();
                pLanguageId.ParameterName = "LanguageId";
                pLanguageId.Value = searchLocalizedValue ? languageId : 0;
                pLanguageId.DbType = DbType.Int32;

                var pOrderBy = _dataProvider.GetParameter();
                pOrderBy.ParameterName = "OrderBy";
                pOrderBy.Value = (int)orderBy;
                pOrderBy.DbType = DbType.Int32;

                var pPageIndex = _dataProvider.GetParameter();
                pPageIndex.ParameterName = "PageIndex";
                pPageIndex.Value = pageIndex;
                pPageIndex.DbType = DbType.Int32;

                var pPageSize = _dataProvider.GetParameter();
                pPageSize.ParameterName = "PageSize";
                pPageSize.Value = pageSize;
                pPageSize.DbType = DbType.Int32;

                var pAllowedCustomerRoleIds= _dataProvider.GetParameter();
                pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
                pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
                pAllowedCustomerRoleIds.DbType = DbType.String;

                var pShowHidden = _dataProvider.GetParameter();
                pShowHidden.ParameterName = "ShowHidden";
                pShowHidden.Value = showHidden;
                pShowHidden.DbType = DbType.Boolean;
                
                var pLoadFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pLoadFilterableSpecificationAttributeOptionIds.ParameterName = "LoadFilterableSpecificationAttributeOptionIds";
                pLoadFilterableSpecificationAttributeOptionIds.Value = loadFilterableSpecificationAttributeOptionIds;
                pLoadFilterableSpecificationAttributeOptionIds.DbType = DbType.Boolean;
                
                var pFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
                pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
                pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
                pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

                var pTotalRecords = _dataProvider.GetParameter();
                pTotalRecords.ParameterName = "TotalRecords";
                pTotalRecords.Direction = ParameterDirection.Output;
                pTotalRecords.DbType = DbType.Int32;

                //invoke vendord procedure
                var products = _dbContext.ExecuteStoredProcedureList<Product>(
                    "ProductLoadAllPagedHzVendor",
                    pCategoryIds,
                    pManufacturerId,
                    pVendorId, //add by hz
                    pProductTagId,
                    pFeaturedProducts,
                    pPriceMin,
                    pPriceMax,
                    pKeywords,
                    pSearchDescriptions,
                    pSearchProductTags,
                    pUseFullTextSearch,
                    pFullTextMode,
                    pFilteredSpecs,
                    pLanguageId,
                    pOrderBy,
                    pPageIndex,
                    pPageSize,
                    pAllowedCustomerRoleIds,
                    pShowHidden,
                    pLoadFilterableSpecificationAttributeOptionIds,
                    pFilterableSpecificationAttributeOptionIds,
                    pTotalRecords);
                //get filterable specification attribute option identifier
                string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
                if (loadFilterableSpecificationAttributeOptionIds &&
                    !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
                {
                     filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x.Trim()))
                        .ToList();
                }
                //return products
                int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
                return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

                #endregion
            }
            else
            {
                //vendord procedures aren't supported. Use LINQ

                #region Search products

                //products
                var query = _productRepository.Table;
                query = query.Where(p => !p.Deleted);
                if (!showHidden)
                {
                    query = query.Where(p => p.Published);
                }

                //searching by keyword
                if (!String.IsNullOrWhiteSpace(keywords))
                {
                    query = from p in query
                            join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
                            from lp in p_lp.DefaultIfEmpty()
                            from pv in p.ProductVariants.DefaultIfEmpty()
                            from pt in p.ProductTags.DefaultIfEmpty()
                            where (p.Name.Contains(keywords)) ||
                                  (searchDescriptions && p.ShortDescription.Contains(keywords)) ||
                                  (searchDescriptions && p.FullDescription.Contains(keywords)) ||
                                  (pv.Name.Contains(keywords)) ||
                                  (searchDescriptions && pv.Description.Contains(keywords)) ||
                                  (searchProductTags && pt.Name.Contains(keywords)) ||
                                  //localized values
                                  (searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.Contains(keywords)) ||
                                  (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.Contains(keywords)) ||
                                  (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" && lp.LocaleValue.Contains(keywords))
                                  //UNDONE search localized values in associated product tags
                            select p;
                }

                //ACL
                if (!showHidden)
                {
                    query = from p in query
                            join acl in _aclRepository.Table on p.Id equals acl.EntityId into p_acl
                            from acl in p_acl.DefaultIfEmpty()
                            where !p.SubjectToAcl || (acl.EntityName == "Product" && allowedCustomerRolesIds.Contains(acl.CustomerRoleId))
                            select p;
                }

                //product variants
                //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                //That's why we pass the date value
                var nowUtc = DateTime.UtcNow;
                query = from p in query
                        from pv in p.ProductVariants.DefaultIfEmpty()
                        where
                            //deleted
                            (showHidden || !pv.Deleted) &&
                            //published
                            (showHidden || pv.Published) &&
                            //price min
                            (
                                !priceMin.HasValue 
                                ||
                                //special price (specified price and valid date range)
                                ((pv.SpecialPrice.HasValue && ((!pv.SpecialPriceStartDateTimeUtc.HasValue || pv.SpecialPriceStartDateTimeUtc.Value < nowUtc) && (!pv.SpecialPriceEndDateTimeUtc.HasValue || pv.SpecialPriceEndDateTimeUtc.Value > nowUtc))) && (pv.SpecialPrice >= priceMin.Value))
                                ||
                                //regular price (price isn't specified or date range isn't valid)
                                ((!pv.SpecialPrice.HasValue || ((pv.SpecialPriceStartDateTimeUtc.HasValue && pv.SpecialPriceStartDateTimeUtc.Value > nowUtc) || (pv.SpecialPriceEndDateTimeUtc.HasValue && pv.SpecialPriceEndDateTimeUtc.Value < nowUtc))) && (pv.Price >= priceMin.Value))
                            ) &&
                            //price max
                            (
                                !priceMax.HasValue 
                                ||
                                //special price (specified price and valid date range)
                                ((pv.SpecialPrice.HasValue && ((!pv.SpecialPriceStartDateTimeUtc.HasValue || pv.SpecialPriceStartDateTimeUtc.Value < nowUtc) && (!pv.SpecialPriceEndDateTimeUtc.HasValue || pv.SpecialPriceEndDateTimeUtc.Value > nowUtc))) && (pv.SpecialPrice <= priceMax.Value))
                                ||
                                //regular price (price isn't specified or date range isn't valid)
                                ((!pv.SpecialPrice.HasValue || ((pv.SpecialPriceStartDateTimeUtc.HasValue && pv.SpecialPriceStartDateTimeUtc.Value > nowUtc) || (pv.SpecialPriceEndDateTimeUtc.HasValue && pv.SpecialPriceEndDateTimeUtc.Value < nowUtc))) && (pv.Price <= priceMax.Value))
                            ) &&
                            //available dates
                            (showHidden || (!pv.AvailableStartDateTimeUtc.HasValue || pv.AvailableStartDateTimeUtc.Value < nowUtc)) &&
                            (showHidden || (!pv.AvailableEndDateTimeUtc.HasValue || pv.AvailableEndDateTimeUtc.Value > nowUtc))
                        select p;


                //search by specs
                if (filteredSpecs != null && filteredSpecs.Count > 0)
                {
                    query = from p in query
                            where !filteredSpecs
                                       .Except(
                                           p.ProductSpecificationAttributes.Where(psa => psa.AllowFiltering).Select(
                                               psa => psa.SpecificationAttributeOptionId))
                                       .Any()
                            select p;
                }

                //category filtering
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    //search in subcategories
                    query = from p in query
                            from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                            where (!featuredProducts.HasValue || featuredProducts.Value == pc.IsFeaturedProduct)
                            select p;
                }

                //manufacturer filtering
                if (manufacturerId > 0)
                {
                    query = from p in query
                            from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                            where (!featuredProducts.HasValue || featuredProducts.Value == pm.IsFeaturedProduct)
                            select p;
                }

                //add by hz
                //vendor filtering
                if (vendorId > 0)
                {
                    query = from p in query
                            from ps in p.ProductVendors.Where(ps => ps.VendorId == vendorId)
                            where (!featuredProducts.HasValue || featuredProducts.Value == ps.IsFeaturedProduct)
                            select p;
                }
                //end by hz

                //related products filtering
                //if (relatedToProductId > 0)
                //{
                //    query = from p in query
                //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
                //            where (relatedToProductId == rp.ProductId1)
                //            select p;
                //}

                //tag filtering
                if (productTagId > 0)
                {
                    query = from p in query
                            from pt in p.ProductTags.Where(pt => pt.Id == productTagId)
                            select p;
                }

                //only distinct products (group by ID)
                //if we use standard Distinct() method, then all fields will be compared (low performance)
                //it'll not work in SQL Server Compact when searching products by a keyword)
                query = from p in query
                        group p by p.Id
                        into pGroup
                        orderby pGroup.Key
                        select pGroup.FirstOrDefault();

                //sort products
                if (orderBy == ProductSortingEnum.Position && categoryIds != null && categoryIds.Count > 0)
                {
                    //category position
                    var firstCategoryId = categoryIds[0];
                    query = query.OrderBy(p => p.ProductCategories.Where(pc => pc.CategoryId == firstCategoryId).FirstOrDefault().DisplayOrder);
                }
                else if (orderBy == ProductSortingEnum.Position && manufacturerId > 0)
                {
                    //manufacturer position
                    query =
                        query.OrderBy(p => p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId).FirstOrDefault().DisplayOrder);
                }
                //add by hz
                else if (orderBy == ProductSortingEnum.Position && vendorId > 0)
                {
                    //vendor position
                    query =
                        query.OrderBy(p => p.ProductVendors.Where(ps => ps.VendorId == vendorId).FirstOrDefault().DisplayOrder);
                }
                //end by hz
                //else if (orderBy == ProductSortingEnum.Position && relatedToProductId > 0)
                //{
                //    //sort by related product display order
                //    query = from p in query
                //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
                //            where (relatedToProductId == rp.ProductId1)
                //            orderby rp.DisplayOrder
                //            select p;
                //}
                else if (orderBy == ProductSortingEnum.Position)
                {
                    //sort by name (there's no any position if category or manufactur is not specified)
                    query = query.OrderBy(p => p.Name);
                }
                else if (orderBy == ProductSortingEnum.NameAsc)
                {
                    //Name: A to Z
                    query = query.OrderBy(p => p.Name);
                }
                else if (orderBy == ProductSortingEnum.NameDesc)
                {
                    //Name: Z to A
                    query = query.OrderByDescending(p => p.Name);
                }
                else if (orderBy == ProductSortingEnum.PriceAsc)
                {
                    //Price: Low to High
                    query = query.OrderBy(p => p.ProductVariants.FirstOrDefault().Price);
                }
                else if (orderBy == ProductSortingEnum.PriceDesc)
                {
                    //Price: High to Low
                    query = query.OrderByDescending(p => p.ProductVariants.FirstOrDefault().Price);
                }
                else if (orderBy == ProductSortingEnum.CreatedOn)
                {
                    //creation date
                    query = query.OrderByDescending(p => p.CreatedOnUtc);
                }
                else
                {
                    //actually this code is not reachable
                    query = query.OrderBy(p => p.Name);
                }

                var products = new PagedList<Product>(query, pageIndex, pageSize);

                //get filterable specification attribute option identifier
                if (loadFilterableSpecificationAttributeOptionIds)
                {
                    var querySpecs = from p in query
                                     join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                     where psa.AllowFiltering
                                     select psa.SpecificationAttributeOptionId;
                    //only distinct attributes
                    filterableSpecificationAttributeOptionIds = querySpecs
                        .Distinct()
                        .ToList();
                }

                //return products
                return products;

                #endregion
            }
        }

       
        #endregion

        #region Product variants

        /// <summary>
        /// Search product variants
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>//add by hz
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search in descriptions</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variants</returns>
        public virtual IPagedList<ProductVariant> SearchProductVariants(int categoryId, int manufacturerId,
            int vendorId, //add by hz
             string keywords, bool searchDescriptions, int pageIndex, int pageSize, bool showHidden = false)
        {
            //products
            var query = _productVariantRepository.Table;
            query = query.Where(pv => !pv.Deleted);
            if (!showHidden)
            {
                query = query.Where(pv => pv.Published);
            }
            query = query.Where(pv => !pv.Product.Deleted);
            if (!showHidden)
            {
                query = query.Where(pv => pv.Product.Published);
            }

            //searching by keyword
            if (!String.IsNullOrWhiteSpace(keywords))
            {
                query = from pv in query
                        where (pv.Product.Name.Contains(keywords)) ||
                        (searchDescriptions && pv.Product.ShortDescription.Contains(keywords)) ||
                        (searchDescriptions && pv.Product.FullDescription.Contains(keywords)) ||
                        (pv.Name.Contains(keywords)) ||
                        (searchDescriptions && pv.Description.Contains(keywords))
                        select pv;
            }

            //category filtering
            if (categoryId > 0)
            {
                query = from pv in query
                        from pc in pv.Product.ProductCategories.Where(pc => pc.CategoryId == categoryId)
                        select pv;
            }

            //manufacturer filtering
            if (manufacturerId > 0)
            {
                query = from pv in query
                        from pm in pv.Product.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                        select pv;
            }

            //add by hz
            //vendor filtering
            if (vendorId > 0)
            {
                query = from pv in query
                        from ps in pv.Product.ProductVendors.Where(ps => ps.VendorId == vendorId)
                        select pv;
            }
            //end by hz

            //only distinct products (group by ID)
            //if we use standard Distinct() method, then all fields will be compared (low performance)
            //it'll not work in SQL Server Compact when searching products by a keyword)
            query = from pv in query
                    group pv by pv.Id into pvGroup
                    orderby pvGroup.Key
                    select pvGroup.FirstOrDefault();

            query = query.OrderBy(pv => pv.Product.Name).ThenBy(pv => pv.DisplayOrder);
            var productVariants = new PagedList<ProductVariant>(query, pageIndex, pageSize);
            return productVariants;
        }

        #endregion

        #endregion
    }
}
