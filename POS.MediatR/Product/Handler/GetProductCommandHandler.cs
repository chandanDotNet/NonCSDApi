﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POS.Data.Dto;
using POS.Helper;
using POS.MediatR.Product.Command;
using POS.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POS.MediatR.Product.Handler
{
    public class GetProductCommandHandler : IRequestHandler<GetProductCommand, ServiceResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<GetProductCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly PathHelper _pathHelper;
        private readonly IInventoryRepository _iInventoryRepository;
        private readonly ICartRepository _icartRepository;

        public GetProductCommandHandler(IProductRepository productRepository,
            ILogger<GetProductCommandHandler> logger,
            IMapper mapper,
            PathHelper pathHelper,
        IInventoryRepository iInventoryRepository,
        ICartRepository icartRepository)
        {
            _productRepository = productRepository;
            _logger = logger;
            _mapper = mapper;
            _pathHelper = pathHelper;
            _iInventoryRepository = iInventoryRepository;
            _icartRepository = icartRepository;
        }

        public async Task<ServiceResponse<ProductDto>> Handle(GetProductCommand request, CancellationToken cancellationToken)
        {
            //var product = await _productRepository.AllIncluding(c => c.ProductTaxes, p => p.Packaging, (i => i.Inventory), u => u.Unit).FirstOrDefaultAsync(c => c.Id == request.Id);
            var product = await _productRepository.AllIncluding(c => c.ProductTaxes, p => p.Packaging, u => u.Unit).FirstOrDefaultAsync(c => c.Id == request.Id);
            if (product == null)
            {
                _logger.LogError("Not found");
                return ServiceResponse<ProductDto>.Return404();
            }
            if (!string.IsNullOrWhiteSpace(product.ProductUrl))
            {
                product.ProductUrl = Path.Combine(_pathHelper.ProductImagePath, product.ProductUrl);
            }
            if (!string.IsNullOrWhiteSpace(product.QRCodeUrl))
            {
                product.QRCodeUrl = Path.Combine(_pathHelper.ProductImagePath, product.QRCodeUrl);
            }

            var invData = _iInventoryRepository.All.Where(i => i.ProductId == request.Id && i.Month == DateTime.Now.Month && i.Year == DateTime.Now.Year).FirstOrDefault();
            if (invData != null)
            {
                product.Inventory = _mapper.Map<Data.Inventory>(invData);
            }

            //decimal stock = 0;
            //if (invData.Count > 0)
            //{
            //    //stock = _iInventoryRepository.All.Where(i => i.ProductId == request.Id && i.Month == DateTime.Now.Month && i.Year == DateTime.Now.Year).FirstOrDefault().Stock == null ? 0 : _iInventoryRepository.All.Where(i => i.ProductId == request.Id && i.Month == DateTime.Now.Month && i.Year == DateTime.Now.Year).FirstOrDefault().Stock;
            //    stock = _iInventoryRepository.All.Where(i => i.ProductId == request.Id && i.Month == DateTime.Now.Month && i.Year == DateTime.Now.Year).FirstOrDefault().Stock;
            //}

            int noOfItems = 0;
            Guid CartId = new Guid();
            var cartData = _icartRepository.All.Where(i => i.ProductId == request.Id && i.CustomerId == request.CustomerId).ToList();
            if (cartData.Count > 0)
            {
                noOfItems = _icartRepository.All.Where(i => i.ProductId == request.Id && i.CustomerId == request.CustomerId).FirstOrDefault().NoOfItems;
                CartId = _icartRepository.All.Where(i => i.ProductId == request.Id && i.CustomerId == request.CustomerId).FirstOrDefault().Id;
            }

            var productRes = _mapper.Map<ProductDto>(product);

            if (productRes != null)
            {
                //productRes.Stock = stock;
                if (productRes.Inventory == null)
                {
                    productRes.Inventory = new InventoryDto();
                }
                productRes.PackagingName = product.Packaging == null ? "" : product.Packaging.Name;
                if (productRes.MinQty == null)
                {
                    productRes.MinQty = 0;
                }
                productRes.NoOfItemsInCart = noOfItems;
                productRes.CartId = CartId;
            }

            return ServiceResponse<ProductDto>.ReturnResultWith200(productRes);
        }
    }
}
