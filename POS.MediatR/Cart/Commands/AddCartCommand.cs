﻿using MediatR;
using POS.Data.Dto;
using POS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MediatR.CommandAndQuery
{
    public class AddCartCommand : IRequest<ServiceResponse<CartDto>>
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitName { get; set; }
        public Guid UnitId { get; set; }
        public decimal Total { get; set; }
        public decimal TaxValue { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public bool? IsAdvanceOrderRequest { get; set; }
        public Guid ProductMainCategoryId { get; set; }
        public string PackagingName { get; set; }
        public Guid? PackagingId { get; set; }
        public decimal? MinQty { get; set; }
        public int NoOfItems { get; set; }

    }
}
