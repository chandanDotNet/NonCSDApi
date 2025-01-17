﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Data.Dto
{
    public class InventoryDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal Stock { get; set; }
        public decimal PricePerUnit { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal AveragePurchasePrice { get; set; }
        public decimal AverageSalesPrice { get; set; }
        public InventorySourceEnum InventorySource { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public Guid? SalesOrderId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? WarehouseId { get; set; }
        public decimal TaxValue { get; set; }
        public decimal Discount { get; set; }
        public string BrandName { get; set; }
        public Guid BrandId { get; set; }
        public string SupplierName { get; set; }
        public Guid? SupplierId { get; set; }
        public decimal TotalStockAmount { get; set; }
        public string Barcode { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Mrp { get; set; }
        public decimal? Margin { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductCode { get; set; }
        public string ManufacturerName { get; set; }
        public List<InventoryHistoryDto> InventoryHistories { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal ClosingStock { get; set; }
        public ProductDto Product { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }
}
