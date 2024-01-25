﻿using POS.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Data.Entities
{
    public class MobileApiResponse
    {
        public int Id { get; set; }

    }

    public class CustomerResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public CustomerDto Data { get; set; }
    }

    public class NonCSDResponseNameData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public IList<NonCSDCanteenDto> Data { get; set; }
    }

    public class ProductListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<ProductDto> Data { get; set; }
    }

    public class ProductDetailsResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public ProductDto Data { get; set; }
    }

    public class ProductDetailsRequestData
    {
        public string Id { get; set; }

    }

    public class ProductCategoriesResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int productCount { get; set; }
        public IList<ProductCategoryDto> Data { get; set; }
    }

    public class IUDResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }

    }

    public class CartListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<CartDto> Data { get; set; }
    }
    public class CustomerAddressListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<CustomerAddressDto> Data { get; set; }
    }
    public class CustomerAddressResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public CustomerAddressDto Data { get; set; }
    }

    public class WishlistResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<WishlistDto> Data { get; set; }
    }



    public class PaymentCardResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public PaymentCardDto Data { get; set; }
    }

    public class PaymentCardListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<PaymentCardDto> Data { get; set; }
    }

    public class CustomerProfileResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public CustomerDto Data { get; set; }
    }

    public class CustomerOrderListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<SalesOrderDto> Data { get; set; }
    }

    public class ReminderListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<ReminderDto> Data { get; set; }
    }

    public class LogoutResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public string Data { get; set; }
    }



    public class CustomerOrderDetailsResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public SalesOrderDto Data { get; set; }
    }
    public class CustomerOrderSummaryResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public OrderSummary Data { get; set; }
    }

    public class OrderSummary
    {
        public string Price { get; set; }
        public string Discount { get; set; }
        public decimal Items { get; set; }
        public string DeliveryCharges { get; set; }
    }

    public class OrderNumberResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public string OrderNumber { get; set; }
    }

    public class SendEmailResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
    }

    public class BannerListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<BannerDto> Data { get; set; }
    }
    public class HomePageBannerListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string Text { get; set; }
        public IList<HomePageBannerDto> Data { get; set; }
    }
    public class NoticeResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public NoticeDto Data { get; set; }
    }

    public class LoginPageBannerListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<LoginPageBannerDto> Data { get; set; }
    }

    public class CategoryBannerListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<CategoryBannerDto> Data { get; set; }
    }

    public class SalesOrderResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public Guid? SalesOrderId { get; set; }
    }

    public class CounterResponseNameData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public IList<CounterDto> Data { get; set; }
    }
    public class ProductMainCategoriesResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public IList<ProductMainCategoryDto> Data { get; set; }
    }

    public class FAQResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public IList<FAQDto> Data { get; set; }
    }
    public class HelpAndSupportResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public IList<HelpAndSupportDto> Data { get; set; }
    }

    public class AppVersionResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public IList<AppVersionDto> Data { get; set; }
    }
    public class ExlUploadPurchaseOrderResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public List<PurchaseOrderItemDto> Data { get; set; }
    }

    public class ProductCategoryWiseSalesReportResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public string ProductCategoryName { get; set; }
        public string PurAmount { get; set; }
        public string TotalAmount { get; set; }

        public string OtherPurAmount { get; set; }
        public string OtherTotalAmount { get; set; }

    }
    public class MessageRequest
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string DeviceToken { get; set; }
    }

    public class TotalPaymentsSalesReportResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }       
        public string PaymentType { get; set; }      
        public string TotalAmount { get; set; }
        public List<PaymentsData> Data { get; set; }

    }

    public class PaymentsData
    {
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        
    }

    public class CounterSalesData
    {
        public string CounterName { get; set; }
        public decimal TotalAmount { get; set; }

    }

    public class DayWiseSummaryReportResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }

        public List<CounterSalesData> CounterSalesData { get; set; }
        public List<PaymentsData> PaymentsData { get; set; }

        public string ProductCategoryName { get; set; }
        public string PurAmount { get; set; }
        public string TotalAmount { get; set; }

        public string OtherPurAmount { get; set; }
        public string OtherTotalAmount { get; set; }

    }

    public class BrandListResponseData
    {
        public bool status { get; set; }
        public int StatusCode { get; set; }
        public string message { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<BrandDto> Data { get; set; }
    }

}


