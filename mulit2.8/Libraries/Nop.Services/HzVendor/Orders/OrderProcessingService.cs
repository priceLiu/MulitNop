using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order processing service
    /// </summary>
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields
        private readonly IVendorService _vendorService; //add by hz
        

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderService">Order service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="productService">Product service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="orderTotalCalculationService">Order total calculationservice</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="productAttributeFormatter">Product attribute formatter</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="shoppingCartService">Shopping cart service</param>
        /// <param name="checkoutAttributeFormatter">Checkout attribute service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shipmentService">Shipment service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="smsService">SMS service</param>
        /// <param name="customerActivityService">Customer activity service</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="orderSettings">Order settings</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="currencySettings">Currency settings</param>
        public OrderProcessingService(IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            ICustomerActivityService customerActivityService,
            ICurrencyService currencyService,
            IEventPublisher eventPublisher,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            CurrencySettings currencySettings
             , IVendorService vendorService //add by hz
            )
        {
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._productService = productService;
            this._paymentService = paymentService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._giftCardService = giftCardService;
            this._shoppingCartService = shoppingCartService;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._taxService = taxService;
            this._customerService = customerService;
            this._discountService = discountService;
            this._encryptionService = encryptionService;
            this._customerActivityService = customerActivityService;
            this._currencyService = currencyService;
            this._eventPublisher = eventPublisher;
            this._paymentSettings = paymentSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._localizationSettings = localizationSettings;
            this._currencySettings = currencySettings;
            this._vendorService = vendorService; //add by hz
        }

        #endregion

        #region Utilities

 
        #endregion

        #region Methods

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual PlaceOrderResult PlaceOrder(VendorOrder vendorOrder)
        {
            //think about moving functionality of processing recurring orders (after the initial order was placed) to ProcessNextRecurringPayment() method
            if (vendorOrder.processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            if (vendorOrder.processPaymentRequest.OrderGuid == Guid.Empty)
                vendorOrder.processPaymentRequest.OrderGuid = Guid.NewGuid();

            var result = new PlaceOrderResult();
            try
            {
                #region Order details (customer, addresses, totals)

                //Recurring orders. Load initial order
                Order initialOrder = _orderService.GetOrderById(vendorOrder.processPaymentRequest.InitialOrderId);
                if (vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    if (initialOrder == null)
                        throw new ArgumentException("Initial order is not set for recurring payment");

                    vendorOrder.processPaymentRequest.PaymentMethodSystemName = initialOrder.PaymentMethodSystemName;
                }

                //customer
                var customer = _customerService.GetCustomerById(vendorOrder.processPaymentRequest.CustomerId);
                if (customer == null)
                    throw new ArgumentException("Customer is not set");

                //customer currency
                string customerCurrencyCode = "";
                decimal customerCurrencyRate = decimal.Zero;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    var customerCurrency = (customer.Currency != null && customer.Currency.Published) ? customer.Currency : _workContext.WorkingCurrency;
                    customerCurrencyCode = customerCurrency.CurrencyCode;
                    var primaryVendorCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                    customerCurrencyRate = customerCurrency.Rate / primaryVendorCurrency.Rate;
                }
                else
                {
                    customerCurrencyCode = initialOrder.CustomerCurrencyCode;
                    customerCurrencyRate = initialOrder.CurrencyRate;
                }
                //customer language
                Language customerLanguage = null;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                    customerLanguage = customer.Language;
                else
                    customerLanguage = _languageService.GetLanguageById(initialOrder.CustomerLanguageId);
                if (customerLanguage == null || !customerLanguage.Published)
                    customerLanguage = _workContext.WorkingLanguage;

                //check whether customer is guest
                if (customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new NopException("Anonymous checkout is not allowed");

                //billing address
                Address billingAddress = null;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    if (customer.BillingAddress == null)
                        throw new NopException("Billing address is not provided");

                    if (!CommonHelper.IsValidEmail(customer.BillingAddress.Email))
                        throw new NopException("Email is not valid");

                    //clone billing address
                    billingAddress = (Address)customer.BillingAddress.Clone();
                    if (billingAddress.Country != null && !billingAddress.Country.AllowsBilling)
                        throw new NopException(string.Format("Country '{0}' is not allowed for billing", billingAddress.Country.Name));
                }
                else
                {
                    if (initialOrder.BillingAddress == null)
                        throw new NopException("Billing address is not available");

                    //clone billing address
                    billingAddress = (Address)initialOrder.BillingAddress.Clone();
                    if (billingAddress.Country != null && !billingAddress.Country.AllowsBilling)
                        throw new NopException(string.Format("Country '{0}' is not allowed for billing", billingAddress.Country.Name));
                }

                //load and validate customer shopping cart
                IList<ShoppingCartItem> cart = null;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    //load shopping cart
                    cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart
                                                            && _vendorService.GetVendorIdByProductId( sci.ProductVariant.Product.Id) == vendorOrder.vendor.Id
                                                            ).ToList();

                    if (cart.Count == 0)
                        throw new NopException("Cart is empty");

                    //validate the entire shopping cart
                    var warnings = _shoppingCartService.GetShoppingCartWarnings(cart, customer.CheckoutAttributes, true);
                    if (warnings.Count > 0)
                    {
                        var warningsSb = new StringBuilder();
                        foreach (string warning in warnings)
                        {
                            warningsSb.Append(warning);
                            warningsSb.Append(";");
                        }
                        throw new NopException(warningsSb.ToString());
                    }

                    //validate individual cart items
                    foreach (var sci in cart)
                    {
                        var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(customer, sci.ShoppingCartType,
                            sci.ProductVariant, sci.AttributesXml,
                            sci.CustomerEnteredPrice, sci.Quantity, false);
                        if (sciWarnings.Count > 0)
                        {
                            var warningsSb = new StringBuilder();
                            foreach (string warning in sciWarnings)
                            {
                                warningsSb.Append(warning);
                                warningsSb.Append(";");
                            }
                            throw new NopException(warningsSb.ToString());
                        }
                    }
                }

                //min totals validation
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    bool minOrderSubtotalAmountOk = ValidateMinOrderSubtotalAmount(cart);
                    if (!minOrderSubtotalAmountOk)
                    {
                        decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                        throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
                    }
                    bool minOrderTotalAmountOk = ValidateMinOrderTotalAmount(cart);
                    if (!minOrderTotalAmountOk)
                    {
                        decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                        throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
                    }
                }
                //tax display type
                var customerTaxDisplayType = TaxDisplayType.IncludingTax;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                        customerTaxDisplayType = customer.TaxDisplayType;
                    else
                        customerTaxDisplayType = _taxSettings.TaxDisplayType;
                }
                else
                {
                    customerTaxDisplayType = initialOrder.CustomerTaxDisplayType;
                }

                //checkout attributes
                string checkoutAttributeDescription, checkoutAttributesXml;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    checkoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(customer.CheckoutAttributes, customer);
                    checkoutAttributesXml = customer.CheckoutAttributes;
                }
                else
                {
                    checkoutAttributeDescription = initialOrder.CheckoutAttributeDescription;
                    checkoutAttributesXml = initialOrder.CheckoutAttributesXml;
                }

                //applied discount (used to vendor discount usage history)
                var appliedDiscounts = new List<Discount>();

                //sub total
                decimal orderSubTotalInclTax, orderSubTotalExclTax;
                decimal orderSubTotalDiscountInclTax = 0, orderSubTotalDiscountExclTax = 0;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    //sub total (incl tax)
                    decimal orderSubTotalDiscountAmount1 = decimal.Zero;
                    Discount orderSubTotalAppliedDiscount1 = null;
                    decimal subTotalWithoutDiscountBase1 = decimal.Zero;
                    decimal subTotalWithDiscountBase1 = decimal.Zero;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                        true, out orderSubTotalDiscountAmount1, out orderSubTotalAppliedDiscount1,
                        out subTotalWithoutDiscountBase1, out subTotalWithDiscountBase1);
                    orderSubTotalInclTax = subTotalWithoutDiscountBase1;
                    orderSubTotalDiscountInclTax = orderSubTotalDiscountAmount1;

                    //discount history
                    if (orderSubTotalAppliedDiscount1 != null && !appliedDiscounts.ContainsDiscount(orderSubTotalAppliedDiscount1))
                        appliedDiscounts.Add(orderSubTotalAppliedDiscount1);

                    //sub total (excl tax)
                    decimal orderSubTotalDiscountAmount2 = decimal.Zero;
                    Discount orderSubTotalAppliedDiscount2 = null;
                    decimal subTotalWithoutDiscountBase2 = decimal.Zero;
                    decimal subTotalWithDiscountBase2 = decimal.Zero;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                        false, out orderSubTotalDiscountAmount2, out orderSubTotalAppliedDiscount2,
                        out subTotalWithoutDiscountBase2, out subTotalWithDiscountBase2);
                    orderSubTotalExclTax = subTotalWithoutDiscountBase2;
                    orderSubTotalDiscountExclTax = orderSubTotalDiscountAmount2;
                }
                else
                {
                    orderSubTotalInclTax = initialOrder.OrderSubtotalInclTax;
                    orderSubTotalExclTax = initialOrder.OrderSubtotalExclTax;
                }


                //shipping info
                bool shoppingCartRequiresShipping = false;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    shoppingCartRequiresShipping = cart.RequiresShipping();
                }
                else
                {
                    shoppingCartRequiresShipping = initialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
                }
                Address shippingAddress = null;
                string shippingMethodName = "", shippingRateComputationMethodSystemName = "";
                if (shoppingCartRequiresShipping)
                {
                    if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                    {
                        if (customer.ShippingAddress == null)
                            throw new NopException("Shipping address is not provided");

                        if (!CommonHelper.IsValidEmail(customer.ShippingAddress.Email))
                            throw new NopException("Email is not valid");

                        //clone shipping address
                        shippingAddress = (Address)customer.ShippingAddress.Clone();
                        if (shippingAddress.Country != null && !shippingAddress.Country.AllowsShipping)
                            throw new NopException(string.Format("Country '{0}' is not allowed for shipping", shippingAddress.Country.Name));

                        var shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.LastShippingOption);
                        if (shippingOption != null)
                        {
                            shippingMethodName = shippingOption.Name;
                            shippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                        }
                    }
                    else
                    {
                        if (initialOrder.ShippingAddress == null)
                            throw new NopException("Shipping address is not available");

                        //clone billing address
                        shippingAddress = (Address)initialOrder.ShippingAddress.Clone();
                        if (shippingAddress.Country != null && !shippingAddress.Country.AllowsShipping)
                            throw new NopException(string.Format("Country '{0}' is not allowed for shipping", shippingAddress.Country.Name));

                        shippingMethodName = initialOrder.ShippingMethod;
                        shippingRateComputationMethodSystemName = initialOrder.ShippingRateComputationMethodSystemName;
                    }
                }


                //shipping total
                decimal? orderShippingTotalInclTax, orderShippingTotalExclTax = null;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    decimal taxRate = decimal.Zero;
                    Discount shippingTotalDiscount = null;
                    orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, true, out taxRate, out shippingTotalDiscount);
                    orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, false);
                    if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                        throw new NopException("Shipping total couldn't be calculated");

                    if (shippingTotalDiscount != null && !appliedDiscounts.ContainsDiscount(shippingTotalDiscount))
                        appliedDiscounts.Add(shippingTotalDiscount);
                }
                else
                {
                    orderShippingTotalInclTax = initialOrder.OrderShippingInclTax;
                    orderShippingTotalExclTax = initialOrder.OrderShippingExclTax;
                }


                //payment total
                decimal paymentAdditionalFeeInclTax, paymentAdditionalFeeExclTax;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    decimal paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart,vendorOrder.processPaymentRequest.PaymentMethodSystemName);
                    paymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, customer);
                    paymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, customer);
                }
                else
                {
                    paymentAdditionalFeeInclTax = initialOrder.PaymentMethodAdditionalFeeInclTax;
                    paymentAdditionalFeeExclTax = initialOrder.PaymentMethodAdditionalFeeExclTax;
                }


                //tax total
                decimal orderTaxTotal = decimal.Zero;
                string vatNumber = "", taxRates = "";
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    //tax amount
                    SortedDictionary<decimal, decimal> taxRatesDictionary = null;
                    orderTaxTotal = _orderTotalCalculationService.GetTaxTotal(cart, out taxRatesDictionary);

                    //VAT number
                    if (_taxSettings.EuVatEnabled && customer.VatNumberStatus == VatNumberStatus.Valid)
                        vatNumber = customer.VatNumber;

                    //tax rates
                    foreach (var kvp in taxRatesDictionary)
                    {
                        var taxRate = kvp.Key;
                        var taxValue = kvp.Value;
                        taxRates += string.Format("{0}:{1};   ", taxRate.ToString(CultureInfo.InvariantCulture), taxValue.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    orderTaxTotal = initialOrder.OrderTax;
                    //VAT number
                    vatNumber = initialOrder.VatNumber;
                }


                //order total (and applied discounts, gift cards, reward points)
                decimal? orderTotal = null;
                decimal orderDiscountAmount = decimal.Zero;
                List<AppliedGiftCard> appliedGiftCards = null;
                int redeemedRewardPoints = 0;
                decimal redeemedRewardPointsAmount = decimal.Zero;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    Discount orderAppliedDiscount = null;
                    orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                        out orderDiscountAmount, out orderAppliedDiscount, out appliedGiftCards,
                        out redeemedRewardPoints, out redeemedRewardPointsAmount);
                    if (!orderTotal.HasValue)
                        throw new NopException("Order total couldn't be calculated");

                    //discount history
                    if (orderAppliedDiscount != null && !appliedDiscounts.ContainsDiscount(orderAppliedDiscount))
                        appliedDiscounts.Add(orderAppliedDiscount);
                }
                else
                {
                    orderDiscountAmount = initialOrder.OrderDiscount;
                    orderTotal = initialOrder.OrderTotal;
                }
                vendorOrder.processPaymentRequest.OrderTotal = orderTotal.Value;

                #endregion

                #region Payment workflow

                //skip payment workflow if order total equals zero
                bool skipPaymentWorkflow = false;
                if (orderTotal.Value == decimal.Zero)
                    skipPaymentWorkflow = true;

                //payment workflow
                IPaymentMethod paymentMethod = null;
                if (!skipPaymentWorkflow)
                {
                    paymentMethod = _paymentService.LoadPaymentMethodBySystemName(vendorOrder.processPaymentRequest.PaymentMethodSystemName);
                    if (paymentMethod == null)
                        throw new NopException("Payment method couldn't be loaded");

                    //ensure that payment method is active
                    if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                        throw new NopException("Payment method is not active");
                }
                else
                    vendorOrder.processPaymentRequest.PaymentMethodSystemName = "";

                //recurring or standard shopping cart?
                bool isRecurringShoppingCart = false;
                if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                {
                    isRecurringShoppingCart = cart.IsRecurring();
                    if (isRecurringShoppingCart)
                    {
                        int recurringCycleLength = 0;
                        RecurringProductCyclePeriod recurringCyclePeriod;
                        int recurringTotalCycles = 0;
                        string recurringCyclesError = cart.GetRecurringCycleInfo(out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
                        if (!string.IsNullOrEmpty(recurringCyclesError))
                            throw new NopException(recurringCyclesError);
                        vendorOrder.processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                        vendorOrder.processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                        vendorOrder.processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
                    }
                }
                else
                    isRecurringShoppingCart = true;


                //process payment
                ProcessPaymentResult processPaymentResult = null;
                if (!skipPaymentWorkflow)
                {
                    if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                    {
                        if (isRecurringShoppingCart)
                        {
                            //recurring cart
                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(vendorOrder.processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new NopException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                case RecurringPaymentType.Automatic:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(vendorOrder.processPaymentRequest);
                                    break;
                                default:
                                    throw new NopException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            //standard cart
                            processPaymentResult = _paymentService.ProcessPayment(vendorOrder.processPaymentRequest);
                        }
                    }
                    else
                    {
                        if (isRecurringShoppingCart)
                        {
                            //Old credit card info
                            vendorOrder.processPaymentRequest.CreditCardType = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardType) : "";
                            vendorOrder.processPaymentRequest.CreditCardName = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardName) : "";
                            vendorOrder.processPaymentRequest.CreditCardNumber = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardNumber) : "";
                            //MaskedCreditCardNumber 
                            vendorOrder.processPaymentRequest.CreditCardCvv2 = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardCvv2) : "";
                            try
                            {
                                vendorOrder.processPaymentRequest.CreditCardExpireMonth = initialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(initialOrder.CardExpirationMonth)) : 0;
                                vendorOrder.processPaymentRequest.CreditCardExpireYear = initialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(initialOrder.CardExpirationYear)) : 0;
                            }
                            catch { }

                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(vendorOrder.processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new NopException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(vendorOrder.processPaymentRequest);
                                    break;
                                case RecurringPaymentType.Automatic:
                                    //payment is processed on payment gateway site
                                    processPaymentResult = new ProcessPaymentResult();
                                    break;
                                default:
                                    throw new NopException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            throw new NopException("No recurring products");
                        }
                    }
                }
                else
                {
                    //payment is not required
                    if (processPaymentResult == null)
                        processPaymentResult = new ProcessPaymentResult();
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                }

                if (processPaymentResult == null)
                    throw new NopException("processPaymentResult is not available");

                #endregion

                if (processPaymentResult.Success)
                {

                    //save order in data storage
                    //uncomment this line to support transactions
                    //using (var scope = new System.Transactions.TransactionScope())
                    {
                        #region Save order details

                        var shippingStatus = ShippingStatus.NotYetShipped;
                        if (!shoppingCartRequiresShipping)
                            shippingStatus = ShippingStatus.ShippingNotRequired;

                        var order = new Order()
                        {
                            OrderGuid = vendorOrder.processPaymentRequest.OrderGuid,
                            CustomerId = customer.Id,
                            CustomerLanguageId = customerLanguage.Id,
                            CustomerTaxDisplayType = customerTaxDisplayType,
                            CustomerIp = _webHelper.GetCurrentIpAddress(),
                            OrderSubtotalInclTax = orderSubTotalInclTax,
                            OrderSubtotalExclTax = orderSubTotalExclTax,
                            OrderSubTotalDiscountInclTax = orderSubTotalDiscountInclTax,
                            OrderSubTotalDiscountExclTax = orderSubTotalDiscountExclTax,
                            OrderShippingInclTax = orderShippingTotalInclTax.Value,
                            OrderShippingExclTax = orderShippingTotalExclTax.Value,
                            PaymentMethodAdditionalFeeInclTax = paymentAdditionalFeeInclTax,
                            PaymentMethodAdditionalFeeExclTax = paymentAdditionalFeeExclTax,
                            TaxRates = taxRates,
                            OrderTax = orderTaxTotal,
                            OrderTotal = orderTotal.Value,
                            RefundedAmount = decimal.Zero,
                            OrderDiscount = orderDiscountAmount,
                            CheckoutAttributeDescription = checkoutAttributeDescription,
                            CheckoutAttributesXml = checkoutAttributesXml,
                            CustomerCurrencyCode = customerCurrencyCode,
                            CurrencyRate = customerCurrencyRate,
                            AffiliateId = (customer.Affiliate != null && !customer.Affiliate.Deleted && customer.Affiliate.Active) ? customer.AffiliateId : null,
                            OrderStatus = OrderStatus.Pending,
                            AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                            CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(vendorOrder.processPaymentRequest.CreditCardType) : string.Empty,
                            CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(vendorOrder.processPaymentRequest.CreditCardName) : string.Empty,
                            CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(vendorOrder.processPaymentRequest.CreditCardNumber) : string.Empty,
                            MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(vendorOrder.processPaymentRequest.CreditCardNumber)),
                            CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(vendorOrder.processPaymentRequest.CreditCardCvv2) : string.Empty,
                            CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(vendorOrder.processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                            CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(vendorOrder.processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                            PaymentMethodSystemName = vendorOrder.processPaymentRequest.PaymentMethodSystemName,
                            AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                            AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                            AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                            CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                            CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                            SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                            PurchaseOrderNumber = vendorOrder.processPaymentRequest.PurchaseOrderNumber,
                            PaymentStatus = processPaymentResult.NewPaymentStatus,
                            PaidDateUtc = null,
                            BillingAddress = billingAddress,
                            ShippingAddress = shippingAddress,
                            ShippingStatus = shippingStatus,
                            ShippingMethod = shippingMethodName,
                            ShippingRateComputationMethodSystemName = shippingRateComputationMethodSystemName,
                            VatNumber = vatNumber,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        _orderService.InsertOrder(order);

                        result.PlacedOrder = order;

                        if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                        {
                            //move shopping cart items to order product variants
                            foreach (var sc in cart)
                            {
                                //prices
                                decimal taxRate = decimal.Zero;
                                decimal scUnitPrice = _priceCalculationService.GetUnitPrice(sc, true);
                                decimal scSubTotal = _priceCalculationService.GetSubTotal(sc, true);
                                decimal scUnitPriceInclTax = _taxService.GetProductPrice(sc.ProductVariant, scUnitPrice, true, customer, out taxRate);
                                decimal scUnitPriceExclTax = _taxService.GetProductPrice(sc.ProductVariant, scUnitPrice, false, customer, out taxRate);
                                decimal scSubTotalInclTax = _taxService.GetProductPrice(sc.ProductVariant, scSubTotal, true, customer, out taxRate);
                                decimal scSubTotalExclTax = _taxService.GetProductPrice(sc.ProductVariant, scSubTotal, false, customer, out taxRate);

                                //discounts
                                Discount scDiscount = null;
                                decimal discountAmount = _priceCalculationService.GetDiscountAmount(sc, out scDiscount);
                                decimal discountAmountInclTax = _taxService.GetProductPrice(sc.ProductVariant, discountAmount, true, customer, out taxRate);
                                decimal discountAmountExclTax = _taxService.GetProductPrice(sc.ProductVariant, discountAmount, false, customer, out taxRate);
                                if (scDiscount != null && !appliedDiscounts.ContainsDiscount(scDiscount))
                                    appliedDiscounts.Add(scDiscount);

                                //attributes
                                string attributeDescription = _productAttributeFormatter.FormatAttributes(sc.ProductVariant, sc.AttributesXml, customer);
                                var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                                //save order item
                                var opv = new OrderProductVariant()
                                {
                                    OrderProductVariantGuid = Guid.NewGuid(),
                                    Order = order,
                                    ProductVariantId = sc.ProductVariantId,
                                    UnitPriceInclTax = scUnitPriceInclTax,
                                    UnitPriceExclTax = scUnitPriceExclTax,
                                    PriceInclTax = scSubTotalInclTax,
                                    PriceExclTax = scSubTotalExclTax,
                                    AttributeDescription = attributeDescription,
                                    AttributesXml = sc.AttributesXml,
                                    Quantity = sc.Quantity,
                                    DiscountAmountInclTax = discountAmountInclTax,
                                    DiscountAmountExclTax = discountAmountExclTax,
                                    DownloadCount = 0,
                                    IsDownloadActivated = false,
                                    LicenseDownloadId = 0,
                                    ItemWeight = itemWeight,
                                };
                                order.OrderProductVariants.Add(opv);
                                _orderService.UpdateOrder(order);

                                //gift cards
                                if (sc.ProductVariant.IsGiftCard)
                                {
                                    string giftCardRecipientName, giftCardRecipientEmail,
                                        giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                    _productAttributeParser.GetGiftCardAttribute(sc.AttributesXml,
                                        out giftCardRecipientName, out giftCardRecipientEmail,
                                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                    for (int i = 0; i < sc.Quantity; i++)
                                    {
                                        var gc = new GiftCard()
                                        {
                                            GiftCardType = sc.ProductVariant.GiftCardType,
                                            PurchasedWithOrderProductVariant = opv,
                                            Amount = scUnitPriceExclTax,
                                            IsGiftCardActivated = false,
                                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                            RecipientName = giftCardRecipientName,
                                            RecipientEmail = giftCardRecipientEmail,
                                            SenderName = giftCardSenderName,
                                            SenderEmail = giftCardSenderEmail,
                                            Message = giftCardMessage,
                                            IsRecipientNotified = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        };
                                        _giftCardService.InsertGiftCard(gc);
                                    }
                                }

                                //inventory
                                _productService.AdjustInventory(sc.ProductVariant, true, sc.Quantity, sc.AttributesXml);
                            }

                            //clear shopping cart
                            //customer.ShoppingCartItems.Clear();
                            //_customerService.UpdateCustomer(customer);
                            customer.ShoppingCartItems.ToList().Where(sci => _vendorService.GetVendorIdByProductId( sci.ProductVariant.Product.Id) == vendorOrder.vendor.Id).ToList().ForEach(sci =>
                                _shoppingCartService.DeleteShoppingCartItem(sci, false));
                        }
                        else
                        {
                            //recurring payment
                            var initialOrderProductVariants = initialOrder.OrderProductVariants;
                            foreach (var opv in initialOrderProductVariants)
                            {
                                //save item
                                var newOpv = new OrderProductVariant()
                                {
                                    OrderProductVariantGuid = Guid.NewGuid(),
                                    Order = order,
                                    ProductVariantId = opv.ProductVariantId,
                                    UnitPriceInclTax = opv.UnitPriceInclTax,
                                    UnitPriceExclTax = opv.UnitPriceExclTax,
                                    PriceInclTax = opv.PriceInclTax,
                                    PriceExclTax = opv.PriceExclTax,
                                    AttributeDescription = opv.AttributeDescription,
                                    AttributesXml = opv.AttributesXml,
                                    Quantity = opv.Quantity,
                                    DiscountAmountInclTax = opv.DiscountAmountInclTax,
                                    DiscountAmountExclTax = opv.DiscountAmountExclTax,
                                    DownloadCount = 0,
                                    IsDownloadActivated = false,
                                    LicenseDownloadId = 0,
                                    ItemWeight = opv.ItemWeight,
                                };
                                order.OrderProductVariants.Add(newOpv);
                                _orderService.UpdateOrder(order);

                                //gift cards
                                if (opv.ProductVariant.IsGiftCard)
                                {
                                    string giftCardRecipientName, giftCardRecipientEmail,
                                        giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                    _productAttributeParser.GetGiftCardAttribute(opv.AttributesXml,
                                        out giftCardRecipientName, out giftCardRecipientEmail,
                                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                    for (int i = 0; i < opv.Quantity; i++)
                                    {
                                        var gc = new GiftCard()
                                        {
                                            GiftCardType = opv.ProductVariant.GiftCardType,
                                            PurchasedWithOrderProductVariant = newOpv,
                                            Amount = opv.UnitPriceExclTax,
                                            IsGiftCardActivated = false,
                                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                            RecipientName = giftCardRecipientName,
                                            RecipientEmail = giftCardRecipientEmail,
                                            SenderName = giftCardSenderName,
                                            SenderEmail = giftCardSenderEmail,
                                            Message = giftCardMessage,
                                            IsRecipientNotified = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        };
                                        _giftCardService.InsertGiftCard(gc);
                                    }
                                }

                                //inventory
                                _productService.AdjustInventory(opv.ProductVariant, true, opv.Quantity, opv.AttributesXml);
                            }
                        }

                        //discount usage history
                        if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                            foreach (var discount in appliedDiscounts)
                            {
                                var duh = new DiscountUsageHistory()
                                {
                                    Discount = discount,
                                    Order = order,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                _discountService.InsertDiscountUsageHistory(duh);
                            }

                        //gift card usage history
                        if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                            if (appliedGiftCards != null)
                                foreach (var agc in appliedGiftCards)
                                {
                                    decimal amountUsed = agc.AmountCanBeUsed;
                                    var gcuh = new GiftCardUsageHistory()
                                    {
                                        GiftCard = agc.GiftCard,
                                        UsedWithOrder = order,
                                        UsedValue = amountUsed,
                                        CreatedOnUtc = DateTime.UtcNow
                                    };
                                    agc.GiftCard.GiftCardUsageHistory.Add(gcuh);
                                    _giftCardService.UpdateGiftCard(agc.GiftCard);
                                }

                        //reward points history
                        if (redeemedRewardPointsAmount > decimal.Zero)
                        {
                            customer.AddRewardPointsHistoryEntry(-redeemedRewardPoints,
                                string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.Id),
                                order,
                                redeemedRewardPointsAmount);
                            _customerService.UpdateCustomer(customer);
                        }

                        //recurring orders
                        if (!vendorOrder.processPaymentRequest.IsRecurringPayment && isRecurringShoppingCart)
                        {
                            //create recurring payment (the first payment)
                            var rp = new RecurringPayment()
                            {
                                CycleLength = vendorOrder.processPaymentRequest.RecurringCycleLength,
                                CyclePeriod = vendorOrder.processPaymentRequest.RecurringCyclePeriod,
                                TotalCycles = vendorOrder.processPaymentRequest.RecurringTotalCycles,
                                StartDateUtc = DateTime.UtcNow,
                                IsActive = true,
                                CreatedOnUtc = DateTime.UtcNow,
                                InitialOrder = order,
                            };
                            _orderService.InsertRecurringPayment(rp);


                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(vendorOrder.processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    {
                                        //not supported
                                    }
                                    break;
                                case RecurringPaymentType.Manual:
                                    {
                                        //first payment
                                        var rph = new RecurringPaymentHistory()
                                        {
                                            RecurringPayment = rp,
                                            CreatedOnUtc = DateTime.UtcNow,
                                            OrderId = order.Id,
                                        };
                                        rp.RecurringPaymentHistory.Add(rph);
                                        _orderService.UpdateRecurringPayment(rp);
                                    }
                                    break;
                                case RecurringPaymentType.Automatic:
                                    {
                                        //will be created later (process is automated)
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        #endregion

                        #region Notifications & notes

                        //notes, messages
                        order.OrderNotes.Add(new OrderNote()
                        {
                            Note = "Order placed",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);

                        //send email notifications
                        int orderPlacedVendorOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                        if (orderPlacedVendorOwnerNotificationQueuedEmailId > 0)
                        {
                            order.OrderNotes.Add(new OrderNote()
                            {
                                Note = string.Format("\"Order placed\" email (to vendor owner) has been queued. Queued email identifier: {0}.", orderPlacedVendorOwnerNotificationQueuedEmailId),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                        }

                        int orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId);
                        if (orderPlacedCustomerNotificationQueuedEmailId > 0)
                        {
                            order.OrderNotes.Add(new OrderNote()
                            {
                                Note = string.Format("\"Order placed\" email (to customer) has been queued. Queued email identifier: {0}.", orderPlacedCustomerNotificationQueuedEmailId),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                        }

                        //check order status
                        CheckOrderStatus(order);

                        //reset checkout data
                        if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                            _customerService.ResetCheckoutData(customer, clearCouponCodes: true, clearCheckoutAttributes: true);

                        if (!vendorOrder.processPaymentRequest.IsRecurringPayment)
                        {
                            _customerActivityService.InsertActivity(
                                "PublicVendor.PlaceOrder",
                                 _localizationService.GetResource("ActivityLog.PublicVendor.PlaceOrder"),
                                order.Id);
                        }

                        //uncomment this line to support transactions
                        //scope.Complete();

                        #endregion
                    }
                }
                else
                {
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format("Payment error: {0}", paymentError));
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            #region Process errors

            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i + 1, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //log it
                string logError = string.Format("Error while placing order. {0}", error);
                _logger.Error(logError);
            }

            #endregion

            return result;
        }

        #endregion
    }
}
