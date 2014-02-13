﻿/*
AspxCommerce® - http://www.aspxcommerce.com
Copyright (c) 20011-2012 by AspxCommerce
Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using AspxCommerce.Core;
using AspxCommerce.Core.Mobile;
using SageFrame.Web;
using AspxCommerce.CashOnDelivery;

public partial class Modules_AspxCommerce_PaymentGateways_CashOnDeliverySuccess : BaseAdministrationUserControl
{   
    public string SendEmailFrom, SendOrderNotice,PageExtension,_sageRedirectPath, _addressPath = string.Empty,MainCurrency = string.Empty, Region, SelectedCurrency;
    bool _isUseFriendlyUrls = true;   
    decimal currencyRate = 1;
    public int orderID;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            AspxCommonInfo aspxCommonObj = new AspxCommonInfo();
            aspxCommonObj.StoreID = GetStoreID;
            aspxCommonObj.PortalID = GetPortalID;
            aspxCommonObj.UserName = GetUsername;
            aspxCommonObj.CultureName = GetCurrentCultureName;            

            if (!IsPostBack)
            {
                if (Session["OrderID"] != null)
                {
                    orderID = int.Parse(Session["OrderID"].ToString());
                }

                StoreSettingConfig ssc = new StoreSettingConfig();
                AspxCommerceWebService aws = new AspxCommerceWebService();
                //Store Default Currency
                MainCurrency = ssc.GetStoreSettingsByKey(StoreSetting.MainCurrency, GetPortalID, GetPortalID, GetCurrentCultureName);

                if (Session["CurrencyCode"] != null && Session["CurrencyCode"] != "")
                {
                    SelectedCurrency = Session["CurrencyCode"].ToString();
                }
                else
                {
                    SelectedCurrency = MainCurrency;
                }
                if (Session["Region"] != null && Session["Region"] != "")
                {
                    Region = Session["Region"].ToString();
                }
                else
                {
                    Region = StoreSetting.GetRegionFromCurrencyCode(SelectedCurrency, GetStoreID, GetPortalID);
                }
               
                //Rate to convert Store Default Item Price to Selcted curency Price
                currencyRate = Convert.ToDecimal(aws.GetCurrencyRateOnChange(aspxCommonObj, MainCurrency, SelectedCurrency, Region));
          


                var sfConfig = new SageFrameConfig();
                _isUseFriendlyUrls = sfConfig.GetSettingBollByKey(SageFrameSettingKeys.UseFriendlyUrls);
                PageExtension = SageFrameSettingKeys.PageExtension;
                if (_isUseFriendlyUrls)
                {
                    if (GetPortalID > 1)
                    {
                        _sageRedirectPath = ResolveUrl("~/portal/" + GetPortalSEOName + "/" + sfConfig.GetSettingsByKey(SageFrameSettingKeys.PortalDefaultPage) + PageExtension);
                        _addressPath = HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + "/portal/" + GetPortalSEOName + "/";
             
                    }
                    else
                    {
                        _sageRedirectPath = ResolveUrl("~/" + sfConfig.GetSettingsByKey(SageFrameSettingKeys.PortalDefaultPage) + PageExtension);
                        _addressPath = HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + "/";
             
                    }
                }
                else
                {
                    _sageRedirectPath = ResolveUrl("{~/Default" + PageExtension + "?ptlid=" + GetPortalID + "&ptSEO=" + GetPortalSEOName + "&pgnm=" + sfConfig.GetSettingsByKey(SageFrameSettingKeys.PortalDefaultPage));
                }

                var imgProgress = (Image)UpdateProgress1.FindControl("imgPrgress");
                if (imgProgress != null)
                {
                    imgProgress.ImageUrl = GetTemplateImageUrl("ajax-loader.gif", true);
                }
                hlnkHomePage.NavigateUrl = _sageRedirectPath;               
                SendEmailFrom = ssc.GetStoreSettingsByKey(StoreSetting.SendEcommerceEmailsFrom, GetStoreID, GetPortalID, GetCurrentCultureName);
                SendOrderNotice = ssc.GetStoreSettingsByKey(StoreSetting.SendOrderNotification, GetStoreID, GetPortalID, GetCurrentCultureName);

                if (Session["mb_IsCheckoutFromMobile"] != null)
                {
                    MobileSuccess(_sageRedirectPath);
                }
                else
                {
                    SendConfrimMessage(_sageRedirectPath);
                }
                
            }
            IncludeLanguageJS();

            
        }
        catch(Exception ex)
        {
            ProcessException(ex);
        }
    }
    private void MobileSuccess(string redirectPath)
    {
        try
        {
            if (Session["mb_OrderDetail"] != null)
            {
                var orderInfo = (OrderInfo) Session["mb_OrderDetail"];
                //Session["mb_IsCheckoutFromMobile"];
                var giftCardUsage = (List<GiftCardUsage>) Session["mb_GiftCardUsage"];
                var couponCode = Session["mb_CouponCode"].ToString();
                var couponCodeApplied = int.Parse(Session["mb_CouponCodeApplied"].ToString());
                var billingAddress = (UserAddressInfo) Session["mb_BillingAddress"];
                var shippingAddress = (UserAddressInfo) Session["mb_ShippingAddress"];
                var itemsInfo = (List<OrderItem>) Session["mb_ItemDetails"];
                int storeId = orderInfo.StoreId;
                int portalId = orderInfo.PortalId;
                string userName = orderInfo.AddedBy;
                int customerId = orderInfo.CustomerId;

                const int responseCode = 1; // response code, defaulted to Invalid
                const string responsereasontext = "Transaction occured successfully";
                const int responsereasonCode = 1;

                string invoice = orderInfo.InvoiceNumber;
                var random = new Random();
                string purchaseorderNo = (random.Next(0, 1000)).ToString();
                string timeStamp = ((int) (DateTime.UtcNow - new DateTime(2011, 1, 1)).TotalSeconds).ToString();
             
                if (!string.IsNullOrEmpty(orderInfo.TransactionId) && orderInfo.TransactionId.Trim()!="0")
                {
                    lblTransaction.Text = orderInfo.TransactionId;
                    lblInvoice.Text = orderInfo.InvoiceNumber;
                    lblPaymentStatus.Text = "Successfull";
                    lblPaymentMethod.Text = orderInfo.PaymentMethodName;
                    lblDateTime.Text = orderInfo.AddedOn.ToString("dddd, dd MMMM yyyy ");  
                    lblOrderNo.Text = "#" + orderInfo.OrderId;
                }
                else
                {
                string transID = (random.Next(99999, 111111)).ToString();
                lblTransaction.Text = transID;
                lblInvoice.Text = invoice;
                lblPaymentStatus.Text = "Successfull";
                lblPaymentMethod.Text = orderInfo.PaymentMethodName;
                lblDateTime.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy ");
                lblOrderNo.Text = "#" + orderInfo.OrderId;
               
                string result = CashOnDelivery.ParseForMobile(transID, orderInfo, purchaseorderNo, responseCode,
                                                              responsereasonCode,
                                                              responsereasontext);
                var couponUsage = "";
                AspxCommonInfo aspxCommonObj = new AspxCommonInfo();
                aspxCommonObj.StoreID = storeId;
                aspxCommonObj.PortalID = portalId;
                aspxCommonObj.UserName = userName;               
                aspxCommonObj.CultureName = GetCurrentCultureName;
                if (!string.IsNullOrEmpty(couponCode))
                    couponUsage = couponCode + "#" + couponCodeApplied;

                CashOnDelivery.UpdateItemQuantityAndCoupon(orderInfo, itemsInfo, couponUsage, storeId, portalId,
                                                           userName);
                AspxGiftCardController.IssueGiftCardForMobile(itemsInfo, false, aspxCommonObj);
                if (giftCardUsage != null && giftCardUsage.Count > 0)
                {
                    AspxGiftCardController.UpdateGiftCardUsage(giftCardUsage, storeId,
                                                           portalId,
                                                           orderInfo.OrderId,
                                                           userName,
                                                           orderInfo.CultureName);

                }


                lblerror.Text = result;
                lblerror.Text = GetSageMessage("Payment", "PaymentProcessed");
                var tinfo = new TransactionLogInfo();
                var tlog = new TransactionLog();                  
              

                tinfo.TransactionID = transID;
                tinfo.AuthCode = "";
                  tinfo.TotalAmount = decimal.Parse(orderInfo.GrandTotal.ToString())* currencyRate;
                //tinfo.TotalAmount = decimal.Parse(Session["GrandTotalAll"].ToString()) * currencyRate; 
                tinfo.ResponseCode = responseCode.ToString();
                tinfo.ResponseReasonText = responsereasontext;
                tinfo.OrderID = orderInfo.OrderId;
                tinfo.StoreID = orderInfo.StoreId;
                tinfo.PortalID = orderInfo.PortalId;
                tinfo.AddedBy = orderInfo.AddedBy;
                tinfo.CustomerID = orderInfo.CustomerId;
                tinfo.SessionCode = orderInfo.SessionCode;
                tinfo.PaymentGatewayID = orderInfo.PaymentGatewayTypeId;
                tinfo.PaymentStatus = "Processed";
                tinfo.CreditCard = "";
                tinfo.CurrencyCode = SelectedCurrency;
                tlog.SaveTransactionLog(tinfo);

                try
                {
                    EmailTemplate.SendEmailForOrderMobile(orderInfo, billingAddress, shippingAddress, _addressPath,
                                                          TemplateName, transID);
                }
                catch
                {
                    lblerror.Text = "";
                    lblerror.Text = GetSageMessage("Payment", "EmailSendOrderProblem");
                }
                }
                Session.Clear();

            }
            else
            {
                Response.Redirect(redirectPath, false);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void SendConfrimMessage(string redirectPat)
    {
        try
        {
            if (Session["OrderID"] != null)
            {
                const int responseCode = 1; // response code, defaulted to Invalid
                const string responsereasontext = "Transaction occured successfully";
                const int responsereasonCode = 1;
                string paymentmethod = string.Empty;
                var orderdata2 = new OrderDetailsCollection();
                if (HttpContext.Current.Session["OrderCollection"] != null)
                {
                  
                    orderdata2 = (OrderDetailsCollection)HttpContext.Current.Session["OrderCollection"];
                   
                }
                string invoice = orderdata2.ObjOrderDetails.InvoiceNumber;
                var random = new Random();
                string purchaseorderNo = (random.Next(0, 1000)).ToString();
                string timeStamp = ((int)(DateTime.UtcNow - new DateTime(2011, 1, 1)).TotalSeconds).ToString();
                string transID = (random.Next(99999, 111111)).ToString();                
                lblTransaction.Text = transID;
                lblInvoice.Text = invoice;
                lblPaymentMethod.Text = "Cash On Delivery";
                lblPaymentStatus.Text = "Successfull";
                lblDateTime.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy ");
                lblOrderNo.Text = "#" + Session["OrderID"].ToString();
                AspxCommonInfo aspxCommonObj = new AspxCommonInfo();
                aspxCommonObj.StoreID = GetStoreID;
                aspxCommonObj.PortalID = GetPortalID;
                aspxCommonObj.UserName = GetUsername;
                aspxCommonObj.CustomerID = GetCustomerID;
                aspxCommonObj.SessionCode = HttpContext.Current.Session.SessionID;
                aspxCommonObj.CultureName = GetCurrentCultureName;
                string result = CashOnDelivery.Parse(transID, invoice, purchaseorderNo, responseCode, responsereasonCode, responsereasontext, aspxCommonObj);
                AspxGiftCardController.IssueGiftCard(orderdata2.LstOrderItemsInfo, false, aspxCommonObj);
                if (orderdata2.GiftCardDetail != null && HttpContext.Current.Session["UsedGiftCard"] != null)
                {
                    AspxGiftCardController.UpdateGiftCardUsage(orderdata2.GiftCardDetail, orderdata2.ObjCommonInfo.StoreID,
                                         orderdata2.ObjCommonInfo.PortalID, orderdata2.ObjOrderDetails.OrderID, orderdata2.ObjCommonInfo.AddedBy,
                                         orderdata2.ObjCommonInfo.CultureName);
                    HttpContext.Current.Session.Remove("UsedGiftCard");
                }
              
                lblerror.Text = result;
                lblerror.Text = GetSageMessage("Payment", "PaymentProcessed"); 
                var tinfo = new TransactionLogInfo();
                var tlog = new TransactionLog();                        

                tinfo.TransactionID = transID;
                tinfo.AuthCode = "";
                // tinfo.TotalAmount = decimal.Parse(orderdata2.ObjOrderDetails.GrandTotal.ToString());
                tinfo.TotalAmount = decimal.Parse(Session["GrandTotalAll"].ToString()) * currencyRate;
                tinfo.ResponseCode = responseCode.ToString();
                tinfo.ResponseReasonText = responsereasontext;
                tinfo.OrderID = orderdata2.ObjOrderDetails.OrderID;
                tinfo.StoreID =  orderdata2.ObjCommonInfo.StoreID;
                tinfo.PortalID = orderdata2.ObjCommonInfo.PortalID;
                tinfo.AddedBy = orderdata2.ObjCommonInfo.AddedBy;
                tinfo.CustomerID = orderdata2.ObjOrderDetails.CustomerID;
                tinfo.SessionCode = orderdata2.ObjOrderDetails.SessionCode;
                tinfo.PaymentGatewayID = orderdata2.ObjOrderDetails.PaymentGatewayTypeID;
                tinfo.PaymentStatus = "Processed";
                tinfo.CreditCard = "";
                tinfo.CurrencyCode = SelectedCurrency;
                tlog.SaveTransactionLog(tinfo);

                var clSes = new AspxCommerceWebService();
                if (Session["IsFreeShipping"] != null)
                {
                    HttpContext.Current.Session.Remove("IsFreeShipping");
                }
                if (Session["DiscountAmount"] != null)
                {
                    HttpContext.Current.Session.Remove("DiscountAmount");

                }
                if (Session["CouponDiscountAmount"] != null)
                {
                    HttpContext.Current.Session.Remove("CouponDiscountAmount");

                }
                if (Session["IsCouponInPercent"] != null)
                {
                    HttpContext.Current.Session.Remove("IsCouponInPercent");

                }
                if (Session["CouponPercentValue"] != null)
                {
                    HttpContext.Current.Session.Remove("CouponPercentValue");

                }
                if (Session["CouponSessionPercentAmount"] != null)
                {
                    HttpContext.Current.Session.Remove("CouponSessionPercentAmount");

                }
                if (Session["CouponSessionAmount"] != null)
                {
                    HttpContext.Current.Session.Remove("CouponSessionAmount");

                }
                if (Session["CouponCode"] != null)
                {
                    HttpContext.Current.Session.Remove("CouponCode");
                }
                if (Session["CouponApplied"] != null)
                {
                    HttpContext.Current.Session.Remove("CouponApplied");
                }
                if (Session["DiscountAll"] != null)
                {
                    HttpContext.Current.Session.Remove("DiscountAll");
                }
                if (Session["TaxAll"] != null)
                {
                    HttpContext.Current.Session.Remove("TaxAll");
                }
                if (Session["ShippingCostAll"] != null)
                {
                    HttpContext.Current.Session.Remove("ShippingCostAll");
                }
                if (Session["GrandTotalAll"] != null)
                {
                    HttpContext.Current.Session.Remove("GrandTotalAll");
                }
                if (Session["Gateway"] != null)
                {
                    HttpContext.Current.Session.Remove("Gateway");
                }  

                //invoice  transID

                if (Session["OrderCollection"] != null)
                {
                   
                  var orderdata = (OrderDetailsCollection)Session["OrderCollection"];
                    try
                    {
                        orderdata.ObjOrderDetails.OrderStatus = "Processed";
                        EmailTemplate.SendEmailForOrder(GetPortalID, orderdata, _addressPath, TemplateName, transID);
                    }
                    catch (Exception ex)
                    {
                        lblerror.Text = "";
                        lblerror.Text= GetSageMessage("Payment", "EmailSendOrderProblem");
                        ProcessException(ex);
                    }
                    clSes.ClearSessionVariable("OrderCollection");
                }
            }
            else
            {
                Response.Redirect(_sageRedirectPath, false);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
