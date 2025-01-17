﻿using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using POS.Common.GenericRepository;
using POS.Common.UnitOfWork;
using POS.Data;
using POS.Data.Dto;
using POS.Data.Resources;
using POS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace POS.Repository
{
    public class InventoryRepository
        : GenericRepository<Inventory, POSDbContext>, IInventoryRepository
    {
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IInventoryHistoryRepository _inventoryHistoryRepository;
        private readonly IUnitConversationRepository _unitConversationRepository;
        private readonly IWarehouseInventoryRepository _warehouseInventoryRepository;
        private readonly IMapper _mapper;

        public InventoryRepository(IUnitOfWork<POSDbContext> uow,
            IPropertyMappingService propertyMappingService,
            IInventoryHistoryRepository inventoryHistoryRepository,
            IUnitConversationRepository unitConversationRepository,
            IWarehouseInventoryRepository warehouseInventoryRepository,
            IMapper mapper)
          : base(uow)
        {
            _propertyMappingService = propertyMappingService;
            _inventoryHistoryRepository = inventoryHistoryRepository;
            _unitConversationRepository = unitConversationRepository;
            _warehouseInventoryRepository = warehouseInventoryRepository;
            _mapper = mapper;
        }

        public async Task AddInventory(InventoryDto inventory)
        {
            if (inventory.Year <= 0 || inventory.Year == null && inventory.Month <= 0 || inventory.Month == null)
            {
                inventory.Year = DateTime.Now.Year;
                //inventory.Month = DateTime.Now.Month;
                inventory.Month = 3;
            }

            var existingInventory = await All.Where(x => x.ProductId == inventory.ProductId
            && x.Year == inventory.Year && x.Month == inventory.Month).FirstOrDefaultAsync();

            if (existingInventory == null)
            {
                _inventoryHistoryRepository.Add(new InventoryHistory
                {
                    ProductId = inventory.ProductId,
                    InventorySource = inventory.InventorySource,
                    Stock = inventory.InventorySource == InventorySourceEnum.SalesOrder ? (-1) * inventory.Stock : inventory.Stock,
                    PricePerUnit = inventory.PricePerUnit,
                    PreviousTotalStock = 0,
                    SalesOrderId = inventory.SalesOrderId,
                    PurchaseOrderId = inventory.PurchaseOrderId,
                    PurchasePrice = inventory.PurchasePrice,
                    Mrp = inventory.Mrp,
                    Margin = inventory.Margin,
                    Year = inventory.Year,
                    Month= inventory.Month
                });

                var inventoryToAdd = new Inventory
                {
                    ProductId = inventory.ProductId,
                };

                if (inventory.InventorySource == InventorySourceEnum.PurchaseOrder || inventory.InventorySource == InventorySourceEnum.Direct)
                {
                    inventoryToAdd.Stock = inventory.Stock;
                    inventoryToAdd.AveragePurchasePrice = inventory.PricePerUnit;
                    inventoryToAdd.Year= inventory.Year;
                    inventoryToAdd.Month = inventory.Month;
                }
                else
                {
                    inventoryToAdd.Stock = (-1) * inventory.Stock;
                    inventoryToAdd.AverageSalesPrice = inventory.PricePerUnit;
                    inventoryToAdd.Year = inventory.Year;
                    inventoryToAdd.Month = inventory.Month;
                }
                Add(inventoryToAdd);
            }
            else
            {
                if (inventory.InventorySource == InventorySourceEnum.DeletePurchaseOrder)
                {
                    var existingPurchaseInventoryHistory = await _inventoryHistoryRepository.All
                        .FirstOrDefaultAsync(c => inventory.ProductId == c.ProductId && inventory.PurchaseOrderId.HasValue && c.PurchaseOrderId == inventory.PurchaseOrderId
                        );
                    if (existingPurchaseInventoryHistory != null)
                    {
                        var purchaseOrderTotalStock = _inventoryHistoryRepository.All.Where(c => c.ProductId == inventory.ProductId && c.Year == inventory.Year && c.Month == inventory.Month
                        && (c.InventorySource == InventorySourceEnum.PurchaseOrder || c.InventorySource == InventorySourceEnum.Direct
                        || c.InventorySource == InventorySourceEnum.PurchaseOrderReturn)).Sum(c => c.Stock);

                        if (purchaseOrderTotalStock - inventory.Stock == 0)
                        {
                            existingInventory.AveragePurchasePrice = 0;
                        }
                        else
                        {
                            existingInventory.AveragePurchasePrice =
                                ((existingInventory.AveragePurchasePrice * purchaseOrderTotalStock) - (inventory.PricePerUnit * inventory.Stock)) / (purchaseOrderTotalStock - inventory.Stock);
                        }
                        existingInventory.Stock -= inventory.Stock;
                        _inventoryHistoryRepository.Remove(existingPurchaseInventoryHistory);
                    }
                }
                else if (inventory.InventorySource == InventorySourceEnum.DeleteSalesOrder)
                {
                    var existingPurchaseInventoryHistory = await _inventoryHistoryRepository.All
                        .FirstOrDefaultAsync(c => inventory.ProductId == c.ProductId && inventory.SalesOrderId.HasValue && c.SalesOrderId == inventory.SalesOrderId
                        );
                    if (existingPurchaseInventoryHistory != null)
                    {
                        var salesOrderTotalStock = _inventoryHistoryRepository.All
                            .Where(c => (c.InventorySource == InventorySourceEnum.SalesOrder || c.InventorySource == InventorySourceEnum.SalesOrderReturn)
                            && c.Year == inventory.Year && c.Month == inventory.Month && c.ProductId == inventory.ProductId).Sum(c => c.Stock);

                        if (salesOrderTotalStock + inventory.Stock == 0)
                        {
                            existingInventory.AverageSalesPrice = 0;
                        }
                        else
                        {
                            existingInventory.AverageSalesPrice =
                                ((-1) * (existingInventory.AverageSalesPrice * salesOrderTotalStock) - (inventory.PricePerUnit * inventory.Stock)) / ((-1) * salesOrderTotalStock - inventory.Stock);
                        }
                        existingInventory.Stock += inventory.Stock;
                        _inventoryHistoryRepository.Remove(existingPurchaseInventoryHistory);
                    }
                }
                else if (inventory.InventorySource == InventorySourceEnum.PurchaseOrder || inventory.InventorySource == InventorySourceEnum.Direct)
                {
                    var existingPurchaseInventoryHistory = await _inventoryHistoryRepository.All.FirstOrDefaultAsync(c => inventory.ProductId == c.ProductId && inventory.PurchaseOrderId.HasValue && c.PurchaseOrderId == inventory.PurchaseOrderId);
                    var purchaseOrderTotalStock = _inventoryHistoryRepository.All.Where(c => c.ProductId == inventory.ProductId
                    && (c.InventorySource == InventorySourceEnum.PurchaseOrder || c.InventorySource == InventorySourceEnum.Direct || c.InventorySource == InventorySourceEnum.PurchaseOrderReturn)).Sum(c => c.Stock);
                    if (existingPurchaseInventoryHistory != null)
                    {
                        if (existingPurchaseInventoryHistory.PricePerUnit != inventory.PricePerUnit)
                        {
                            var stock = purchaseOrderTotalStock - existingPurchaseInventoryHistory.Stock + inventory.Stock;
                            existingInventory.AveragePurchasePrice =
                                Math.Abs((existingInventory.AveragePurchasePrice * purchaseOrderTotalStock - existingPurchaseInventoryHistory.PricePerUnit * existingPurchaseInventoryHistory.Stock + inventory.PricePerUnit * inventory.Stock)
                                / (stock == 0 ? Math.Abs(purchaseOrderTotalStock) : stock));
                            existingPurchaseInventoryHistory.PricePerUnit = inventory.PricePerUnit;
                        }

                        if (existingPurchaseInventoryHistory.Stock != inventory.Stock)
                        {
                            existingInventory.Stock = existingInventory.Stock - existingPurchaseInventoryHistory.Stock + inventory.Stock;
                            existingPurchaseInventoryHistory.Stock = inventory.Stock;
                        }
                        _inventoryHistoryRepository.Update(existingPurchaseInventoryHistory);
                    }
                    else
                    {
                        _inventoryHistoryRepository.Add(new InventoryHistory
                        {
                            ProductId = inventory.ProductId,
                            InventorySource = inventory.InventorySource,
                            Stock = inventory.Stock,
                            PricePerUnit = inventory.PricePerUnit,
                            PreviousTotalStock = existingInventory.Stock,
                            SalesOrderId = inventory.SalesOrderId,
                            PurchaseOrderId = inventory.PurchaseOrderId,

                            PurchasePrice = inventory.PurchasePrice,
                            Mrp = inventory.Mrp,
                            Margin = inventory.Margin,
                            Year = inventory.Year,
                            Month = inventory.Month
                        });
                        existingInventory.AveragePurchasePrice =
                     (existingInventory.AveragePurchasePrice * purchaseOrderTotalStock + inventory.PricePerUnit * inventory.Stock) / (purchaseOrderTotalStock + inventory.Stock);
                        existingInventory.Stock += inventory.Stock;
                    }
                }
                else if (inventory.InventorySource == InventorySourceEnum.PurchaseOrderReturn)
                {
                    existingInventory.Stock = existingInventory.Stock - inventory.Stock;
                    _inventoryHistoryRepository.Add(new InventoryHistory
                    {
                        ProductId = inventory.ProductId,
                        InventorySource = inventory.InventorySource,
                        Stock = (-1) * inventory.Stock,
                        PricePerUnit = inventory.PricePerUnit,
                        PreviousTotalStock = existingInventory.Stock,
                        SalesOrderId = inventory.SalesOrderId,
                        PurchaseOrderId = inventory.PurchaseOrderId,

                        PurchasePrice = inventory.PurchasePrice,
                        Mrp = inventory.Mrp,
                        Margin = inventory.Margin,
                        Year = inventory.Year,
                        Month = inventory.Month
                    });
                }
                else if (inventory.InventorySource == InventorySourceEnum.SalesOrderReturn)
                {
                    existingInventory.Stock = existingInventory.Stock + inventory.Stock;
                    _inventoryHistoryRepository.Add(new InventoryHistory
                    {
                        ProductId = inventory.ProductId,
                        InventorySource = inventory.InventorySource,
                        Stock = inventory.Stock,
                        PricePerUnit = inventory.PricePerUnit,
                        PreviousTotalStock = existingInventory.Stock,
                        SalesOrderId = inventory.SalesOrderId,
                        PurchaseOrderId = inventory.PurchaseOrderId,

                        PurchasePrice = inventory.PurchasePrice,
                        Mrp = inventory.Mrp,
                        Margin = inventory.Margin,
                        Year = inventory.Year,
                        Month = inventory.Month
                    });
                }
                else
                {
                    var existingSalesInventoryHistory = await _inventoryHistoryRepository.All.FirstOrDefaultAsync(c => inventory.ProductId == c.ProductId && inventory.SalesOrderId.HasValue && c.SalesOrderId == inventory.SalesOrderId);
                    var salesOrderTotalStock = _inventoryHistoryRepository.All.Where(c => (c.InventorySource == InventorySourceEnum.SalesOrder || c.InventorySource == InventorySourceEnum.SalesOrderReturn) && c.ProductId == inventory.ProductId
                    && c.Year == inventory.Year && c.Month == inventory.Month).Sum(c => c.Stock);
                    if (existingSalesInventoryHistory != null)
                    {
                        var stock = salesOrderTotalStock - existingSalesInventoryHistory.Stock + inventory.Stock;
                        if (existingSalesInventoryHistory.PricePerUnit != inventory.PricePerUnit)
                        {
                            existingInventory.AverageSalesPrice =
                            Math.Abs((existingInventory.AverageSalesPrice * salesOrderTotalStock - ((-1) * existingSalesInventoryHistory.Stock) * existingSalesInventoryHistory.PricePerUnit + inventory.PricePerUnit * inventory.Stock)
                            / (stock == 0 ? Math.Abs(salesOrderTotalStock) : stock));
                            existingSalesInventoryHistory.PricePerUnit = inventory.PricePerUnit;
                        }

                        if (existingSalesInventoryHistory.Stock != inventory.Stock)
                        {
                            existingInventory.Stock = existingInventory.Stock + ((-1) * existingSalesInventoryHistory.Stock) - inventory.Stock;
                            existingSalesInventoryHistory.Stock = (-1) * inventory.Stock;
                        }
                        _inventoryHistoryRepository.Update(existingSalesInventoryHistory);
                    }
                    else
                    {
                        _inventoryHistoryRepository.Add(new InventoryHistory
                        {
                            ProductId = inventory.ProductId,
                            InventorySource = inventory.InventorySource,
                            Stock = (-1) * inventory.Stock,
                            PricePerUnit = inventory.PricePerUnit,
                            PreviousTotalStock = existingInventory.Stock,
                            SalesOrderId = inventory.SalesOrderId,
                            PurchaseOrderId = inventory.PurchaseOrderId,

                            PurchasePrice = inventory.PurchasePrice,
                            Mrp = inventory.Mrp,
                            Margin = inventory.Margin,
                            Year = inventory.Year,
                            Month = inventory.Month
                    });
                        salesOrderTotalStock = Math.Abs(salesOrderTotalStock);
                        existingInventory.AverageSalesPrice =
                             Math.Abs((existingInventory.AverageSalesPrice * salesOrderTotalStock + inventory.PricePerUnit * inventory.Stock) / (salesOrderTotalStock + inventory.Stock));
                        existingInventory.Stock -= inventory.Stock;
                    }

                }
                Update(existingInventory);
            }
        }

        public async Task<InventoryList> GetInventories(InventoryResource inventoryResource)
        {
            var collectionBeforePaging =
                AllIncluding(c => c.Product, u => u.Product.Unit, b => b.Product.Brand, s => s.Product.Supplier, cs => cs.Product.ProductCategory).ApplySort(inventoryResource.OrderBy,
                _propertyMappingService.GetPropertyMapping<InventoryDto, Inventory>());

            if (!string.IsNullOrWhiteSpace(inventoryResource.ProductName))
            {
                // trim & ignore casing
                var genreForWhereClause = inventoryResource.ProductName
                    .Trim().ToLowerInvariant();
                var name = Uri.UnescapeDataString(genreForWhereClause);
                var encodingName = WebUtility.UrlDecode(name);
                var ecapestring = Regex.Unescape(encodingName);
                encodingName = encodingName.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[").Replace(" ", "%");
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => EF.Functions.Like(a.Product.Name, $"{encodingName}%"));
            }

            if (!string.IsNullOrEmpty(inventoryResource.BrandName))
            {
                // trim & ignore casing
                var genreForWhereClause = inventoryResource.BrandName
                    .Trim().ToLowerInvariant();
                var name = Uri.UnescapeDataString(genreForWhereClause);
                var encodingName = WebUtility.UrlDecode(name);
                var ecapestring = Regex.Unescape(encodingName);
                encodingName = encodingName.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[").Replace(" ", "%");
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => EF.Functions.Like(a.Product.Brand.Name, $"{encodingName}%"));
            }

            if (!string.IsNullOrEmpty(inventoryResource.ProductCategoryName))
            {
                // trim & ignore casing
                var genreForWhereClause = inventoryResource.ProductCategoryName
                    .Trim().ToLowerInvariant();
                var name = Uri.UnescapeDataString(genreForWhereClause);
                var encodingName = WebUtility.UrlDecode(name);
                var ecapestring = Regex.Unescape(encodingName);
                encodingName = encodingName.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[").Replace(" ", "%");
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => EF.Functions.Like(a.Product.ProductCategory.Name, $"{encodingName}%"));
            }

            if (!string.IsNullOrEmpty(inventoryResource.ProductCode))
            {
                // trim & ignore casing
                var genreForWhereClause = inventoryResource.ProductCode
                    .Trim().ToLowerInvariant();
                var name = Uri.UnescapeDataString(genreForWhereClause);
                var encodingName = WebUtility.UrlDecode(name);
                var ecapestring = Regex.Unescape(encodingName);
                encodingName = encodingName.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[").Replace(" ", "%");
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => EF.Functions.Like(a.Product.Code, $"{encodingName}%"));
            }

            if (!string.IsNullOrEmpty(inventoryResource.SupplierName))
            {
                // trim & ignore casing
                var genreForWhereClause = inventoryResource.SupplierName
                    .Trim().ToLowerInvariant();
                var name = Uri.UnescapeDataString(genreForWhereClause);
                var encodingName = WebUtility.UrlDecode(name);
                var ecapestring = Regex.Unescape(encodingName);
                encodingName = encodingName.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[").Replace(" ", "%");
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => EF.Functions.Like(a.Product.PurchaseOrderItems.PurchaseOrder.Supplier.SupplierName, $"{encodingName}%"));
            }

            if (inventoryResource.SupplierId.HasValue)
            {
                collectionBeforePaging = collectionBeforePaging
                   .Where(a => a.Product.Supplier.Id == inventoryResource.SupplierId);
            }

            if (inventoryResource.ProductMainCategoryId.HasValue)
            {

                collectionBeforePaging = collectionBeforePaging
                   .Where(a => a.Product.ProductCategory.ProductMainCategoryId == inventoryResource.ProductMainCategoryId);
            }

            if (inventoryResource.Year <= 0 || inventoryResource.Year == null && inventoryResource.Month <= 0 || inventoryResource.Month == null)
            {
                inventoryResource.Year = DateTime.Now.Year;
                inventoryResource.Month = DateTime.Now.Month;
            }

            if (inventoryResource.Year > 0 && inventoryResource.Month > 0 )
            { 
                collectionBeforePaging = collectionBeforePaging
                   .Where(a => a.Year == inventoryResource.Year && a.Month == inventoryResource.Month);
            }
            //Negative Stock Find
            if (inventoryResource.IsNegativeStock==true)
            {
                collectionBeforePaging = collectionBeforePaging
                   .Where(a => a.Stock <0);
            }


            var inventoryList = new InventoryList(_mapper);
            return await inventoryList.Create(collectionBeforePaging, inventoryResource.Skip, inventoryResource.PageSize, inventoryResource.DefaultDate);
        }

        public InventoryDto ConvertStockAndPriceToBaseUnit(InventoryDto inventory)
        {
            var unit = _unitConversationRepository.AllIncluding(c => c.Parent)
                .FirstOrDefault(c => c.Id == inventory.UnitId);

            if (unit.ParentId.HasValue && unit.Operator.HasValue && unit.Value.HasValue)
            {
                switch (unit.Operator)
                {
                    case Operator.Plush:
                        inventory.Stock = inventory.Stock + unit.Value.Value;
                        inventory.PricePerUnit = inventory.PricePerUnit - unit.Value.Value;
                        break;
                    case Operator.Minus:
                        inventory.Stock = inventory.Stock - unit.Value.Value;
                        inventory.PricePerUnit = inventory.PricePerUnit + unit.Value.Value;
                        break;
                    case Operator.Multiply:
                        inventory.Stock = inventory.Stock * unit.Value.Value;
                        inventory.PricePerUnit = inventory.PricePerUnit / unit.Value.Value;
                        break;
                    case Operator.Divide:
                        inventory.Stock = inventory.Stock / unit.Value.Value;
                        inventory.PricePerUnit = inventory.PricePerUnit * unit.Value.Value;
                        break;
                    default:
                        break;
                }
            }

            return inventory;
        }

        public async Task AddWarehouseInventory(InventoryDto inventory)
        {
            if (!inventory.WarehouseId.HasValue)
            {
                return;
            }
            var existingInventory = await _warehouseInventoryRepository.All.FirstOrDefaultAsync(c => c.WarehouseId == inventory.WarehouseId.Value && c.ProductId == inventory.ProductId);
            if (existingInventory == null)
            {
                _warehouseInventoryRepository.Add(new WarehouseInventory
                {
                    ProductId = inventory.ProductId,
                    Stock = inventory.InventorySource == InventorySourceEnum.SalesOrder ? (-1) * inventory.Stock : inventory.Stock,
                    WarehouseId = inventory.WarehouseId.Value
                });
            }
            else
            {
                if (inventory.InventorySource == InventorySourceEnum.DeletePurchaseOrder)
                {
                    existingInventory.Stock -= inventory.Stock;
                }
                else if (inventory.InventorySource == InventorySourceEnum.DeleteSalesOrder)
                {
                    existingInventory.Stock += inventory.Stock;
                }
                else if (inventory.InventorySource == InventorySourceEnum.PurchaseOrder || inventory.InventorySource == InventorySourceEnum.Direct)
                {
                    existingInventory.Stock += inventory.Stock;
                }
                else if (inventory.InventorySource == InventorySourceEnum.PurchaseOrderReturn)
                {
                    existingInventory.Stock -= inventory.Stock;
                }
                else if (inventory.InventorySource == InventorySourceEnum.SalesOrderReturn)
                {
                    existingInventory.Stock += inventory.Stock;
                }
                else
                {
                    existingInventory.Stock -= inventory.Stock; //Sales order
                }

                var local = _uow.Context.Set<WarehouseInventory>().Local
                    .FirstOrDefault(entry => entry.Id.Equals(existingInventory.Id));

                if (local != null)
                {
                    _uow.Context.Entry(local).State = EntityState.Detached;
                }

                _warehouseInventoryRepository.Update(existingInventory);
            }
        }

        public async Task RemoveExistingWareHouseInventory(List<InventoryDto> inventories)
        {
            var updatedInvetories = inventories
                .GroupBy(c => new { c.ProductId, c.WarehouseId })
                .Select(cs => new InventoryDto
                {
                    ProductId = cs.Key.ProductId,
                    Stock = cs.Sum(d => d.Stock),
                    WarehouseId = cs.Key.WarehouseId
                }).ToList();

            foreach (var inventory in updatedInvetories)
            {
                var existingInventory = await _warehouseInventoryRepository.All
                    .FirstOrDefaultAsync(c => c.ProductId == inventory.ProductId && c.WarehouseId == inventory.WarehouseId);
                if (existingInventory != null)
                {
                    existingInventory.Stock -= inventory.Stock;
                    _warehouseInventoryRepository.Update(existingInventory);
                }
            }
        }
    }
}
