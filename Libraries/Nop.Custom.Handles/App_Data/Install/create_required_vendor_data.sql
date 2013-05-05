
SET IDENTITY_INSERT [dbo].[StoreTemplate] ON
INSERT [dbo].[StoreTemplate] ([Id], [Name], [ViewPath], [DisplayOrder]) VALUES (1, N'Products in Grid or Lines', N'StoreTemplate.ProductsInGridOrLines', 1)
SET IDENTITY_INSERT [dbo].[StoreTemplate] OFF
GO



INSERT [dbo].[ActivityLogType] ( [SystemKeyword], [Name], [Enabled]) VALUES (N'AddNewStore', N'Add a new store', 1)
INSERT [dbo].[ActivityLogType] ([SystemKeyword], [Name], [Enabled]) VALUES (N'DeleteStore', N'Delete a store', 1)
INSERT [dbo].[ActivityLogType] ([SystemKeyword], [Name], [Enabled]) VALUES (N'EditStore', N'Edit a store', 1)
INSERT [dbo].[ActivityLogType] ([SystemKeyword], [Name], [Enabled]) VALUES (N'PublicStore.ViewStore', N'Public store. View a store', 0)

GO


INSERT [dbo].[Setting] ([Name], [Value]) VALUES (N'commonsettings.sitemapincludestores', N'True')
INSERT [dbo].[Setting] ([Name], [Value]) VALUES (N'catalogsettings.defaultstorepagesizeoptions', N'4, 2, 8, 12')
INSERT [dbo].[Setting] ([Name], [Value]) VALUES (N'catalogsettings.storesblockitemstodisplay', N'5')
INSERT [dbo].[Setting] ([Name], [Value]) VALUES (N'mediasettings.storethumbpicturesize', N'125')

GO

