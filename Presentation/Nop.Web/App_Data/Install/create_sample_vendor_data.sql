
SET IDENTITY_INSERT [dbo].[Vendor] ON
INSERT [dbo].[Vendor] ([Id], [Name], [Description], [VendorTemplateId], [MetaKeywords], [MetaDescription], [MetaTitle], [PictureId], [PageSize], [AllowCustomersToSelectPageSize], [PageSizeOptions], [PriceRanges], [SubjectToAcl], [Published], [Deleted], [DisplayOrder], [CreatedOnUtc], [UpdatedOnUtc]) VALUES (1, N'Main Vendor', NULL, 1, NULL, NULL, NULL, 0, 4, 1, N'4, 2, 8, 12', NULL, 0, 1, 0, 2, CAST(0x0000A14100B7396C AS DateTime), CAST(0x0000A14100B7396C AS DateTime))
SET IDENTITY_INSERT [dbo].[Vendor] OFF
GO


SET IDENTITY_INSERT [dbo].[Product_Vendor_Mapping] ON
INSERT [dbo].[Product_Vendor_Mapping] ([Id], [ProductId], [VendorId], [IsFeaturedProduct], [DisplayOrder]) VALUES (1, 11, 1, 0, 2)
INSERT [dbo].[Product_Vendor_Mapping] ([Id], [ProductId], [VendorId], [IsFeaturedProduct], [DisplayOrder]) VALUES (2, 12, 1, 0, 1)
INSERT [dbo].[Product_Vendor_Mapping] ([Id], [ProductId], [VendorId], [IsFeaturedProduct], [DisplayOrder]) VALUES (3, 28, 2, 0, 1)
INSERT [dbo].[Product_Vendor_Mapping] ([Id], [ProductId], [VendorId], [IsFeaturedProduct], [DisplayOrder]) VALUES (4, 29, 2, 0, 2)
INSERT [dbo].[Product_Vendor_Mapping] ([Id], [ProductId], [VendorId], [IsFeaturedProduct], [DisplayOrder]) VALUES (5, 30, 2, 0, 3)
INSERT [dbo].[Product_Vendor_Mapping] ([Id], [ProductId], [VendorId], [IsFeaturedProduct], [DisplayOrder]) VALUES (6, 31, 2, 0, 4)
SET IDENTITY_INSERT [dbo].[Product_Vendor_Mapping] OFF
GO




SET IDENTITY_INSERT [dbo].[UrlRecord] ON

INSERT [dbo].[UrlRecord] ([Id], [EntityId], [EntityName], [Slug], [IsActive], [LanguageId]) VALUES (19, 1, N'Vendor', N'Main Vendor', 1, 0)

SET IDENTITY_INSERT [dbo].[UrlRecord] OFF
GO


INSERT [dbo].[CustomerRole] ([Name], [FreeShipping], [TaxExempt], [Active], [IsSystemRole], [SystemName]) VALUES (N'Vendor Manager', 0, 0, 1, 1, N'VendorManagers')
GO


INSERT [dbo].[Address] ([FirstName], [LastName], [Email], [Company], [CountryId], [StateProvinceId], [City], [Address1], [Address2], [ZipPostalCode], [PhoneNumber], [FaxNumber], [CreatedOnUtc]) VALUES (1, N'David', N'Bland', N'1@vendor.com', N'Multi Vendor', 1, 40, N'New York', N'21 West 52nd Street', N'', N'10021', N'12345678', N'', CAST(0x0000A14100B723A3 AS DateTime))
GO

INSERT [dbo].[Customer] ([CustomerGuid], [Username], [Email], [Password], [PasswordFormatId], [PasswordSalt], [AdminComment], [LanguageId], [CurrencyId], [TaxDisplayTypeId], [IsTaxExempt], [VatNumber], [VatNumberStatusId], [SelectedPaymentMethodSystemName], [CheckoutAttributes], [DiscountCouponCode], [GiftCardCouponCodes], [UseRewardPointsDuringCheckout], [TimeZoneId], [AffiliateId], [Active], [Deleted], [IsSystemAccount], [SystemName], [LastIpAddress], [CreatedOnUtc], [LastLoginDateUtc], [LastActivityDateUtc], [BillingAddress_Id], [ShippingAddress_Id]) VALUES (1, N'28693780-39CA-4766-9E3C-2E4AB089B235', N'1@Vendor.com', N'1@Vendor.com', N'1234', 0, N'auRbEq4=', NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, NULL, 0, NULL, NULL, 1, 0, 0, NULL, NULL, CAST(0x0000A14100B72305 AS DateTime), NULL, CAST(0x0000A14100B72306 AS DateTime), 1, 1)
GO


INSERT [dbo].[CustomerAddresses] ([Customer_Id], [Address_Id]) VALUES (3, 2)
GO


INSERT [dbo].[Customer_CustomerRole_Mapping] ([Customer_Id], [CustomerRole_Id]) VALUES (3, 3)
INSERT [dbo].[Customer_CustomerRole_Mapping] ([Customer_Id], [CustomerRole_Id]) VALUES (3, 5)
GO