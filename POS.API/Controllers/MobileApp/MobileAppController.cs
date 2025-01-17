﻿using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POS.Data.Dto;
using POS.Data;
using POS.MediatR.CommandAndQuery;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using static POS.API.Controllers.Counter.CounterController;
using System.Collections.Generic;
using System.Linq;
using POS.MediatR.Commands;
using POS.API.Helpers;
using System;
using POS.MediatR.Country.Command;
using POS.MediatR.Counter.Commands;
using Azure;
using Microsoft.AspNetCore.Authorization;
using POS.Data.Entities;
using POS.Data.Resources;
using POS.MediatR.Product.Command;
using POS.MediatR.CustomerAddress.Commands;
using POS.MediatR.Country.Commands;
using POS.MediatR.Cart;
using POS.MediatR.PaymentCard.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using POS.MediatR.Reminder.Commands;
using POS.MediatR.SalesOrder.Commands;
using POS.MediatR.Cart.Commands;
using POS.MediatR.Banner.Command;
using POS.MediatR.Brand.Command;
using System.Security.Claims;
using System.Linq.Dynamic.Core.Tokenizer;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using POS.MediatR.Inventory.Command;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System.Data;
using System.IO;
using System.Reflection.PortableExecutable;
using ExcelDataReader;
using POS.MediatR.ExcelUpload.Command;
using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using POS.Helper;
using POS.MediatR.NonCSDCanteen.Command;
using POS.Data.Dto.GRN;
using POS.MediatR.SalesOrderPayment.Command;
using POS.Repository;
using POS.MediatR.Supplier.Commands;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using static System.Net.Mime.MediaTypeNames;
using FirebaseAdmin.Messaging;
using POS.MediatR.Notice.Command;
using static Google.Apis.Requests.BatchRequest;
using POS.MediatR.ShopHoliday.Command;
using iText.Kernel.Pdf;
using iText.Layout.Properties;
using iText.Layout;
using iText.Layout.Element;
using static iText.IO.Util.IntHashtable;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout.Borders;
using iText.IO.Image;
using static iText.Svg.SvgConstants;
using System.ComponentModel;
using iText.Kernel.Colors;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using System.IO.Pipelines;
using POS.MediatR.MSTBSetting.Command;
using POS.MediatR.PurchaseOrderMSTB.Command;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using Org.BouncyCastle.Asn1.Ocsp;
using POS.MediatR.PurchaseOrder.Commands;
using AutoMapper;

namespace POS.API.Controllers.MobileApp
{
    //[Route("api/[controller]")]
    [Route("api")]
    [ApiController]
    [Authorize]
    public class MobileAppController : BaseController
    {
        public IMediator _mediator { get; set; }
        IExcelDataReader reader;
        private readonly PathHelper _pathHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IProductRepository _productRepository;
        private readonly IUnitConversationRepository _unitConversationRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICustomerRepository _customerRepository;
        private IConfiguration Configuration;
        private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IMapper _mapper;
        public MobileAppController(IMediator mediator, PathHelper pathHelper,
            IWebHostEnvironment webHostEnvironment, IProductRepository productRepository, IUnitConversationRepository unitConversationRepository, IWarehouseRepository warehouseRepository, ICustomerRepository customerRepository,
            IConfiguration _configuration, IPurchaseOrderItemRepository purchaseOrderItemRepository, IPurchaseOrderRepository purchaseOrderRepository, IMapper mapper)
        {
            _mediator = mediator;
            _pathHelper = pathHelper;
            _webHostEnvironment = webHostEnvironment;
            _productRepository = productRepository;
            _unitConversationRepository = unitConversationRepository;
            _warehouseRepository = warehouseRepository;
            _customerRepository = customerRepository;
            Configuration = _configuration;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Login customers.       
        /// </summary>
        /// <param name="customerResource">The customer resource.</param>
        /// <returns></returns>
        [HttpPost("LoginCustomers")]
        [Produces("application/json", "application/xml", Type = typeof(CustomerDto))]
        public async Task<IActionResult> LoginCustomers(CustomerResource customerResource)
        {
            //CustomerDto
            CustomerResponseData response = new CustomerResponseData();
            var query = new LoginCustomerQuery
            {
                CustomerResource = customerResource
            };
            var customersFromRepo = await _mediator.Send(query);

            if (customersFromRepo.Count > 0)
            {
                //Update OTP
                //var cus = customersFromRepo.FirstOrDefault();
                //UpdateCustomerCommand updateCustomerCommand = new UpdateCustomerCommand();
                //updateCustomerCommand.Id = cus.Id;
                //updateCustomerCommand.OTP = 1010;
                //updateCustomerCommand.CustomerName = cus.CustomerName;
                //updateCustomerCommand.Email = cus.Email;
                //var Updateresponse = await _mediator.Send(updateCustomerCommand);
                //*************************

                //customersFromRepo.FirstOrDefault().OTP = 1234;
                //UpdateCustomerCommand updateCustomerCommand = new UpdateCustomerCommand();
                var customer = await _customerRepository.FindAsync(customersFromRepo.FirstOrDefault().Id.Value);

                int _min = 1000;
                int _max = 9999;
                Random rnd = new Random();
                customer.OTP = rnd.Next(_min, _max);
              
                string smsGateWay = this.Configuration.GetSection("AppSettings")["SmsGateway"];
                if (smsGateWay == "SGSMS")
                {
                    string smsResponse = SendOTPMessage(customer.MobileNo, customer.OTP);
                }

                UpdateCustomerCommand updateCustomerCommand = new UpdateCustomerCommand()
                {
                    Id = customersFromRepo.FirstOrDefault().Id,
                    DeviceKey = customerResource.DeviceKey,
                    AadharCard = customer.AadharCard,
                    Address = customer.Address,
                    Category = customer.Category,
                    CityId = customer.CityId,
                    CityName = customer.CityName,
                    ContactPerson = customer.ContactPerson,
                    CountryId = customer.CountryId,
                    CountryName = customer.CountryName,
                    CustomerName = customer.CustomerName,
                    CustomerProfile = customer.CustomerProfile,
                    DependantCard = customer.DependantCard,
                    Description = customer.Description,
                    Email = customer.Email,
                    Fax = customer.Fax,
                    IsVarified = customer.IsVarified,
                    OTP = customer.OTP,
                    MobileNo = customer.MobileNo,
                    PinCode = customer.PinCode,
                    PhoneNo = customer.PhoneNo,
                    Website = customer.Website,
                    Url = customer.Url,
                    Password = customer.Password,
                    ServiceNo = customer.ServiceNo,
                    RewardPoints = customer.RewardPoints
                };
                var result = await _mediator.Send(updateCustomerCommand);

                customersFromRepo.FirstOrDefault().OTP = customer.OTP;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Logged in successfully!";
                response.Data = customersFromRepo.FirstOrDefault();
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Mobile number not exist.";
                response.Data = new CustomerDto { };
            }

            return Ok(response);
        }

        /// <summary>
        /// OTP Verify the customer.
        /// </summary>        
        /// <param name="customerResource">The customer resource.</param>
        /// <returns></returns>
        [HttpPost("CustomersOTPVerify")]
        [Produces("application/json", "application/xml", Type = typeof(CustomerDto))]
        public async Task<IActionResult> CustomersOTPVerify(CustomerResource customerResource)
        {
            //CustomerDto
            CustomerResponseData response = new CustomerResponseData();
            var query = new LoginCustomerQuery
            {
                CustomerResource = customerResource
            };
            var customersFromRepo = await _mediator.Send(query);

            if (customersFromRepo.Count > 0)
            {

                if (customersFromRepo.FirstOrDefault().OTP == customerResource.OTP)
                {

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "OTP verified successfully!";
                    response.Data = customersFromRepo.FirstOrDefault();
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid OTP";
                    response.Data = new CustomerDto { };
                }
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid OTP";
                response.Data = customersFromRepo.FirstOrDefault();
            }

            return Ok(response);

        }

        /// <summary>
        /// Get Non CSD List.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNonCSDList")]
        [Produces("application/json", "application/xml", Type = typeof(List<NonCSDCanteenDto>))]
        public async Task<IActionResult> GetNonCSDList()
        {
            NonCSDResponseNameData response = new NonCSDResponseNameData();
            var getAllNonCSDCanteenCommand = new GetAllNonCSDCanteenCommand { };
            var result = await _mediator.Send(getAllNonCSDCanteenCommand);

            if (result.Count > 0)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
                response.Data = result;
            }

            return Ok(response);
        }


        /// <summary>
        /// Get Non CSD Canteen.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetNonCSDCanteen/{id}", Name = "GetNonCSDCanteen")]
        [Produces("application/json", "application/xml", Type = typeof(NonCSDCanteenDto))]
        public async Task<IActionResult> GetNonCSDCanteen(Guid id)
        {
            var getNonCSDCanteenCommand = new GetNonCSDCanteenCommand { Id = id };
            var result = await _mediator.Send(getNonCSDCanteenCommand);
            return ReturnFormattedResponse(result);
        }

        /// <summary>
        /// Get All Products List.
        /// </summary>
        /// <param name="productResource"></param>
        /// <returns></returns>
        [HttpPost("GetProductsList")]
        public async Task<IActionResult> GetProductsList(ProductResource productResource)
        {
            ProductListResponseData response = new ProductListResponseData();
            try
            {
                if (productResource.Skip > 0)
                {
                    productResource.Skip = productResource.PageSize * productResource.Skip;
                }
                var getAllProductCommand = new GetAllProductCommand
                {
                    ProductResource = productResource
                };

                var result = await _mediator.Send(getAllProductCommand);

                //List<ProductDto> myResult = new List<ProductDto>();
                //if (productResource.BrandNameFilter != null)
                //{
                //    myResult = result.Where(x => x.BrandName != "Baggage" && x.BrandName != "Delivery").ToList();
                //}

                if (result.Count > 0)
                {
                    response.PageSize = result.TotalCount;
                    response.PageSize = result.PageSize;
                    response.Skip = result.Skip;
                    response.TotalPages = result.TotalPages;

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "failed.";
                    response.Data = result;

                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);

        }

        /// <summary>
        /// Get Product Details.
        /// </summary>       
        /// <returns></returns>
        [HttpPost("GetProductDetails")]
        [Produces("application/json", "application/xml", Type = typeof(ProductDto))]
        public async Task<IActionResult> GetProductDetails(ProductDetailsRequestData productRequestData)
        {
            ProductDetailsResponseData response = new ProductDetailsResponseData();
            if (productRequestData.Id == null || productRequestData.Id == "")
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid Product Id";

            }
            else
            {
                try
                {
                    Guid id = new Guid(productRequestData.Id);
                    if (!productRequestData.CustomerId.HasValue)
                    {
                        productRequestData.CustomerId = new Guid();
                    }
                    //Guid customerid = new Guid(productRequestData.CustomerId);
                    var getProductCommand = new GetProductCommand { Id = id, CustomerId = productRequestData.CustomerId.Value };
                    var result = await _mediator.Send(getProductCommand);

                    if (result.Success)
                    {
                        ProductResource similarProductResource = new ProductResource()
                        {
                            ProductId = id,
                            ProductTypeId = result.Data.ProductTypeId
                        };

                        var getAllProductCommand = new GetAllProductCommand
                        {
                            ProductResource = similarProductResource
                        };
                        var similarProductResult = await _mediator.Send(getAllProductCommand);

                        response.status = true;
                        response.StatusCode = 1;
                        response.message = "Success";
                        response.Data = result.Data;
                        if (similarProductResult.Count > 0)
                        {
                            response.SimilarProductData = similarProductResult;
                        }
                        else
                        {
                            response.SimilarProductData = new List<ProductDto>();
                        }
                    }
                    else
                    {
                        response.status = false;
                        response.StatusCode = 0;
                        response.message = "Please wait! Server is not responding.";
                        response.Data = new ProductDto { };
                    }
                }
                catch (Exception ex)
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = ex.Message;

                }
            }
            return Ok(response);

        }

        /// <summary>
        /// Get all Product Categories
        /// </summary>
        /// <param name="getAllProductCategoriesQuery"></param>
        /// <returns></returns>
        //[HttpGet]
        [HttpPost("ProductCategoriesList")]
        [Produces("application/json", "application/xml", Type = typeof(List<ProductCategoryDto>))]
        public async Task<IActionResult> ProductCategoriesList([FromBody] GetAllProductCategoriesQuery getAllProductCategoriesQuery)
        {
            ProductCategoriesResponseData response = new ProductCategoriesResponseData();
            try
            {
                var result = await _mediator.Send(getAllProductCategoriesQuery);
                result = result.Where(x => x.Name.ToLower() != "BAGGAGE".ToLower() && x.ProductMainCategoryId == getAllProductCategoriesQuery.ProductMainCategoryId).ToList();

                var count = await GetProductCount(getAllProductCategoriesQuery.ProductMainCategoryId);

                //var returnVal = await GetStoreOpenClose();

                if (result.Count > 0)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    //response.StoreOpenClose = returnVal;
                    response.productCount = count;
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Please wait! Server is not responding.";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Creates the cart.
        /// </summary>
        /// <param name="addCartCommand">The add cart command.</param>
        /// <returns></returns>
        //[HttpPost, DisableRequestSizeLimit]
        [HttpPost("AddToCart")]
        public async Task<IActionResult> CreateCart([FromBody] AddCartCommand addCartCommand)
        {
            IUDResponseData response = new IUDResponseData();
            var result = await _mediator.Send(addCartCommand);

            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Product has been added to your cart";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";

            }
            return Ok(response);
        }

        /// <summary>
        /// Updates the cart.
        /// </summary>

        /// <param name="updateCartCommand">The update cart command.</param>
        /// <returns></returns>
        [HttpPut("UpdateToCart")]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartCommand updateCartCommand)
        {
            IUDResponseData response = new IUDResponseData();
            //updateCustomerCommand.Id = id;
            var result = await _mediator.Send(updateCartCommand);
            //return ReturnFormattedResponse(response);
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";

            }
            return Ok(response);
        }

        /// <summary>
        /// Get All Cart List.
        /// </summary>
        /// <param name="cartResource"></param>
        /// <returns></returns>
        [HttpPost("GetCartList")]
        public async Task<IActionResult> GetCartList(CartResource cartResource)
        {

            CartListResponseData response = new CartListResponseData();

            try
            {
                var query = new GetAllCartQuery
                {
                    CartResource = cartResource
                };
                var result = await _mediator.Send(query);

                if (result.Count > 0)
                {
                    response.TotalCount = result.TotalCount;
                    response.PageSize = result.PageSize;
                    response.Skip = result.Skip;
                    response.TotalPages = result.TotalPages;


                    result.ForEach(item =>
                    {
                        decimal value = (decimal)(item.UnitPrice) * item.Quantity;
                        int roundedValue = (int)Math.Round(value, MidpointRounding.AwayFromZero);
                        item.Total = roundedValue;
                    });

                    var price = Math.Round(result.Sum(x => x.Total), MidpointRounding.AwayFromZero).ToString("0.00");
                    //var price = result.Sum(x => x.Total);
                    var discount = result.Sum(x => x.Discount);
                    var items = result.Sum(x => x.Quantity);
                    var deliveryCharges = 0;

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                    response.Data = result;

                }

            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Create Customer Address.
        /// </summary>
        /// <param name="addCustomerAddressCommand"></param>
        /// <returns></returns>
        [HttpPost("CustomerAddress")]
        [Produces("application/json", "application/xml", Type = typeof(CustomerAddressDto))]
        public async Task<IActionResult> AddCustomerAddress(AddCustomerAddressCommand addCustomerAddressCommand)
        {
            var result = await _mediator.Send(addCustomerAddressCommand);
            if (!result.Success)
            {
                return ReturnFormattedResponse(result);
            }
            //return CreatedAtAction("GetCustomerAddress", new { customerId = response.Data.CustomerId }, response.Data);
            CustomerAddressResponseData response = new CustomerAddressResponseData();

            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Your address added successfully!";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Customer Address.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("CustomerAddress/{customerId}", Name = "GetCustomerAddress")]
        [Produces("application/json", "application/xml", Type = typeof(CustomerAddressDto))]
        public async Task<IActionResult> GetCustomerAddress(Guid customerId)
        {
            var getCustomerAddressCommand = new GetCustomerAddressCommand { CustomerId = customerId };
            var result = await _mediator.Send(getCustomerAddressCommand);
            //return ReturnFormattedResponse(result);

            CustomerAddressResponseData response = new CustomerAddressResponseData();
            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Customer Addresses
        /// </summary>
        /// <param name="customerAddressResource"></param>
        /// <returns></returns>

        [HttpGet("GetCustomerAddresses")]
        public async Task<IActionResult> GetCustomerAddresses([FromQuery] CustomerAddressResource customerAddressResource)
        {
            var getCustomerAddressQuery = new GetCustomerAddressQuery
            {
                CustomerAddressResource = customerAddressResource
            };
            var result = await _mediator.Send(getCustomerAddressQuery);

            var paginationMetadata = new
            {
                totalCount = result.TotalCount,
                pageSize = result.PageSize,
                skip = result.Skip,
                totalPages = result.TotalPages
            };
            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            CustomerAddressListResponseData response = new CustomerAddressListResponseData();
            if (result.Count > 0)
            {
                response.TotalCount = result.TotalCount;
                response.PageSize = result.PageSize;
                response.Skip = result.Skip;
                response.TotalPages = result.TotalPages;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete Customer Address.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("CustomerAddress/{id}")]
        public async Task<IActionResult> DeleteCustomerAddress(Guid Id)
        {
            var deleteCustomerAddressCommand = new DeleteCustomerAddressCommand { Id = Id };
            var result = await _mediator.Send(deleteCustomerAddressCommand);
            //return ReturnFormattedResponse(result);            
            CustomerAddressListResponseData response = new CustomerAddressListResponseData();
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Your address deleted successfully!";
                response.Data = new CustomerAddressDto[0];
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete Cart By Id
        /// </summary>
        /// <param name="deleteCartCommand">The delete cart command.</param>
        /// <returns></returns>
        [HttpDelete("DeleteToCart")]
        public async Task<IActionResult> DeleteCart(DeleteCartCommand deleteCartCommand)
        {
            IUDResponseData response = new IUDResponseData();
            if (deleteCartCommand.Id != null)
            {
                var command = new DeleteCartCommand { Id = deleteCartCommand.Id };
                var result = await _mediator.Send(command);
                if (result.Success)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Your cart deleted successfully!";
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Please wait! Server is not responding.";

                }
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid Cart Id";
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete All Cart By Customer Id
        /// </summary>
        /// <param name="deleteAllCartCommand">The delete customer cart command.</param>
        /// <returns></returns>
        [HttpDelete("DeleteCustomerCart")]
        public async Task<IActionResult> DeleteCustomerCart(DeleteCartByCustomerCommand deleteAllCartCommand)
        {
            IUDResponseData response = new IUDResponseData();
            if (deleteAllCartCommand.CustomerId != null)
            {
                var command = new DeleteCartByCustomerCommand { CustomerId = deleteAllCartCommand.CustomerId, ProductMainCategoryId = deleteAllCartCommand.ProductMainCategoryId };
                var result = await _mediator.Send(command);
                if (result.Success)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Your cart deleted successfully!";
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Please wait! Server is not responding.";
                }
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid Id";
            }

            return Ok(response);
        }

        /// <summary>
        /// Update Customer Address.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="updateCustomerAddressCommand"></param>
        /// <returns></returns>
        [HttpPut("CustomerAddress/{Id}")]
        [Produces("application/json", "application/xml", Type = typeof(CustomerAddressDto))]
        public async Task<IActionResult> UpdateCustomerAddress(Guid Id, UpdateCustomerAddressCommand updateCustomerAddressCommand)
        {
            updateCustomerAddressCommand.Id = Id;
            var result = await _mediator.Send(updateCustomerAddressCommand);
            //return ReturnFormattedResponse(result);           

            CustomerAddressResponseData response = new CustomerAddressResponseData();

            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Your addrsss updated successfully!";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
            }
            return Ok(response);
        }

        //Wishlist-list

        /// <summary>
        /// Creates the cart.
        /// </summary>
        /// <param name="addWishlistCommand">The add cart command.</param>
        /// <returns></returns>
        //[HttpPost, DisableRequestSizeLimit]
        [HttpPost("AddToWishlist")]
        public async Task<IActionResult> CreateWishlist([FromBody] AddWishlistCommand addWishlistCommand)
        {
            IUDResponseData response = new IUDResponseData();
            var result = await _mediator.Send(addWishlistCommand);

            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Product has been added to your wishlist!";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = response.message = result.Errors[0].ToString();

            }
            return Ok(response);
        }

        /// <summary>
        /// Get All Cart List.
        /// </summary>
        /// <param name="wishlistResource"></param>
        /// <returns></returns>
        [HttpPost("GetWishlist")]
        public async Task<IActionResult> GetWishlist(WishlistResource wishlistResource)
        {

            WishlistResponseData response = new WishlistResponseData();

            try
            {
                var query = new GetAllWishlistQuery
                {
                    WishlistResource = wishlistResource
                };
                var result = await _mediator.Send(query);

                if (result.Count > 0)
                {
                    response.TotalCount = result.TotalCount;
                    response.PageSize = result.PageSize;
                    response.Skip = result.Skip;
                    response.TotalPages = result.TotalPages;

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                    response.Data = result;

                }

            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete Wishlist By Id
        /// </summary>
        /// <param name="deleteWishlistCommand">The delete cart command.</param>
        /// <returns></returns>
        [HttpDelete("DeleteWishlist")]
        public async Task<IActionResult> DeleteWishlist(DeleteWishlistCommand deleteWishlistCommand)
        {
            IUDResponseData response = new IUDResponseData();
            if (deleteWishlistCommand.Id != null)
            {
                var command = new DeleteWishlistCommand { Id = deleteWishlistCommand.Id };
                var result = await _mediator.Send(command);
                if (result.Success)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Please wait! Server is not responding.";

                }
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid Cart Id";
            }

            return Ok(response);
        }

        /// <summary>
        /// Add Payment Card.
        /// </summary>
        /// <param name="addPaymentCardCommand"></param>
        /// <returns></returns>
        [HttpPost("PaymentCard")]
        [Produces("application/json", "application/xml", Type = typeof(PaymentCardDto))]
        public async Task<IActionResult> AddPaymentCard(AddPaymentCardCommand addPaymentCardCommand)
        {
            var result = await _mediator.Send(addPaymentCardCommand);
            if (!result.Success)
            {
                return ReturnFormattedResponse(result);
            }
            //return CreatedAtAction("GetCustomerAddress", new { customerId = response.Data.CustomerId }, response.Data);
            PaymentCardResponseData response = new PaymentCardResponseData();

            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Card added successfully!";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Payment Cards
        /// </summary>
        /// <param name="paymentCardResource"></param>
        /// <returns></returns>

        [HttpGet("GetPaymentCards")]
        public async Task<IActionResult> GetPaymentCards([FromQuery] PaymentCardResource paymentCardResource)
        {
            var getPaymentCardQuery = new GetPaymentCardQuery
            {
                PaymentCardResource = paymentCardResource
            };
            var result = await _mediator.Send(getPaymentCardQuery);

            var paginationMetadata = new
            {
                totalCount = result.TotalCount,
                pageSize = result.PageSize,
                skip = result.Skip,
                totalPages = result.TotalPages
            };
            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            PaymentCardListResponseData response = new PaymentCardListResponseData();
            if (result.Count > 0)
            {
                response.TotalCount = result.TotalCount;
                response.PageSize = result.PageSize;
                response.Skip = result.Skip;
                response.TotalPages = result.TotalPages;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete CPayment Card.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("PaymentCard/{id}")]
        public async Task<IActionResult> DeletePaymentCard(Guid Id)
        {
            var deletePaymentCardCommand = new DeletePaymentCardCommand { Id = Id };
            var result = await _mediator.Send(deletePaymentCardCommand);
            //return ReturnFormattedResponse(result);            
            PaymentCardListResponseData response = new PaymentCardListResponseData();
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Card deleted successfully";
                response.Data = new PaymentCardDto[0];
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "IPlease wait! Server is not responding.nvalid";
            }

            return Ok(response);
        }

        /// <summary>
        /// Update Payment Card.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="updatePaymentCardCommand"></param>
        /// <returns></returns>
        [HttpPut("PaymentCard/{Id}")]
        [Produces("application/json", "application/xml", Type = typeof(PaymentCardDto))]
        public async Task<IActionResult> UpdatePaymentCard(Guid Id, UpdatePaymentCardCommand updatePaymentCardCommand)
        {
            updatePaymentCardCommand.Id = Id;
            var result = await _mediator.Send(updatePaymentCardCommand);
            //return ReturnFormattedResponse(result);           

            PaymentCardResponseData response = new PaymentCardResponseData();

            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Card updated successfully!";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }


        /// <summary>
        /// Gets the customer profile.
        /// </summary>
        /// <param name="customerQuery">The identifier.</param>
        /// <returns></returns>
        [HttpPost("GetCustomerProfile")]
        public async Task<IActionResult> GetCustomerProfile(GetCustomerQuery customerQuery)
        {
            CustomerProfileResponseData response = new CustomerProfileResponseData();
            var query = new GetCustomerQuery { Id = customerQuery.Id };
            var result = await _mediator.Send(query);

            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }


        /// <summary>
        /// Updates the customer Profile.
        /// </summary>       
        /// <param name="updateCustomerCommand">The update customer Profile command.</param>
        /// <returns></returns>
        [HttpPut("UpdateCustomerProfile"), DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateCustomerProfile(UpdateCustomerCommand updateCustomerCommand)
        {
            IUDResponseData response = new IUDResponseData();
            // updateCustomerCommand.Id = id;
            var result = await _mediator.Send(updateCustomerCommand);
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Profile updated successfully";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";

            }
            return Ok(response);
        }

        /// <summary>
        /// Gets all customer's order list.
        /// </summary>
        /// <param name="salesOrderResource">The update customer Profile command.</param>
        /// <returns></returns>
        [HttpPost("GetAllCustomerOrderList")]
        [Produces("application/json", "application/xml", Type = typeof(List<SalesOrderDto>))]
        public async Task<IActionResult> GetAllCustomerOrderList(SalesOrderResource salesOrderResource)
        {
            CustomerOrderListResponseData response = new CustomerOrderListResponseData();
            var getAllSalesOrderQuery = new GetAllSalesOrderCommand
            {
                SalesOrderResource = salesOrderResource
            };
            var result = await _mediator.Send(getAllSalesOrderQuery);

            if (result.Count > 0)
            {
                response.TotalCount = result.TotalCount;
                response.PageSize = result.PageSize;
                response.Skip = result.Skip;
                response.TotalPages = result.TotalPages;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result;
            }

            return Ok(response);
        }


        /// <summary>
        /// Get Sales Order Details.
        /// </summary>
        /// <param name="salesOrderCommand"></param>
        /// <returns></returns>
        [HttpPost("GetCustomerOrderDetails")]
        [Produces("application/json", "application/xml", Type = typeof(List<SalesOrderDto>))]
        public async Task<IActionResult> GetCustomerOrderDetails(GetSalesOrderCommand salesOrderCommand)
        {
            CustomerOrderDetailsResponseData response = new CustomerOrderDetailsResponseData();
            var getSalesOrderQuery = new GetSalesOrderCommand
            {
                Id = salesOrderCommand.Id
            };
            var result = await _mediator.Send(getSalesOrderQuery);

            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid - " + result.Errors.First();
                response.Data = new SalesOrderDto { };
            }

            return Ok(response);
        }

        /// <summary>
        /// Creates the Customer sales order.
        /// </summary>
        /// <param name="addCustomerSalesOrderCommand">The add sales order command.</param>
        /// <returns></returns>
        [HttpPost("CreateCustomerSalesOrder")]
        [Produces("application/json", "application/xml", Type = typeof(SalesOrderDto))]
        public async Task<IActionResult> CreateCustomerSalesOrder(AddSalesOrderCommand addCustomerSalesOrderCommand)
        {
            SalesOrderResponseData response = new SalesOrderResponseData();
            var result = await _mediator.Send(addCustomerSalesOrderCommand);
            if (result.Success)
            {
                var command = new DeleteCartByCustomerCommand
                {
                    CustomerId = addCustomerSalesOrderCommand.CustomerId,
                    ProductMainCategoryId = addCustomerSalesOrderCommand.ProductMainCategoryId
                };
                var result2 = await _mediator.Send(command);

                bool isOrderDateChanged = false;
                string today = DateTime.Now.ToString("dddd");
                if (today == "Sunday")
                {
                    if (DateTime.Now.TimeOfDay.Hours >= 13 && DateTime.Now.TimeOfDay.Minutes > 0)
                    {
                        isOrderDateChanged = true;
                    }
                    else
                    {
                        isOrderDateChanged = false;

                    }
                }
                else
                {
                    if (DateTime.Now.TimeOfDay.Hours >= 17 && DateTime.Now.TimeOfDay.Minutes > 0)
                    {
                        isOrderDateChanged = true;
                    }
                    else
                    {
                        isOrderDateChanged = false;
                    }
                }

                //var addSalesOrderPaymentCommand = new AddSalesOrderPaymentCommand
                //{
                //    SalesOrderId = result.Data.Id,
                //    ReferenceNumber = "RFN-COD",
                //    Amount = result.Data.TotalAmount,
                //    PaymentMethod = PaymentMethod.COD,
                //    Note = addCustomerSalesOrderCommand.PaymentType,
                //    PaymentDate = addCustomerSalesOrderCommand.SOCreatedDate,
                //    AttachmentData = "",
                //    AttachmentUrl = "",
                //};
                //var resultPayment = await _mediator.Send(addSalesOrderPaymentCommand);

                response.status = true;
                response.StatusCode = 1;
                response.message = "Order placed successfully!";
                response.isOrderDateChanged = isOrderDateChanged;
                response.SalesOrderId = result.Data.Id;

                //CreateInvoice(response.SalesOrderId.Value);
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding. - " + result.Errors.First();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Customer Notifications.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCustomerNotifications")]
        public async Task<IActionResult> GetCustomerNotifications([FromQuery] ReminderResource reminderResource)
        {
            //var getUserNotificationCountQuery = new GetUserNotificationCountQuery { };
            //var result = await _mediator.Send(getUserNotificationCountQuery);
            //return Ok(result);
            var getReminderNotificationQuery = new GetReminderNotificationQuery
            {
                ReminderResource = reminderResource
            };
            var result = await _mediator.Send(getReminderNotificationQuery);

            var paginationMetadata = new
            {
                totalCount = result.TotalCount,
                pageSize = result.PageSize,
                skip = result.Skip,
                totalPages = result.TotalPages
            };
            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            ReminderListResponseData response = new ReminderListResponseData();
            if (result.Count > 0)
            {
                response.TotalCount = result.TotalCount;
                response.PageSize = result.PageSize;
                response.Skip = result.Skip;
                response.TotalPages = result.TotalPages;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }

            return Ok(response);

        }


        /// <summary>
        /// Update the Customer Sales order return.
        /// </summary>
        /// <param name="UpdateSalesOrderReturnCommand">The update customer Sales order command.</param>
        /// <returns></returns>
        [HttpPut("CustomerSalesOrderReturn")]
        [Produces("application/json", "application/xml")]
        public async Task<IActionResult> UpdateCustomerSalesOrderReturn(UpdateSalesOrderReturnCommand updateSalesOrderReturnCommand)
        {
            IUDResponseData response = new IUDResponseData();
            var result = await _mediator.Send(updateSalesOrderReturnCommand);
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Sales order updated successfully!";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid - " + result.Errors.First();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Order Summary
        /// </summary>
        /// <param name="cartResource"></param>
        /// <returns></returns>
        [HttpPost("GetOrderSummary")]
        public async Task<IActionResult> GetOrderSummary(CartResource cartResource)
        {
            decimal totalSaleAmount = 0;
            decimal totalMrpAmount = 0;
            CustomerOrderSummaryResponseData response = new CustomerOrderSummaryResponseData();

            try
            {
                var query = new GetAllCartQuery
                {
                    CartResource = cartResource
                };
                var result = await _mediator.Send(query);

                if (result.Count > 0)
                {
                    result.ForEach(item =>
                    {
                        decimal value = (decimal)(item.UnitPrice) * item.Quantity;
                        int roundedValue = (int)Math.Round(value, MidpointRounding.AwayFromZero);
                        item.Total = roundedValue;

                        totalSaleAmount += (decimal)(item.UnitPrice) * item.Quantity;
                        totalMrpAmount += (decimal)(item.MRP) * item.Quantity;

                    });

                    var Data = new OrderSummary
                    {
                        //Price = result.Sum(x => x.Total).ToString("0.00"),
                        Price = Math.Round(result.Sum(x => x.Total), MidpointRounding.AwayFromZero).ToString("0.00"),
                        //Price = Math.Round((decimal)result.Sum(x => x.Quantity*x.UnitPrice), MidpointRounding.AwayFromZero).ToString("0.00"),
                        Discount = result.Sum(x => x.Discount).ToString("0.00"),
                        DeliveryCharges = "0.00",
                        Items = result.Sum(x => x.Quantity),
                        TotalSaveAmount = Math.Round((totalMrpAmount - totalSaleAmount), MidpointRounding.AwayFromZero).ToString("0.00"),
                    };

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = Data;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                    response.Data = new OrderSummary { };
                }

            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [Authorize]
        /// <summary>
        /// Logout.
        /// </summary>
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            LogoutResponseData response = new LogoutResponseData();
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJEQl9TVEFUSVNUSUNTIjoidHJ1ZSIsIkRCX0JFU1RfU0VMTElOR19QUk9TIjoidHJ1ZSIsIkRCX1JFTUlOREVSUyI6InRydWUiLCJEQl9MQVRFU1RfSU5RVUlSSUVTIjoidHJ1ZSIsIkRCX1JFQ0VOVF9TT19TSElQTUVOVCI6InRydWUiLCJEQl9SRUNFTlRfUE9fREVMSVZFUlkiOiJ0cnVlIiwiUFJPX1ZJRVdfUFJPRFVDVFMiOiJ0cnVlIiwiUFJPX0FERF9QUk9EVUNUIjoidHJ1ZSIsIlBST19VUERBVEVfUFJPRFVDVCI6InRydWUiLCJQUk9fREVMRVRFX1BST0RVQ1QiOiJ0cnVlIiwiUFJPX01BTkFHRV9QUk9fQ0FUIjoidHJ1ZSIsIlBST19NQU5BR0VfVEFYIjoidHJ1ZSIsIlBST19NQU5BR0VfVU5JVCI6InRydWUiLCJQUk9fTUFOQUdFX0JSQU5EIjoidHJ1ZSIsIlBST19NQU5BR0VfV0FSRUhPVVNFIjoidHJ1ZSIsIlNVUFBfVklFV19TVVBQTElFUlMiOiJ0cnVlIiwiU1VQUF9BRERfU1VQUExJRVIiOiJ0cnVlIiwiU1VQUF9VUERBVEVfU1VQUExJRVIiOiJ0cnVlIiwiU1VQUF9ERUxFVEVfU1VQUExJRVIiOiJ0cnVlIiwiQ1VTVF9WSUVXX0NVU1RPTUVSUyI6InRydWUiLCJDVVNUX0FERF9DVVNUT01FUiI6InRydWUiLCJDVVNUX1VQREFURV9DVVNUT01FUiI6InRydWUiLCJDVVNUX0RFTEVURV9DVVNUT01FUiI6InRydWUiLCJJTlFfVklFV19JTlFVSVJJRVMiOiJ0cnVlIiwiSU5RX0FERF9JTlFVSVJZIjoidHJ1ZSIsIklOUV9VUERBVEVfSU5RVUlSWSI6InRydWUiLCJJTlFfREVMRVRFX0lOUVVJUlkiOiJ0cnVlIiwiSU5RX01BTkFHRV9JTlFfU1RBVFVTIjoidHJ1ZSIsIklOUV9NQU5BR0VfSU5RX1NPVVJDRSI6InRydWUiLCJJTlFfTUFOQUdFX1JFTUlOREVSUyI6InRydWUiLCJQT1JfVklFV19QT19SRVFVRVNUUyI6InRydWUiLCJQT1JfQUREX1BPX1JFUVVFU1QiOiJ0cnVlIiwiUE9SX1VQREFURV9QT19SRVFVRVNUIjoidHJ1ZSIsIlBPUl9ERUxFVEVfUE9fUkVRVUVTVCI6InRydWUiLCJQT1JfQ09OVkVSVF9UT19QTyI6InRydWUiLCJQT1JfR0VORVJBVEVfSU5WT0lDRSI6InRydWUiLCJQT1JfUE9SX0RFVEFJTCI6InRydWUiLCJQT19WSUVXX1BVUkNIQVNFX09SREVSUyI6InRydWUiLCJQT19BRERfUE8iOiJ0cnVlIiwiUE9fVVBEQVRFX1BPIjoidHJ1ZSIsIlBPX0RFTEVURV9QTyI6InRydWUiLCJQT19WSUVXX1BPX0RFVEFJTCI6InRydWUiLCJQT19SRVRVUk5fUE8iOiJ0cnVlIiwiUE9fVklFV19QT19QQVlNRU5UUyI6InRydWUiLCJQT19BRERfUE9fUEFZTUVOVCI6InRydWUiLCJQT19ERUxFVEVfUE9fUEFZTUVOVCI6InRydWUiLCJQT19HRU5FUkFURV9JTlZPSUNFIjoidHJ1ZSIsIlNPX1ZJRVdfU0FMRVNfT1JERVJTIjoidHJ1ZSIsIlNPX0FERF9TTyI6InRydWUiLCJTT19VUERBVEVfU08iOiJ0cnVlIiwiU09fREVMRVRFX1NPIjoidHJ1ZSIsIlNPX1ZJRVdfU09fREVUQUlMIjoidHJ1ZSIsIlNPX1JFVFVSTl9TTyI6InRydWUiLCJTT19WSUVXX1NPX1BBWU1FTlRTIjoidHJ1ZSIsIlNPX0FERF9TT19QQVlNRU5UIjoidHJ1ZSIsIlNPX0RFTEVURV9TT19QQVlNRU5UIjoidHJ1ZSIsIlNPX0dFTkVSQVRFX0lOVk9JQ0UiOiJ0cnVlIiwiSU5WRV9WSUVXX0lOVkVOVE9SSUVTIjoidHJ1ZSIsIklOVkVfTUFOQUdFX0lOVkVOVE9SWSI6InRydWUiLCJFWFBfVklFV19FWFBFTlNFUyI6InRydWUiLCJFWFBfQUREX0VYUEVOU0UiOiJ0cnVlIiwiRVhQX1VQREFURV9FWFBFTlNFIjoidHJ1ZSIsIkVYUF9ERUxFVEVfRVhQRU5TRSI6InRydWUiLCJFWFBfTUFOQUdFX0VYUF9DQVRFR09SWSI6InRydWUiLCJSRVBfUE9fUkVQIjoidHJ1ZSIsIlJFUF9TT19SRVAiOiJ0cnVlIiwiUkVQX1BPX1BBWU1FTlRfUkVQIjoidHJ1ZSIsIlJFUF9FWFBFTlNFX1JFUCI6InRydWUiLCJSRVBfU09fUEFZTUVOVF9SRVAiOiJ0cnVlIiwiUkVQX1NBTEVTX1ZTX1BVUkNIQVNFX1JFUCI6InRydWUiLCJSRU1fVklFV19SRU1JTkRFUlMiOiJ0cnVlIiwiUkVNX0FERF9SRU1JTkRFUiI6InRydWUiLCJSRU1fVVBEQVRFX1JFTUlOREVSIjoidHJ1ZSIsIlJFTV9ERUxFVEVfUkVNSU5ERVIiOiJ0cnVlIiwiUk9MRVNfVklFV19ST0xFUyI6InRydWUiLCJST0xFU19BRERfUk9MRSI6InRydWUiLCJST0xFU19VUERBVEVfUk9MRSI6InRydWUiLCJST0xFU19ERUxFVEVfUk9MRSI6InRydWUiLCJVU1JfVklFV19VU0VSUyI6InRydWUiLCJVU1JfQUREX1VTRVIiOiJ0cnVlIiwiVVNSX1VQREFURV9VU0VSIjoidHJ1ZSIsIlVTUl9ERUxFVEVfVVNFUiI6InRydWUiLCJVU1JfUkVTRVRfUFdEIjoidHJ1ZSIsIlVTUl9BU1NJR05fVVNSX1JPTEVTIjoidHJ1ZSIsIlVTUl9BU1NJR05fVVNSX1BFUk1JU1NJT05TIjoidHJ1ZSIsIlVTUl9PTkxJTkVfVVNFUlMiOiJ0cnVlIiwiRU1BSUxfTUFOQUdFX0VNQUlMX1NNVFBfU0VUVElOUyI6InRydWUiLCJFTUFJTF9NQU5BR0VfRU1BSUxfVEVNUExBVEVTIjoidHJ1ZSIsIkVNQUlMX1NFTkRfRU1BSUwiOiJ0cnVlIiwiU0VUVF9VUERBVEVfQ09NX1BST0ZJTEUiOiJ0cnVlIiwiU0VUVF9NQU5BR0VfQ09VTlRSWSI6InRydWUiLCJTRVRUX01BTkFHRV9DSVRZIjoidHJ1ZSIsIkxPR1NfVklFV19MT0dJTl9BVURJVFMiOiJ0cnVlIiwiTE9HU19WSUVXX0VSUk9SX0xPR1MiOiJ0cnVlIiwiUkVQX1BST19QUF9SRVAiOiJ0cnVlIiwiUkVQX0NVU1RfUEFZTUVOVF9SRVAiOiJ0cnVlIiwiUkVQX1BST19TT19SRVBPUlQiOiJ0cnVlIiwiUkVQX1NVUF9QQVlNRU5UX1JFUCI6InRydWUiLCJSRVBfU1RPQ0tfUkVQT1JUIjoidHJ1ZSIsIlBPU19QT1MiOiJ0cnVlIiwiUkVQX1ZJRVdfV0FSX1JFUCI6InRydWUiLCJSRVBfVklFV19QUk9fTE9TU19SRVAiOiJ0cnVlIiwic3ViIjoiNGIzNTJiMzctMzMyYS00MGM2LWFiMDUtZTM4ZmNmMTA5NzE5IiwiRW1haWwiOiJhZG1pbkBnbWFpbC5jb20iLCJuYmYiOjE3MDEwODA0MjgsImV4cCI6MTcwMTEyMzYyOCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIiwiYXVkIjoiUFRDVXNlcnMifQ.bZvhsh1KWB8xrO7JZh0Mral3RO0pdoevQamJyZJj9Yg";

            var principal = GetPrincipalFromExpiredToken(token);
            var expClaim = principal.Claims.First(x => x.Type == "Email").Value;
            var identity = principal.Identity as ClaimsIdentity;
            var tok = identity.FindFirst("Token");
            identity.RemoveClaim(identity.FindFirst("Token"));
            //var existingClaim = identity.FindFirst(key);
            response.status = true;
            response.StatusCode = 1;
            response.message = "Success";
            response.Data = "Logout Successfully";
            return Ok(response);
        }


        /// <summary>
        /// Gets the new Sales order number.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNewOrderNumber")]
        public async Task<IActionResult> GetNewSalesOrderNumber()
        {
            OrderNumberResponseData response = new OrderNumberResponseData();
            var getNewSalesOrderNumberQuery = new GetNewSalesOrderNumberCommand { };
            var result = await _mediator.Send(getNewSalesOrderNumberQuery);
            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.OrderNumber = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";

            }
            return Ok(response);
        }


        /// <summary>
        /// Send mail.
        /// </summary>
        /// <param name="sendEmailCommand"></param>
        /// <returns></returns>
        [HttpPost("SendAllEmail")]
        [Produces("application/json", "application/xml", Type = typeof(void))]
        public async Task<IActionResult> SendAllEmail(SendEmailCommand sendEmailCommand)
        {
            SendEmailResponseData response = new SendEmailResponseData();
            var result = await _mediator.Send(sendEmailCommand);
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid - " + result.Errors.First();
            }
            return Ok(response);

        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("This*Is&A!Long)Key(For%Creating@A$SymmetricKey")),
                //ValidateLifetime = false, //here we are saying that we don't care about the token's expiration date               
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        /// <summary>
        /// Add Notice.
        /// </summary>
        /// <param name="addNoticeCommand"></param>
        /// <returns></returns>
        [HttpPost("AddNotice")]
        [Produces("application/json", "application/xml", Type = typeof(NoticeDto))]
        public async Task<IActionResult> AddNotice(AddNoticeCommand addNoticeCommand)
        {
            var response = await _mediator.Send(addNoticeCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return CreatedAtAction("GetNotices", new { id = response.Data.Id }, response.Data);
        }

        /// <summary>
        /// Get Notices.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNotices")]
        [Produces("application/json", "application/xml", Type = typeof(List<NoticeDto>))]
        public async Task<IActionResult> GetNotices()
        {
            NoticeResponseData response = new NoticeResponseData();
            var getAllNoticeCommand = new GetAllNoticeCommand { };
            var result = await _mediator.Send(getAllNoticeCommand);

            if (result.Count > 0)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result.FirstOrDefault();
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result.FirstOrDefault();
            }
            return Ok(response);
        }

        /// <summary>
        /// Add Home Page Banner.
        /// </summary>
        /// <param name="addHomePageBannerCommand"></param>
        /// <returns></returns>
        [HttpPost("AddHomePageBanner")]
        [Produces("application/json", "application/xml", Type = typeof(HomePageBannerDto))]
        public async Task<IActionResult> AddHomePageBanner(AddHomePageBannerCommand addHomePageBannerCommand)
        {
            var response = await _mediator.Send(addHomePageBannerCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return CreatedAtAction("GetHomePageBanners", new { id = response.Data.Id }, response.Data);
        }

        /// <summary>
        /// Get Home Page Banners.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetHomePageBanners")]
        [Produces("application/json", "application/xml", Type = typeof(List<HomePageBannerDto>))]
        public async Task<IActionResult> GetHomePageBanners()
        {
            HomePageBannerListResponseData response = new HomePageBannerListResponseData();
            var getAllHomePageBannerCommand = new GetAllHomePageBannerCommand { };
            var result = await _mediator.Send(getAllHomePageBannerCommand);

            var getAllNoticeCommand = new GetAllNoticeCommand { };
            var noticeResult = await _mediator.Send(getAllNoticeCommand);

            if (result.Count > 0)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.TextData = noticeResult;
                response.AlertMessage = noticeResult.FirstOrDefault().AlertMessage;
                response.StoreOpenClose = noticeResult.FirstOrDefault().StoreOpenClose;
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result;
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete Home Page Banner.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteHomePageBanner/{id}")]
        public async Task<IActionResult> DeleteHomePageBanner(Guid Id)
        {
            var deleteHomePageBannerCommand = new DeleteHomePageBannerCommand { Id = Id };
            var result = await _mediator.Send(deleteHomePageBannerCommand);
            //return ReturnFormattedResponse(result);            
            HomePageBannerListResponseData response = new HomePageBannerListResponseData();
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = new HomePageBannerDto[0];
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }

        /// <summary>
        /// Add Banner.
        /// </summary>
        /// <param name="addOTPBannerCommand"></param>
        /// <returns></returns>
        [HttpPost("AddOTPBanner")]
        [Produces("application/json", "application/xml", Type = typeof(OTPBannerDto))]
        public async Task<IActionResult> AddOTPPageBanner(AddOTPBannerCommand addOTPBannerCommand)
        {
            var response = await _mediator.Send(addOTPBannerCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return CreatedAtAction("GetOTPBanners", new { id = response.Data.Id }, response.Data);
        }


        /// <summary>
        /// Get Banners.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetOTPBanners")]
        [Produces("application/json", "application/xml", Type = typeof(List<OTPBannerDto>))]
        public async Task<IActionResult> GetOTPBanners()
        {
            OTPBannerListResponseData response = new OTPBannerListResponseData();
            var getAllOTPBannerCommand = new GetAllOTPBannerCommand { };
            var result = await _mediator.Send(getAllOTPBannerCommand);

            if (result.Count > 0)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result;
            }
            return Ok(response);
        }

        /// <summary>
        /// Add Banner.
        /// </summary>
        /// <param name="addBannerCommand"></param>
        /// <returns></returns>
        [HttpPost("AddBanner")]
        [Produces("application/json", "application/xml", Type = typeof(BannerDto))]
        public async Task<IActionResult> AddBanner(AddBannerCommand addBannerCommand)
        {
            var response = await _mediator.Send(addBannerCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return CreatedAtAction("GetBanners", new { id = response.Data.Id }, response.Data);
        }

        /// <summary>
        /// Get Banners.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetBanners")]
        [Produces("application/json", "application/xml", Type = typeof(List<BannerDto>))]
        public async Task<IActionResult> GetBanners()
        {
            BannerListResponseData response = new BannerListResponseData();
            var getAllBannerCommand = new GetAllBannerCommand { };
            var result = await _mediator.Send(getAllBannerCommand);

            if (result.Count > 0)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result;
            }
            return Ok(response);
        }

        /// <summary>
        /// Add Category Banner.
        /// </summary>
        /// <param name="addCategoryBannerCommand"></param>
        /// <returns></returns>
        [HttpPost("AddCategoryBanner")]
        [Produces("application/json", "application/xml", Type = typeof(CategoryBannerDto))]
        public async Task<IActionResult> AddCategoryBanner(AddCategoryBannerCommand addCategoryBannerCommand)
        {
            var response = await _mediator.Send(addCategoryBannerCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return CreatedAtAction("GetCategoryBanners", new { id = response.Data.Id }, response.Data);
        }

        /// <summary>
        /// Get Login Page Banners.
        /// </summary>
        /// <returns></returns>     
        [AllowAnonymous]
        [HttpGet("GetCategoryBanners")]
        [Produces("application/json", "application/xml", Type = typeof(List<CategoryBannerDto>))]
        public async Task<IActionResult> GetCategoryBanners([FromQuery] string ImgType)
        {
            CategoryBannerListResponseData response = new CategoryBannerListResponseData();
            var getAllCategoryBannerCommand = new GetAllCategoryBannerCommand
            {
                Type = ImgType
            };
            var result = await _mediator.Send(getAllCategoryBannerCommand);

            if (result.Count > 0)
            {

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result;
            }
            return Ok(response);
        }

        /// <summary>
        /// Add Login Page Banner.
        /// </summary>
        /// <param name="addLoginPageBannerCommand"></param>
        /// <returns></returns>
        [HttpPost("AddLoginPageBanner")]
        [Produces("application/json", "application/xml", Type = typeof(LoginPageBannerDto))]
        public async Task<IActionResult> AddLoginPageBanner(AddLoginPageBannerCommand addLoginPageBannerCommand)
        {
            var response = await _mediator.Send(addLoginPageBannerCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return CreatedAtAction("GetLoginPageBanners", new { id = response.Data.Id }, response.Data);
        }

        /// <summary>
        /// Get Login Page Banners.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetLoginPageBanners")]
        [Produces("application/json", "application/xml", Type = typeof(List<LoginPageBannerDto>))]
        public async Task<IActionResult> GetLoginPageBanners()
        {
            LoginPageBannerListResponseData response = new LoginPageBannerListResponseData();
            var getAllLoginPageBannerCommand = new GetAllLoginPageBannerCommand { };
            var result = await _mediator.Send(getAllLoginPageBannerCommand);

            if (result.Count > 0)
            {
                //response.TotalCount = result.TotalCount;
                //response.PageSize = result.PageSize;
                //response.Skip = result.Skip;
                //response.TotalPages = result.TotalPages;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
                response.Data = result;
            }
            return Ok(response);
        }


        /// <summary>
        /// Add Inventory
        /// </summary>
        /// <param name="addInventoryCommand"></param>
        /// <returns></returns>
        [HttpPost("UploadStockInventory")]
        [Produces("application/json", "application/xml", Type = typeof(InventoryDto))]
        public async Task<IActionResult> UploadStockInventory([FromForm] AddStockExcelUploadCommand addStockExcelUploadCommand)
        {
            AddInventoryCommand addInventoryCommand = new AddInventoryCommand();
            IUDResponseData response = new IUDResponseData();
            bool ResponseStatus = false;
            //IFormFile file = null;

            if (addStockExcelUploadCommand.FileDetails != null)
            {
                //var pathToSave = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.StockExcelUploadFilePath);
                //if (!Directory.Exists(pathToSave))
                //{
                //    Directory.CreateDirectory(pathToSave);
                //}
                //await using (FileStream stream = new FileStream(Path.Combine(pathToSave,
                //    addStockExcelUploadCommand.FileDetails.FileName), FileMode.Create))
                //{
                //    addStockExcelUploadCommand.FileDetails.CopyTo(stream);

                //    stream.Flush();
                //    stream.Dispose();
                //    stream.Close();

                //    //

                //}


                //==============================

                try
                {
                    // Check the File is received

                    //if (file == null)
                    //    throw new Exception("File is Not Received...");


                    // Create the Directory if it is not exist
                    string dirPath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.StockExcelUploadFilePath);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // MAke sure that only Excel file is used 
                    string dataFileName = Path.GetFileName(addStockExcelUploadCommand.FileDetails.FileName);

                    string extension = Path.GetExtension(dataFileName);

                    string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };

                    if (!allowedExtsnions.Contains(extension))
                        throw new Exception("Sorry! This file is not allowed, make sure that file having extension as either.xls or.xlsx is uploaded.");

                    // Make a Copy of the Posted File from the Received HTTP Request
                    string saveToPath = Path.Combine(dirPath, dataFileName);

                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        addStockExcelUploadCommand.FileDetails.CopyTo(stream);
                    }


                    // USe this to handle Encodeing differences in .NET Core
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    // read the excel file
                    using (var stream = new FileStream(saveToPath, FileMode.Open))
                    {
                        if (extension == ".xls")
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        else
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                        //var conf = new ExcelDataSetConfiguration
                        //{
                        //    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        //    {
                        //        UseHeaderRow = true
                        //    }
                        //};

                        DataSet ds = new DataSet();
                        ds = reader.AsDataSet();
                        reader.Close();

                        if (ds != null && ds.Tables.Count > 0)
                        {
                            // Read the the Table
                            DataTable serviceDetails = ds.Tables[0];
                            //DataTable serviceDetails =new DataTable();
                            //serviceDetails.Columns.AddRange(new DataColumn[] { new DataColumn("ProductId"), new DataColumn("Stock"), new DataColumn("PricePerUnit"), new DataColumn("UnitId") });
                            //serviceDetails = ds.Tables[0];
                            //serviceDetails.Columns.Add("ProductId");
                            //serviceDetails.Columns.Add("Stock");
                            //serviceDetails.Columns.Add("PricePerUnit");
                            //serviceDetails.Columns.Add("UnitId");
                            // serviceDetails = ds.Tables[0];

                            for (int i = 1; i < serviceDetails.Rows.Count; i++)
                            {
                                //string aa = serviceDetails.Rows[i]["Column0"].ToString();
                                AddInventoryCommand details = new AddInventoryCommand();
                                //details.ProductId = new Guid(serviceDetails.Rows[i][0].ToString());
                                string ProductName = serviceDetails.Rows[i][0].ToString();
                                details.ProductCode = serviceDetails.Rows[i][1].ToString();
                                //details.Stock = Convert.ToDecimal(serviceDetails.Rows[i][1].ToString());
                                //details.PricePerUnit = Convert.ToDecimal(serviceDetails.Rows[i][2].ToString());
                                //details.UnitId = new Guid(serviceDetails.Rows[i][3].ToString());

                                string UnitName = serviceDetails.Rows[i][2].ToString();
                                details.PurchasePrice = Convert.ToDecimal(serviceDetails.Rows[i][3].ToString());
                                details.Mrp = Convert.ToDecimal(serviceDetails.Rows[i][4].ToString());
                                details.Margin = Convert.ToDecimal(serviceDetails.Rows[i][5].ToString());
                                details.PricePerUnit = Convert.ToDecimal(serviceDetails.Rows[i][6].ToString());
                                details.Stock = Convert.ToDecimal(serviceDetails.Rows[i][7].ToString());

                                // Add the record in Database
                                var result = await _mediator.Send(details);
                                if (result.Success)
                                {
                                    ResponseStatus = true;
                                }

                            }
                        }
                    }

                    if (ResponseStatus != null)
                    {
                        response.status = true;
                        response.StatusCode = 1;
                        response.message = "Success";
                    }
                    else
                    {
                        response.status = false;
                        response.StatusCode = 0;
                        response.message = "Invalid";
                    }

                }
                catch (Exception ex)
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid - " + ex.Message;
                }

            }

            return Ok(response);
            //===============================

        }

        /// <summary>
        /// Delete Login Page Banner.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteLoginPageBanner/{id}")]
        public async Task<IActionResult> DeleteLoginPageBanner(Guid Id)
        {
            var deleteLoginPageBannerCommand = new DeleteLoginPageBannerCommand { Id = Id };
            var result = await _mediator.Send(deleteLoginPageBannerCommand);
            //return ReturnFormattedResponse(result);            
            LoginPageBannerListResponseData response = new LoginPageBannerListResponseData();
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = new LoginPageBannerDto[0];
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete Banner.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteBanner/{id}")]
        public async Task<IActionResult> DeleteBanner(Guid Id)
        {
            var deleteBannerCommand = new DeleteBannerCommand { Id = Id };
            var result = await _mediator.Send(deleteBannerCommand);
            //return ReturnFormattedResponse(result);            
            BannerListResponseData response = new BannerListResponseData();
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = new BannerDto[0];
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete Category Banner.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteCategoryBanner/{id}")]
        public async Task<IActionResult> DeleteCategoryBanner(Guid Id)
        {
            var deleteCategoryBannerCommand = new DeleteCategoryBannerCommand { Id = Id };
            var result = await _mediator.Send(deleteCategoryBannerCommand);
            //return ReturnFormattedResponse(result);            
            CategoryBannerListResponseData response = new CategoryBannerListResponseData();
            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = new CategoryBannerDto[0];
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid";
            }
            return Ok(response);
        }

        /// <summary>
        /// Download Stock Inventory File Format.
        /// </summary>
        [HttpGet("DownloadStockInventoryFile")]
        public IActionResult DownloadStockInventoryFile()
        {
            var filepath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.DownloadFileFormat, "StockInventoryFormat.xlsx");
            return File(System.IO.File.ReadAllBytes(filepath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", System.IO.Path.GetFileName(filepath));
        }

        /// <summary>
        /// Download Product File Format.
        /// </summary>
        [HttpGet("DownloadProductFile")]
        public IActionResult DownloadProductFile()
        {
            var filepath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.DownloadFileFormat, "ProductFormat.xlsx");
            return File(System.IO.File.ReadAllBytes(filepath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", System.IO.Path.GetFileName(filepath));
        }


        /// <summary>
        /// Add Inventory
        /// </summary>
        /// <param name="addStockExcelUploadCommand"></param>
        /// <returns></returns>
        [HttpPost("UploadProductData")]
        [Produces("application/json", "application/xml", Type = typeof(InventoryDto))]
        public async Task<IActionResult> UploadProductData([FromForm] AddStockExcelUploadCommand addStockExcelUploadCommand)
        {
            AddInventoryCommand addInventoryCommand = new AddInventoryCommand();
            IUDResponseData response = new IUDResponseData();
            bool ResponseStatus = false;
            //IFormFile file = null;

            if (addStockExcelUploadCommand.FileDetails != null)
            {

                //==============================
                try
                {
                    // Check the File is received
                    //if (file == null)
                    //    throw new Exception("File is Not Received...");
                    // Create the Directory if it is not exist
                    string dirPath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.StockExcelUploadFilePath);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // MAke sure that only Excel file is used 
                    string dataFileName = Path.GetFileName(addStockExcelUploadCommand.FileDetails.FileName);

                    string extension = Path.GetExtension(dataFileName);

                    string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };

                    if (!allowedExtsnions.Contains(extension))
                        throw new Exception("Sorry! This file is not allowed, make sure that file having extension as either.xls or.xlsx is uploaded.");

                    // Make a Copy of the Posted File from the Received HTTP Request
                    string saveToPath = Path.Combine(dirPath, dataFileName);

                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        addStockExcelUploadCommand.FileDetails.CopyTo(stream);
                    }

                    // USe this to handle Encodeing differences in .NET Core
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    // read the excel file
                    using (var stream = new FileStream(saveToPath, FileMode.Open))
                    {
                        if (extension == ".xls")
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        else
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                        //var conf = new ExcelDataSetConfiguration
                        //{
                        //    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        //    {
                        //        UseHeaderRow = true
                        //    }
                        //};

                        DataSet ds = new DataSet();
                        ds = reader.AsDataSet();
                        reader.Close();

                        if (ds != null && ds.Tables.Count > 0)
                        {
                            // Read the the Table
                            DataTable serviceDetails = ds.Tables[0];
                            //DataTable serviceDetails =new DataTable();
                            //serviceDetails.Columns.AddRange(new DataColumn[] { new DataColumn("ProductId"), new DataColumn("Stock"), new DataColumn("PricePerUnit"), new DataColumn("UnitId") });
                            //serviceDetails = ds.Tables[0];
                            //serviceDetails.Columns.Add("ProductId");
                            //serviceDetails.Columns.Add("Stock");
                            //serviceDetails.Columns.Add("PricePerUnit");
                            //serviceDetails.Columns.Add("UnitId");
                            // serviceDetails = ds.Tables[0];

                            for (int i = 1; i < serviceDetails.Rows.Count; i++)
                            {
                                Boolean VerifyStatus = true;
                                string BrandName = string.Empty, UnitName = string.Empty, WHName = "Pune - Maitri Complex", CategoryName = string.Empty;

                                //string aa = serviceDetails.Rows[i]["Column0"].ToString();
                                AddProductCommand details = new AddProductCommand();
                                //details.ProductId = new Guid(serviceDetails.Rows[i][0].ToString());
                                details.Name = serviceDetails.Rows[i][0].ToString();
                                details.Code = serviceDetails.Rows[i][1].ToString();
                                details.Mrp = Convert.ToDecimal(serviceDetails.Rows[i][2].ToString());
                                details.SalesPrice = Convert.ToDecimal(serviceDetails.Rows[i][3].ToString());

                                // Add the record in Database
                                var result = await _mediator.Send(details);
                                if (result.Success)
                                {
                                    ResponseStatus = true;
                                }

                            }
                        }
                    }

                    if (ResponseStatus != null)
                    {
                        response.status = true;
                        response.StatusCode = 1;
                        response.message = "Success";
                    }
                    else
                    {
                        response.status = false;
                        response.StatusCode = 0;
                        response.message = "Invalid";
                    }

                }
                catch (Exception ex)
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid - " + ex.Message;
                }

            }

            return Ok(response);
            //===============================

        }

        /// <summary>
        /// Get Product Main Categories
        /// </summary>
        /// <param name="getProductMainCategoriesQuery"></param>
        /// <returns></returns>
        //[HttpGet]
        [HttpGet("ProductMainCategoriesList")]
        [Produces("application/json", "application/xml", Type = typeof(List<ProductMainCategoryDto>))]
        public async Task<IActionResult> ProductMainCategoriesList([FromQuery] GetProductMainCategoriesQuery getProductMainCategoriesQuery)
        {
            ProductMainCategoriesResponseData response = new ProductMainCategoriesResponseData();
            try
            {
                var result = await _mediator.Send(getProductMainCategoriesQuery);

                if (result.Count > 0)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);
        }


        /// <summary>
        /// Add Inventory
        /// </summary>
        /// <param name="addStockExcelUploadCommand"></param>
        /// <returns></returns>
        [HttpPost("UploadVendorData")]
        //[Produces("application/json", "application/xml", Type = typeof(InventoryDto))]
        public async Task<IActionResult> UploadVendorData([FromForm] AddStockExcelUploadCommand addStockExcelUploadCommand)
        {

            IUDResponseData response = new IUDResponseData();
            bool ResponseStatus = false;
            //IFormFile file = null;

            if (addStockExcelUploadCommand.FileDetails != null)
            {

                //==============================
                try
                {
                    // Check the File is received
                    //if (file == null)
                    //    throw new Exception("File is Not Received...");
                    // Create the Directory if it is not exist
                    string dirPath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.StockExcelUploadFilePath);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // MAke sure that only Excel file is used 
                    string dataFileName = Path.GetFileName(addStockExcelUploadCommand.FileDetails.FileName);

                    string extension = Path.GetExtension(dataFileName);

                    string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };

                    if (!allowedExtsnions.Contains(extension))
                        throw new Exception("Sorry! This file is not allowed, make sure that file having extension as either.xls or.xlsx is uploaded.");

                    // Make a Copy of the Posted File from the Received HTTP Request
                    string saveToPath = Path.Combine(dirPath, dataFileName);

                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        addStockExcelUploadCommand.FileDetails.CopyTo(stream);
                    }

                    // USe this to handle Encodeing differences in .NET Core
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    // read the excel file
                    using (var stream = new FileStream(saveToPath, FileMode.Open))
                    {
                        if (extension == ".xls")
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        else
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                        //var conf = new ExcelDataSetConfiguration
                        //{
                        //    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        //    {
                        //        UseHeaderRow = true
                        //    }
                        //};

                        DataSet ds = new DataSet();
                        ds = reader.AsDataSet();
                        reader.Close();

                        if (ds != null && ds.Tables.Count > 0)
                        {
                            // Read the the Table
                            DataTable serviceDetails = ds.Tables[0];
                            //DataTable serviceDetails =new DataTable();
                            //serviceDetails.Columns.AddRange(new DataColumn[] { new DataColumn("ProductId"), new DataColumn("Stock"), new DataColumn("PricePerUnit"), new DataColumn("UnitId") });
                            //serviceDetails = ds.Tables[0];
                            //serviceDetails.Columns.Add("ProductId");
                            //serviceDetails.Columns.Add("Stock");
                            //serviceDetails.Columns.Add("PricePerUnit");
                            //serviceDetails.Columns.Add("UnitId");
                            // serviceDetails = ds.Tables[0];

                            for (int i = 1; i < serviceDetails.Rows.Count; i++)
                            {
                                //string aa = serviceDetails.Rows[i]["Column0"].ToString();
                                AddSupplierCommand addSupplierCommand = new AddSupplierCommand();
                                SupplierAddressDto supplierAddress = new SupplierAddressDto();
                                //details.ProductId = new Guid(serviceDetails.Rows[i][0].ToString());
                                addSupplierCommand.SupplierNo = serviceDetails.Rows[i][0].ToString();
                                addSupplierCommand.SupplierType = serviceDetails.Rows[i][1].ToString();
                                addSupplierCommand.SupplierName = serviceDetails.Rows[i][2].ToString();
                                addSupplierCommand.Description = serviceDetails.Rows[i][3].ToString();
                                supplierAddress.Address = serviceDetails.Rows[i][4].ToString();
                                supplierAddress.CityName = serviceDetails.Rows[i][5].ToString();
                                supplierAddress.CountryName = "India";
                                //addSupplierCommand.SupplierAddress.Address = "test"; //serviceDetails.Rows[i][4].ToString();
                                //addSupplierCommand.SupplierAddress.CityName = "Test 2";// serviceDetails.Rows[i][5].ToString();
                                //addSupplierCommand.SupplierAddress.CountryName = "India";
                                addSupplierCommand.PhoneNo = serviceDetails.Rows[i][6].ToString();
                                addSupplierCommand.Email = serviceDetails.Rows[i][7].ToString();
                                addSupplierCommand.SupplierAddress = supplierAddress;

                                // Add the record in Database
                                var result = await _mediator.Send(addSupplierCommand);
                                if (result.Success)
                                {
                                    ResponseStatus = true;
                                }

                            }
                        }
                    }

                    if (ResponseStatus != null)
                    {
                        response.status = true;
                        response.StatusCode = 1;
                        response.message = "Success";
                    }
                    else
                    {
                        response.status = false;
                        response.StatusCode = 0;
                        response.message = "Invalid";
                    }

                }
                catch (Exception ex)
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid - " + ex.Message;
                }

            }

            return Ok(response);
            //===============================

        }

        /// <summary>
        /// Get FAQ List
        /// </summary>        
        /// <returns></returns>
        [HttpGet("FAQList")]
        [Produces("application/json", "application/xml", Type = typeof(List<FAQDto>))]
        public async Task<IActionResult> FAQList()
        {
            FAQResponseData response = new FAQResponseData();
            try
            {
                var getFAQQuery = new GetFAQQuery { };
                var result = await _mediator.Send(getFAQQuery);

                if (result.Count > 0)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);
        }

        /// <summary>
        /// Get Help And Support
        /// </summary>        
        /// <returns></returns>
        [HttpGet("HelpAndSupport")]
        [Produces("application/json", "application/xml", Type = typeof(List<HelpAndSupportDto>))]
        public async Task<IActionResult> HelpAndSupport()
        {
            HelpAndSupportResponseData response = new HelpAndSupportResponseData();
            try
            {
                var getHelpAndSupportQuery = new GetHelpAndSupportQuery { };
                var result = await _mediator.Send(getHelpAndSupportQuery);

                if (result != null)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);
        }


        /// <summary>
        /// Creates the purchase order.
        /// </summary>
        /// <param name="addPurchaseOrderCommand">The add purchase order command.</param>
        /// <returns></returns>
        [HttpPost("CreateGRN"), DisableRequestSizeLimit]
        //[ClaimCheck("PO_ADD_PO,POR_ADD_PO_REQUEST")]
        [Produces("application/json", "application/xml", Type = typeof(GRNDto))]
        public async Task<IActionResult> CreateGRN(AddGRNCommand addGRNCommand)
        {
            var result = await _mediator.Send(addGRNCommand);
            return ReturnFormattedResponse(result);
        }

        /// <summary>
        /// Get App Version
        /// </summary>        
        /// <returns></returns>
        [HttpGet("AppVersion")]
        [Produces("application/json", "application/xml", Type = typeof(List<AppVersionDto>))]
        public async Task<IActionResult> AppVersion()
        {
            AppVersionResponseData response = new AppVersionResponseData();
            try
            {
                var getAppVersionQuery = new GetAppVersionQuery { };
                var result = await _mediator.Send(getAppVersionQuery);

                if (result != null)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);
        }


        /// <summary>
        /// Gets the new Sales order number.
        /// </summary>
        /// <returns></returns>
        //[HttpGet("GetNewBillNumber")]
        //public async Task<IActionResult> GetNewBillNumber()
        //{
        //    OrderNumberResponseData response = new OrderNumberResponseData();
        //    var getNewSalesOrderNumberQuery = new GetNewSalesOrderNumberCommand { };
        //    var result = await _mediator.Send(getNewSalesOrderNumberQuery);
        //    if (result != null)
        //    {
        //        response.status = true;
        //        response.StatusCode = 1;
        //        response.message = "Success";
        //        response.OrderNumber = result;
        //    }
        //    else
        //    {
        //        response.status = false;
        //        response.StatusCode = 0;
        //        response.message = "Invalid";
        //    }
        //    return Ok(response);
        //}


        /// <summary>
        /// Add Inventory
        /// </summary>
        /// <param name="addStockExcelUploadCommand"></param>
        /// <returns></returns>
        [HttpPost("UploadGRNandInventory")]
        [Produces("application/json", "application/xml", Type = typeof(PurchaseOrderDto))]
        public async Task<IActionResult> UploadGRNandInventory([FromForm] AddPurchaseOrderCommand addStockExcelUploadCommand)
        {

            ExlUploadPurchaseOrderResponseData response = new ExlUploadPurchaseOrderResponseData();
            List<PurchaseOrderItemDto> VerifiedPurchaseOrderItems = new List<PurchaseOrderItemDto>();
            List<PurchaseOrderItemDto> UnverifiedPurchaseOrderItems = new List<PurchaseOrderItemDto>();
            bool ResponseStatus = false;
            //IFormFile file = null;

            if (addStockExcelUploadCommand.FileDetails != null)
            {
                //var pathToSave = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.StockExcelUploadFilePath);
                //if (!Directory.Exists(pathToSave))
                //{
                //    Directory.CreateDirectory(pathToSave);
                //}
                //await using (FileStream stream = new FileStream(Path.Combine(pathToSave,
                //    addStockExcelUploadCommand.FileDetails.FileName), FileMode.Create))
                //{
                //    addStockExcelUploadCommand.FileDetails.CopyTo(stream);

                //    stream.Flush();
                //    stream.Dispose();
                //    stream.Close();

                //    //

                //}


                //==============================

                try
                {
                    // Check the File is received

                    //if (file == null)
                    //    throw new Exception("File is Not Received...");


                    // Create the Directory if it is not exist
                    string dirPath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.StockExcelUploadFilePath);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // MAke sure that only Excel file is used 
                    string dataFileName = Path.GetFileName(addStockExcelUploadCommand.FileDetails.FileName);

                    string extension = Path.GetExtension(dataFileName);

                    string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };

                    if (!allowedExtsnions.Contains(extension))
                        throw new Exception("Sorry! This file is not allowed, make sure that file having extension as either.xls or.xlsx is uploaded.");

                    // Make a Copy of the Posted File from the Received HTTP Request
                    string saveToPath = Path.Combine(dirPath, dataFileName);

                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        addStockExcelUploadCommand.FileDetails.CopyTo(stream);
                    }


                    // USe this to handle Encodeing differences in .NET Core
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    // read the excel file
                    using (var stream = new FileStream(saveToPath, FileMode.Open))
                    {
                        if (extension == ".xls")
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        else
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                        //var conf = new ExcelDataSetConfiguration
                        //{
                        //    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        //    {
                        //        UseHeaderRow = true
                        //    }
                        //};

                        DataSet ds = new DataSet();
                        ds = reader.AsDataSet();
                        reader.Close();



                        if (ds != null && ds.Tables.Count > 0)
                        {
                            // Read the the Table
                            DataTable serviceDetails = ds.Tables[0];
                            //DataTable serviceDetails =new DataTable();
                            //serviceDetails.Columns.AddRange(new DataColumn[] { new DataColumn("ProductId"), new DataColumn("Stock"), new DataColumn("PricePerUnit"), new DataColumn("UnitId") });
                            //serviceDetails = ds.Tables[0];
                            //serviceDetails.Columns.Add("ProductId");
                            //serviceDetails.Columns.Add("Stock");
                            //serviceDetails.Columns.Add("PricePerUnit");
                            //serviceDetails.Columns.Add("UnitId");
                            // serviceDetails = ds.Tables[0];

                            for (int i = 1; i < serviceDetails.Rows.Count; i++)
                            {
                                Boolean VerifyStatus = true;
                                string ProductCode = string.Empty, UnitName = string.Empty, WHName = "Pune - Maitri Complex", ProductName = string.Empty;
                                ProductName = serviceDetails.Rows[i][0].ToString();
                                ProductCode = serviceDetails.Rows[i][1].ToString();
                                UnitName = serviceDetails.Rows[i][2].ToString();
                                PurchaseOrderItemDto PurchaseOrderItems = new PurchaseOrderItemDto();

                                if (!string.IsNullOrEmpty(ProductCode))
                                {
                                    var findProduct = _productRepository.FindBy(c => c.Code == ProductCode).FirstOrDefault();
                                    if (findProduct != null)
                                    {
                                        PurchaseOrderItems.ProductId = findProduct.Id;
                                        PurchaseOrderItems.ProductName = findProduct.Name;
                                        PurchaseOrderItems.ProductCode = ProductCode;
                                        PurchaseOrderItems.UnitId = findProduct.UnitId;

                                    }
                                    else
                                    {
                                        PurchaseOrderItems.ProductId = new Guid { };
                                        PurchaseOrderItems.ProductCode = ProductCode;
                                        PurchaseOrderItems.Message = "Invalid Product Code|";
                                        VerifyStatus = false;
                                    }
                                }

                                if (!string.IsNullOrEmpty(ProductName))
                                {

                                    var findProduct = _productRepository.FindBy(c => c.Name == ProductName).FirstOrDefault();
                                    if (findProduct != null)
                                    {
                                        if (ProductCode != findProduct.Code)
                                        {
                                            PurchaseOrderItems.Message += "Mismatched Product Code|";
                                        }
                                    }
                                }

                                //if (!string.IsNullOrEmpty(UnitName))
                                //{
                                //    var findUnit = _unitConversationRepository.FindBy(c => c.Name == UnitName).FirstOrDefault();
                                //    if (findUnit != null)
                                //    {
                                //        PurchaseOrderItems.UnitId = findUnit.Id;
                                //        PurchaseOrderItems.UnitName = findUnit.Name;
                                //    }
                                //    else
                                //    {
                                //        PurchaseOrderItems.UnitId = new Guid { };
                                //        PurchaseOrderItems.UnitName = UnitName;
                                //        PurchaseOrderItems.Message += "Invalid Unit|";
                                //        VerifyStatus = false;
                                //    }
                                //}

                                if (!string.IsNullOrEmpty(WHName))
                                {
                                    var findWH = _warehouseRepository.FindBy(c => c.Name == WHName).FirstOrDefault();
                                    if (findWH != null)
                                    {
                                        PurchaseOrderItems.WarehouseId = findWH.Id;
                                        PurchaseOrderItems.WarehouseName = WHName;
                                    }
                                    else
                                    {
                                        PurchaseOrderItems.WarehouseId = new Guid { };
                                    }

                                }

                                //PurchaseOrderItems.ProductId = new Guid(serviceDetails.Rows[i][0].ToString());
                                //PurchaseOrderItems.WarehouseId = new Guid(serviceDetails.Rows[i][1].ToString());
                                //PurchaseOrderItems.UnitId = new Guid(serviceDetails.Rows[i][3].ToString());
                                PurchaseOrderItems.UnitName = serviceDetails.Rows[i][2].ToString();
                                PurchaseOrderItems.UnitPrice = Convert.ToDecimal(serviceDetails.Rows[i][3].ToString());
                                PurchaseOrderItems.Mrp = Convert.ToDecimal(serviceDetails.Rows[i][4].ToString());
                                PurchaseOrderItems.Margin = Convert.ToDecimal(serviceDetails.Rows[i][5].ToString());
                                PurchaseOrderItems.SalesPrice = Convert.ToDecimal(serviceDetails.Rows[i][6].ToString());
                                PurchaseOrderItems.Quantity = Convert.ToInt32(serviceDetails.Rows[i][7].ToString());


                                if (VerifyStatus == true)
                                {
                                    VerifiedPurchaseOrderItems.Add(PurchaseOrderItems);
                                }
                                else
                                {
                                    UnverifiedPurchaseOrderItems.Add(PurchaseOrderItems);
                                }
                            }

                            //New GRN No -------------

                            var getNewPurchaseOrderNumberQuery = new GetNewPurchaseOrderNumberQuery
                            {
                                isPurchaseOrder = true
                            };
                            var responseGRNNo = await _mediator.Send(getNewPurchaseOrderNumberQuery);
                            if (responseGRNNo != null)
                            {
                                addStockExcelUploadCommand.OrderNumber = responseGRNNo;
                            }

                            //------------------------

                            if (UnverifiedPurchaseOrderItems.Count > 0)
                            {
                                addStockExcelUploadCommand.PurchaseOrderItems = UnverifiedPurchaseOrderItems;
                                ResponseStatus = false;
                            }
                            else
                            {
                                addStockExcelUploadCommand.PurchaseOrderItems = VerifiedPurchaseOrderItems;
                                ResponseStatus = true;
                            }

                            decimal TotalSaleAmount = addStockExcelUploadCommand.PurchaseOrderItems.Sum(x => Convert.ToDecimal(x.SalesPrice * x.Quantity));
                            decimal TotalAmount = addStockExcelUploadCommand.PurchaseOrderItems.Sum(x => Convert.ToDecimal(x.UnitPrice * x.Quantity));
                            addStockExcelUploadCommand.TotalAmount = TotalAmount;
                            addStockExcelUploadCommand.TotalSaleAmount = TotalSaleAmount;

                            if (ResponseStatus == true)
                            {
                                var result = await _mediator.Send(addStockExcelUploadCommand);
                                if (result.Success)
                                {
                                    ResponseStatus = true;
                                }
                                else
                                {
                                    ResponseStatus = false;
                                }
                            }
                            else
                            {
                                ResponseStatus = false;
                            }
                        }
                    }

                    if (ResponseStatus == true)
                    {
                        response.status = true;
                        response.StatusCode = 1;
                        response.message = "Success";
                        response.Data = VerifiedPurchaseOrderItems;
                    }
                    else
                    {
                        response.status = false;
                        response.StatusCode = 0;
                        response.message = "Invalid";
                        response.Data = UnverifiedPurchaseOrderItems;
                    }

                }
                catch (Exception ex)
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid - " + ex.Message;
                }
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid - Please select file to upload";
            }

            return Ok(response);
            //===============================

        }


        /// <summary>
        /// Get Alert Message
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetAlertMessage")]
        public async Task<IActionResult> GetAlertMessage()
        {
            var result = "Successfully Deleted";
            return Ok(result);
        }

        /// <summary>
        /// Add Supplier Documents.
        /// </summary>
        /// <param name="addSupplierDocumentCommand"></param>
        /// <returns></returns>
        [HttpPost("AddSupplierDocument")]
        [Produces("application/json", "application/xml", Type = typeof(SupplierDocumentDto))]
        public async Task<IActionResult> AddSupplierDocument(AddSupplierDocumentCommand addSupplierDocumentCommand)
        {
            var response = await _mediator.Send(addSupplierDocumentCommand);
            if (!response.Success)
            {
                return ReturnFormattedResponse(response);
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Supplier Document by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetSupplierDocument/{id}", Name = "GetSupplierDocument")]
        public async Task<IActionResult> GetSupplierDocument(Guid id)
        {
            var getSupplierDocumentByIdCommand = new GetSupplierDocumentByIdCommand
            {
                Id = id
            };

            var result = await _mediator.Send(getSupplierDocumentByIdCommand);
            return ReturnFormattedResponse(result);
        }


        /// <summary>
        /// Send Message
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("SendMessage")]
        public async Task<IActionResult> SendMessage()
        {
            //var result = "Successfully Send";
            // var response=sendSMS();
            string var = "123456";
            string phone = "918100037343";
            string result = Empty.ToString();
            //String message = HttpUtility.UrlEncode("Hi there, thank you for sending your first test message from Textlocal. See how you can send effective SMS campaigns here: https://tx.gl/r/2nGVj/");
            String message = HttpUtility.UrlEncode("Hi there, thank you for sending your first test message from Textlocal. Get 20% off today with our code: " + var + ".");
            using (var wb = new WebClient())
            {
                byte[] response = wb.UploadValues("https://api.textlocal.in/send/", new NameValueCollection()
                    {
                    {"apikey" , "MzQ0MzQ3NGI2MjU4Nzg3MTY3NjU0ZDU3NzgzNjczNDk="},
                    {"numbers" , phone},
                    {"message" , message},
                    {"sender" , "600010"}
                    });
                result = System.Text.Encoding.UTF8.GetString(response);
                //return result;
            }

            // return result;


            return Ok(result);
        }



        /// <summary>
        /// Clean Inventory
        /// </summary>    
        /// <param></param>
        /// <returns></returns>
        [HttpPost("CleanInventory")]
        public async Task<IActionResult> CleanInventory()
        {
            var cleanInventoryCommand = new CleanInventoryCommand { };
            var result = await _mediator.Send(cleanInventoryCommand);
            return ReturnFormattedResponse(result);
        }


        /// <summary>
        /// Get Sales Item Cat report.
        /// </summary>
        /// <param name="salesOrderResource"></param>
        /// <returns></returns>
        [HttpGet("GetSalesReportProductCategoryWise")]
        public async Task<IActionResult> GetSalesReportProductCategoryWise([FromQuery] SalesOrderResource salesOrderResource)
        {
            if (salesOrderResource.ProductCategoryName == "VEGETABLES")
            {
                salesOrderResource.ProductCategoryId = new Guid("4015D064-3CFF-4459-9E9B-5DA260598447"); //Vegetables 
                salesOrderResource.ProductCategoryName = null;
            }

            if (salesOrderResource.ProductCategoryName == "BAKERY")
            {
                salesOrderResource.ProductMainCategoryId = new Guid("06C71507-B6DE-4D59-DE84-08DBEB3C9568"); //BAKERY 
                salesOrderResource.ProductCategoryName = null;
            }
            var getSalesOrderItemsReportCommand = new GetSalesOrderItemsReportCommand { SalesOrderResource = salesOrderResource };
            var response = await _mediator.Send(getSalesOrderItemsReportCommand);

            //var paginationMetadata = new
            //{
            //    totalCount = response.TotalCount,
            //    pageSize = response.PageSize,
            //    skip = response.Skip,
            //    totalPages = response.TotalPages
            //};

            ProductCategoryWiseSalesReportResponseData Data = new ProductCategoryWiseSalesReportResponseData();

            if (response != null)
            {
                //=============
                decimal VegTotalAmount = Math.Round((decimal)response.Sum(x => x.TotalSalesPrice));
                decimal VegPurAmount = Math.Round((decimal)response.Sum(x => x.PurPrice));
                //decimal VegPurAmount = response.Sum(x => x.PurPrice);


                //========================
                SalesOrderResource salesOrderResource1 = new SalesOrderResource();
                salesOrderResource1 = salesOrderResource;
                salesOrderResource1.ProductCategoryName = null;
                salesOrderResource1.ProductCategoryId = null;
                salesOrderResource1.IsSalesOrderNotReturn = true;
                var getSalesOrderItemsReportCommand1 = new GetSalesOrderItemsReportCommand { SalesOrderResource = salesOrderResource1 };
                var response2 = await _mediator.Send(getSalesOrderItemsReportCommand1);

                decimal TotalAmount = decimal.Round((decimal)response2.Sum(x => x.TotalSalesPrice));
                decimal PurAmount = decimal.Round(response2.Sum(x => x.PurPrice));




                Data.ProductCategoryName = salesOrderResource.ProductCategoryName;
                Data.TotalAmount = decimal.Round((decimal)response.Sum(x => x.TotalSalesPrice)).ToString("0.00");
                Data.PurAmount = decimal.Round(response.Sum(x => x.PurPrice)).ToString("0.00");

                Data.OtherTotalAmount = (TotalAmount - VegTotalAmount).ToString("0.00");
                Data.OtherPurAmount = (PurAmount - VegPurAmount).ToString("0.00");
                Data.status = true;
                Data.StatusCode = 1;
                Data.message = "Success";

            }
            else
            {
                Data.status = false;
                Data.StatusCode = 0;
                Data.message = "Invalid";
            }

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(Data));

            return Ok(Data);
        }

        [AllowAnonymous]
        [HttpPost("PushNotification")]
        public async Task<IActionResult> SendNotificationAsync([FromBody] MessageRequest request)
        {
            string result = string.Empty;
            string[] DeviceKey = request.DeviceToken.Split(',');
            if (DeviceKey.Count() > 0)
            {
                foreach (var itemKey in DeviceKey)
                {
                    if (!String.IsNullOrEmpty(itemKey))
                    {
                        var message = new Message()
                        {
                            Notification = new Notification
                            {
                                Title = request.Title,
                                Body = request.Body,
                            },
                            Data = new Dictionary<string, string>()
                            {
                                ["FirstName"] = "Sainik",
                                ["LastName"] = "Grocery"
                            },
                            Token = itemKey
                        };
                        var messaging = FirebaseMessaging.DefaultInstance;
                        result = await messaging.SendAsync(message);
                        //if (!string.IsNullOrEmpty(result))
                        //{
                        //    return Ok("Message sent successfully!");
                        //}
                        if (string.IsNullOrEmpty(result))
                        {
                            throw new Exception("Error sending the message.");
                        }
                    }
                }

                return Ok("All Messages sent successfully!");
            }
            return Ok("Push Notification failed!");
        }

        /// <summary>
        /// Get Day Wise Summary.
        /// </summary>
        /// <param name="salesOrderResource"></param>
        /// <returns></returns>
        [HttpGet("GetDayWiseSummary")]
        public async Task<IActionResult> GetDayWiseSummary([FromQuery] SalesOrderResource salesOrderResource)
        {
            var getSalesOrderItemsReportCommand = new GetSalesOrderItemsReportCommand { SalesOrderResource = salesOrderResource };
            var response = await _mediator.Send(getSalesOrderItemsReportCommand);

            DayWiseSummaryReportResponseData Data = new DayWiseSummaryReportResponseData();
            List<PaymentsData> paymentsData = new List<PaymentsData>();
            List<CounterSalesData> counterSalesData = new List<CounterSalesData>();

            if (response != null)
            {
                //========================
                SalesOrderResource salesOrderResource1 = new SalesOrderResource();
                salesOrderResource1 = salesOrderResource;
                salesOrderResource1.ProductCategoryName = null;
                var getSalesOrderItemsReportCommand1 = new GetSalesOrderItemsReportCommand { SalesOrderResource = salesOrderResource1 };
                var response2 = await _mediator.Send(getSalesOrderItemsReportCommand1);

                decimal TotalAmount = response2.Sum(x => x.Total);
                decimal PurAmount = response2.Sum(x => x.PurPrice);

                //=============
                decimal VegTotalAmount = response.Sum(x => x.Total);
                decimal VegPurAmount = response.Sum(x => x.PurPrice);



                Data.ProductCategoryName = salesOrderResource.ProductCategoryName;
                Data.TotalAmount = response.Sum(x => x.Total).ToString("0.00");
                Data.PurAmount = response.Sum(x => x.PurPrice).ToString("0.00");

                Data.OtherTotalAmount = (TotalAmount - VegTotalAmount).ToString("0.00");
                Data.OtherPurAmount = (PurAmount - VegPurAmount).ToString("0.00");

                Data.status = true;
                Data.StatusCode = 1;
                Data.message = "Success";

            }
            else
            {
                Data.status = false;
                Data.StatusCode = 0;
                Data.message = "Invalid";
            }


            //=============================================Counter Wise Bill =====================



            var getAllSalesOrderQuery = new GetAllSalesOrderCommand
            {
                SalesOrderResource = salesOrderResource
            };
            var salesOrders = await _mediator.Send(getAllSalesOrderQuery);

            if (salesOrders.Count > 0)
            {

                var Counter = salesOrders.Where(x => x.IsAppOrderRequest == false).GroupBy(x => x.CounterName)
                            .Select(x => new
                            {
                                CounterName = x.Key,
                                TotalAmount = x.Sum(y => y.TotalAmount)
                            }).ToList();

                var App = salesOrders.Where(x => x.IsAppOrderRequest == true).GroupBy(x => x.CounterName)
                           .Select(x => new
                           {
                               CounterName = "App",
                               TotalAmount = x.Sum(y => y.TotalAmount)
                           }).ToList();

                if (App.Count > 0)
                {
                    Counter.Insert(Counter.Count, App.FirstOrDefault());
                }

                foreach (var payments in Counter)
                {
                    var CounterSalesData1 = new CounterSalesData();
                    CounterSalesData1.CounterName = payments.CounterName.ToString();
                    CounterSalesData1.TotalAmount = payments.TotalAmount;
                    counterSalesData.Add(CounterSalesData1);
                }

                Data.CounterSalesData = counterSalesData;
            }
            //========================================================


            var getAllSalesOrderPaymentsReportCommand = new GetAllSalesOrderPaymentsReportCommand
            {
                SalesOrderResource = salesOrderResource
            };
            var salesOrderPayments = await _mediator.Send(getAllSalesOrderPaymentsReportCommand);

            if (salesOrderPayments != null)
            {

                var paymentsData1 = salesOrderPayments.GroupBy(x => x.PaymentMethod)
                       .Select(x => new
                       {
                           PaymentMethod = x.Max(y => y.PaymentMethod),
                           TotalAmount = x.Sum(y => y.Amount)

                       }).ToList();

                foreach (var payments in paymentsData1)
                {
                    var PaymentsData2 = new PaymentsData();
                    PaymentsData2.PaymentMethod = payments.PaymentMethod.ToString();
                    PaymentsData2.TotalAmount = payments.TotalAmount;
                    paymentsData.Add(PaymentsData2);
                }

                Data.PaymentsData = paymentsData;
            }

            //Response.Headers.Add("X-Pagination",
            //Newtonsoft.Json.JsonConvert.SerializeObject(Data));

            return Ok(Data);
        }

        /// <summary>
        /// Get All Products List.
        /// </summary>
        ///// <param name="productResource"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("GetProductCount/{Id}")]
        public async Task<int> GetProductCount(Guid Id)
        {
            int count = 0;
            try
            {
                ProductResource productResource = new ProductResource();
                productResource.ProductMainCategoryId = Id;
                var getAllProductCommand = new GetAllProductCommand
                {
                    ProductResource = productResource
                };
                var result = await _mediator.Send(getAllProductCommand);

                count = result.TotalCount;

                return count;
            }
            catch (Exception ex)
            {
                _ = ex;
            }
            return count;
        }

        /// <summary>
        /// Get Store Open Close.
        /// </summary>
        ///// <param name="productResource"></param>
        ///// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("GetStoreOpenClose")]
        public async Task<IActionResult> GetStoreOpenClose()
        {
            //var getShopHolidayCommand = new GetShopHolidayCommand { };
            //var result = await _mediator.Send(getShopHolidayCommand);
            //return ReturnFormattedResponse(result);

            StoreOpenCloseResponseData response = new StoreOpenCloseResponseData();
            try
            {
                var getShopHolidayCommand = new GetShopHolidayCommand { };
                var result = await _mediator.Send(getShopHolidayCommand);

                if (result.Data != null)
                {
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result.Data;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid";
                    response.Data = new ShopHolidayDto { };
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);

        }

        ///// <summary>
        ///// Get Store Open Close.
        ///// </summary>
        /////// <param name="productResource"></param>
        /////// <param name="Id"></param>
        ///// <returns></returns>
        //[HttpGet("GetStoreOpenClose")]
        //public async Task<bool> GetStoreOpenClose()
        //{
        //    bool storeOpenClose = false;
        //    try
        //    {
        //        var getAppVersionQuery = new GetAppVersionQuery { };
        //        var result = await _mediator.Send(getAppVersionQuery);
        //        storeOpenClose = result.FirstOrDefault().StoreOpenClose;
        //        return storeOpenClose;
        //    }
        //    catch (Exception ex)
        //    {
        //        _ = ex;
        //    }
        //    return storeOpenClose;
        //}

        ///// <summary>
        ///// Get all Product Categories
        ///// </summary>
        ///// <param name="getAllProductCategoriesQuery"></param>
        ///// <returns></returns>
        ////[HttpGet]
        //[HttpPost("ProductBrandList")]
        //[Produces("application/json", "application/xml", Type = typeof(List<ProductCategoryDto>))]
        //public async Task<IActionResult> ProductBrandList([FromBody] GetAllProductCategoriesQuery getAllProductCategoriesQuery)
        //{
        //    ProductCategoriesResponseData response = new ProductCategoriesResponseData();
        //    try
        //    {
        //        var result = await _mediator.Send(getAllProductCategoriesQuery);
        //        result = result.Where(x => x.Name.ToLower() != "BAGGAGE".ToLower() && x.ProductMainCategoryId == getAllProductCategoriesQuery.ProductMainCategoryId).ToList();

        //        var count = await GetProductCount(getAllProductCategoriesQuery.ProductMainCategoryId);

        //        if (result.Count > 0)
        //        {
        //            response.status = true;
        //            response.StatusCode = 1;
        //            response.message = "Success";
        //            response.productCount = count;
        //            response.Data = result;
        //        }
        //        else
        //        {
        //            response.status = false;
        //            response.StatusCode = 0;
        //            response.message = "Please wait! Server is not responding.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.status = false;
        //        response.StatusCode = 0;
        //        response.message = ex.Message;
        //    }
        //    return Ok(response);
        //}

        /// <summary>
        /// Prduct Brand List.
        /// </summary>
        /// <param name="brandResource">The search query.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        [HttpGet("ProductBrandList")]
        public async Task<IActionResult> ProductBrandList([FromQuery] BrandResource brandResource)
        {
            BrandListResponseData response = new BrandListResponseData();
            try
            {
                var searchBrandQuery = new SearchBrandQuery
                {
                    BrandResource = brandResource
                };
                var result = await _mediator.Send(searchBrandQuery);

                if (result.Count > 0)
                {
                    response.TotalCount = result.TotalCount;
                    response.PageSize = result.PageSize;
                    response.Skip = result.Skip;
                    response.TotalPages = result.TotalPages;

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Please wait! Server is not responding.";
                    response.Data = result;

                }

            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Prduct Brand List.
        /// </summary>
        /// <param name="similarProductResource">The search query.</param>
        /// <returns></returns>
        [HttpPost("SimilarProductList")]
        public async Task<IActionResult> SimilarProductList(ProductResource similarProductResource)
        {
            ProductListResponseData response = new ProductListResponseData();
            try
            {
                var getAllProductCommand = new GetAllProductCommand
                {
                    ProductResource = similarProductResource
                };
                var result = await _mediator.Send(getAllProductCommand);

                if (result.Count > 0)
                {
                    response.TotalCount = result.TotalCount;
                    response.PageSize = result.PageSize;
                    response.Skip = result.Skip;
                    response.TotalPages = result.TotalPages;

                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "Success";
                    response.Data = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Please wait! Server is not responding.";
                    response.Data = result;
                }

            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }

            return Ok(response);
        }

        /// <summary>
        /// Download Product File Format.
        /// </summary>
        //[HttpGet("DownloadInvoice")]
        //public IActionResult DownloadInvoice([FromQuery] string OrderNumber)
        //{
        //    var filepath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.InvoiceFile, OrderNumber + ".pdf");
        //    return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", System.IO.Path.GetFileName(filepath));
        //}

        ////[NonAction] async  Task<IActionResult>
        [HttpGet("DownloadInvoice")]
        //[NonAction]
        public async Task<IActionResult> DownloadInvoice([FromQuery] Guid? SaleOrderId)
        {
            FileDownloadResponseData response = new FileDownloadResponseData();
            var getSalesOrderQuery = new GetSalesOrderCommand
            {
                Id = SaleOrderId.Value
            };
            var invoiceDetails = await _mediator.Send(getSalesOrderQuery);

            var pdfByte = GeneratePDF(invoiceDetails.Data);

            var pathToSave = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.InvoiceFile);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }
            System.IO.File.WriteAllBytes(Path.Combine(pathToSave, invoiceDetails.Data.OrderNumber.Replace("#", "_") + ".pdf"), pdfByte);
            //return Ok();

            var filepath = Path.Combine(_pathHelper.InvoiceFile, invoiceDetails.Data.OrderNumber.Replace("#", "_") + ".pdf");

            if (!string.IsNullOrEmpty(filepath))
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = filepath;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "File failed to download.";
            }

            return Ok(response);
            //return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", System.IO.Path.GetFileName(filepath));
        }

        [NonAction]
        public byte[] GeneratePDF(SalesOrderDto invoice)
        {
            //Define your memory stream which will temporarily hold the PDF
            using (MemoryStream stream = new MemoryStream())
            {
                //Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                //Initialize PDF document
                PdfDocument pdf = new PdfDocument(writer);
                // Initialize document
                Document document = new Document(pdf);
                // Add content to the document
                // Header

                var imgpath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.InvoiceFile, "sg-image.png");
                iText.Layout.Element.Image logo = new iText.Layout.Element.Image(ImageDataFactory.Create(imgpath));
                logo.SetHeight(15);
                logo.SetWidth(15);
                logo.SetFixedPosition(530, 790);

                iText.Layout.Element.Table topHeaderTable = new iText.Layout.Element.Table(new float[] { 3, 1, 1, 1, 1, 1 });
                topHeaderTable.SetWidth(UnitValue.CreatePercentValue(100));
                topHeaderTable.AddCell(new Cell().Add(new Paragraph("Sales Order Invoice")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(9).SetBorder(Border.NO_BORDER).SetBold());
                topHeaderTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                topHeaderTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                topHeaderTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7).SetBorder(Border.NO_BORDER));
                topHeaderTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7).SetBorder(Border.NO_BORDER));
                topHeaderTable.AddCell(new Cell().Add(logo).SetTextAlignment(TextAlignment.RIGHT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                document.Add(topHeaderTable);
                //
                float icol = 300f;
                float[] icolWidth = { icol, icol };
                iText.Layout.Element.Table invoiceDetails = new iText.Layout.Element.Table(new float[] { 3, 1 });
                invoiceDetails.SetWidth(UnitValue.CreatePercentValue(100));

                string orderType = invoice.IsAdvanceOrderRequest == true ? "Advance" : "Current";
                Cell cell2 = new Cell(1, 1)
                    .SetFontSize(7)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph(
                    $"Sales Order: {invoice.OrderNumber}\n" +
                    $"Order Date: {invoice.SOCreatedDate.ToShortDateString()}\n" +
                    $"Order Type: {orderType}"));

                invoiceDetails.AddCell(cell2);

                iText.Layout.Element.Table detailsTable = new iText.Layout.Element.Table(new float[] { 3, 1, 1, 1, 1, 1, 1 });
                detailsTable.SetWidth(UnitValue.CreatePercentValue(100));
                detailsTable.AddCell(new Cell().Add(new Paragraph($"To,\n" +
                $"Customer Name: {invoice.Customer.CustomerName}\n" +
                $"Address: {invoice.DeliveryAddress}\n"))
                .SetFontSize(7).SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell().Add(new Paragraph("                   ")).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7).SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell().Add(new Paragraph($"From,\n" +
                    $"Sainik Grocery\n" +
                    $"Address:\n" +
                    $"NEEDS, Southern Command HQ, Pune\n" +
                    $"Phone: +91 8149580080\n" +
                    $"Email: it@sainikgrocery.in\n")).SetFontSize(7).SetBorder(Border.NO_BORDER));

                document.Add(invoiceDetails);
                LineSeparator firstSeparator = new LineSeparator(new SolidLine());
                document.Add(firstSeparator);
                document.Add(detailsTable);
                LineSeparator secondSeparator = new LineSeparator(new SolidLine());
                document.Add(secondSeparator);
                document.Add(new Paragraph());

                iText.Kernel.Colors.Color bgColour = new DeviceRgb(169, 169, 169);
                // Table for invoice items
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(new float[] { 1, 3, 1, 1, 1, 1 });
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.AddHeaderCell("Sl No.").SetTextAlignment(TextAlignment.CENTER).SetFontSize(8);
                table.AddHeaderCell("Description").SetTextAlignment(TextAlignment.CENTER).SetFontSize(8);
                table.AddHeaderCell("Unit Price").SetTextAlignment(TextAlignment.CENTER).SetFontSize(8);
                table.AddHeaderCell("Quantity").SetTextAlignment(TextAlignment.CENTER).SetFontSize(8);
                table.AddHeaderCell("Save").SetTextAlignment(TextAlignment.CENTER).SetFontSize(8);
                table.AddHeaderCell("Total").SetTextAlignment(TextAlignment.CENTER).SetFontSize(8);
                int i = 1;

                foreach (var item in invoice.SalesOrderItems)
                {
                    var saveCalc = (item.Product.Mrp * item.Quantity) - (item.UnitPrice * item.Quantity);
                    int save = (int)Math.Round(saveCalc.Value, MidpointRounding.AwayFromZero);
                    table.AddCell(new Cell().Add(new Paragraph((i++).ToString())).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                    table.AddCell(new Cell().Add(new Paragraph(item.ProductName)).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7));
                    table.AddCell(new Cell().Add(new Paragraph(item.UnitPrice.ToString())).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                    table.AddCell(new Cell().Add(new Paragraph(item.Quantity.ToString() + " " + item.UnitConversation.Name)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                    table.AddCell(new Cell().Add(new Paragraph(save.ToString())).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalSalesPrice.Value.ToString())).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                }

                table.AddCell(new Cell().Add(new Paragraph()).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7));
                table.AddCell(new Cell().Add(new Paragraph()).SetTextAlignment(TextAlignment.LEFT).SetFontSize(7));
                table.AddCell(new Cell().Add(new Paragraph()).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                table.AddCell(new Cell().Add(new Paragraph()).SetTextAlignment(TextAlignment.CENTER).SetFontSize(7));
                table.AddCell(new Cell().Add(new Paragraph("Total Amount")).SetTextAlignment(TextAlignment.CENTER).SetFontSize(8).SetBold());
                table.AddCell(new Cell().Add(new Paragraph(invoice.TotalAmount.ToString())).SetTextAlignment(TextAlignment.CENTER).SetFontSize(8).SetBold());

                //Add the Table to the PDF Document
                document.Add(table);
                // Total Amount
                //document.Add(new Paragraph($"Total Amount: {invoice.TotalAmount.ToString("C")}")
                //    .SetTextAlignment(TextAlignment.RIGHT).SetFontSize(9));
                // Close the Document

                document.Add(new Paragraph());
                document.Add(new Paragraph($"Term & Condition:\n" + invoice.TermAndCondition).SetFontSize(8));
                document.Add(new Paragraph($"__________________\n" + "Authorized Signature").
                SetTextAlignment(TextAlignment.RIGHT).SetFontSize(8));
                document.Add(new Paragraph($"Payment Status: {invoice.PaymentStatus}").SetFontSize(8));
                document.Close();
                return stream.ToArray();
            }

        }

        /// <summary>
        /// Download APK File.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("DownloadAPKFile")]
        public IActionResult DownloadAPKFile()
        {
            var filepath = Path.Combine(_webHostEnvironment.WebRootPath, _pathHelper.DownloadFileFormat, "Android.apk");
            return File(System.IO.File.ReadAllBytes(filepath), "application/vnd.android.package-archive", System.IO.Path.GetFileName(filepath));
        }

        [AllowAnonymous]
        [NonAction]
        public string SendOTPMessage(string mobileNo, int otp)
        {
            string responseString = string.Empty;
            //we creating the necessary URL string:
            string _URL = "http://164.52.195.161/API/SendMsg.aspx?";
            string _senderid = "SGROCY";

            string _uname = HttpUtility.UrlEncode("20240063");
            string _pass = HttpUtility.UrlEncode("180899d9");
            string _recipient = HttpUtility.UrlEncode(mobileNo);
            string _messageText = HttpUtility.UrlEncode("Welcome to Sainik Grocery. Please validate your phone number by entering the OTP " + otp + ". Team Sainik Grocery -Sainik Grocery"); // text message

            // Creating URL to send sms
            string _createURL = _URL +
               "uname=" + _uname +
               "&pass=" + _pass +
               "&send=" + _senderid +
               "&dest=" + _recipient +
               "&msg=" + _messageText +
               "&priority=" + 1 +
               "&schtm=" + DateTime.Now.ToString();

            try
            {
                // creating web request to send sms 
                HttpWebRequest _createRequest = (HttpWebRequest)WebRequest.Create(_createURL);
                // getting response of sms
                HttpWebResponse myResp = (HttpWebResponse)_createRequest.GetResponse();
                System.IO.StreamReader _responseStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                responseString = _responseStreamReader.ReadToEnd();
                _responseStreamReader.Close();
                myResp.Close();
            }
            catch
            {
                //
            }
            return responseString;
        }

        //========================== MSTB ===============================================

        /// <summary>
        /// Get all Years
        /// </summary>
        /// <param name="getAllYearQuery"></param>
        /// <returns></returns>       
        [HttpGet("GetYears")]
        [Produces("application/json", "application/xml", Type = typeof(List<YearDto>))]
        public async Task<IActionResult> GetYears()
        {
            YearListResponseData response = new YearListResponseData();
            try
            {
                var getAllyearQuery = new GetAllYearQuery { };
                var result = await _mediator.Send(getAllyearQuery);

                if (result != null)
                {
                    response.TotalCount = result.Count;
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "success";
                    response.YearData = result;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "please wait! server is not responding.";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Get All Products List.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetMonthYearList")]
        public async Task<IActionResult> GetMonthYearList([FromQuery] string DefaultQueryYear)
        {
            YearListResponseData response = new YearListResponseData();
            try
            {
                string defaultYear = string.Empty;

                var getAllyearQuery = new GetAllYearQuery { };
                var yearResult = await _mediator.Send(getAllyearQuery);

                if (!string.IsNullOrEmpty(DefaultQueryYear))
                {
                    defaultYear = DefaultQueryYear;

                }
                else
                {
                    defaultYear = yearResult.Where(x => x.DefaultYear == true).SingleOrDefault().Name;
                }

                //var getEnum = Enum.GetNames(typeof(Months));
                //var result = string.Join(" " + defaultYear + ",", getEnum).Split(',');
                //result[11] = result[11] + " " + defaultYear;
                //var resultData = result.ToList();

                List<YearMonthDto> yearMonth = new List<YearMonthDto>();

                /*int i = 1;
                foreach (var item in resultData)
                {
                    yearMonth.Add(new YearMonthDto()
                    {
                        YearMonth = item,
                        Month = i++,
                        Year = defaultYear
                    });
                }*/

                MstbSettingResource mstbSettingResource = new MstbSettingResource()
                {
                    Year = Convert.ToInt32(DefaultQueryYear)
                };
                var getMstbSettingQuery = new GetMstbSettingQuery
                {
                    MstbSettingResource = mstbSettingResource
                };
                var resultMstb = await _mediator.Send(getMstbSettingQuery);
                resultMstb.ForEach(item =>
                {
                    yearMonth.Add(new YearMonthDto()
                    {
                        YearMonth = item.MonthName + " " + item.Year,
                        Month = item.Month,
                        Year = Convert.ToString(item.Year),
                        IsDefault = item.IsDefault
                    });

                });

                if (yearMonth != null)
                {
                    response.TotalCount = yearMonth.Count;
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "success";
                    response.Data = yearMonth;
                    response.YearData = yearResult;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "failed.";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Gets all purchase order suppliers.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPurchaseOrderSuppliers")]
        [Produces("application/json", "application/xml", Type = typeof(List<PurchaseOrderDto>))]
        public async Task<IActionResult> GetPurchaseOrderSuppliers([FromQuery] SupplierResource supplierResource)
        {
            SupplierResponseData response = new SupplierResponseData();
            try
            {
                //Purchase Order
                PurchaseOrderResource purchaseOrderResource = new PurchaseOrderResource()
                {
                    PageSize = 0,
                    Skip = 0,
                    Year = supplierResource.Year,
                    Month = supplierResource.Month,
                    IsMSTBGRN = true
                };

                var getAllPurchaseOrderQuery = new GetAllPurchaseOrderQuery
                {
                    PurchaseOrderResource = purchaseOrderResource
                };
                var purchaseOrders = await _mediator.Send(getAllPurchaseOrderQuery);

                //MSTB Purchase Order
                MSTBPurchaseOrderResource MSTBpurchaseOrderResource = new MSTBPurchaseOrderResource()
                {
                    PageSize = 0,
                    Skip = 0,
                    Year = supplierResource.Year,
                    Month = supplierResource.Month,
                    IsMSTBGRN = true
                };

                var getAllMSTBPurchaseOrderQuery = new GetMSTBPurchaseOrderItemsReportCommand
                {
                    MSTBPurchaseOrderResource = MSTBpurchaseOrderResource
                };
                var MstbPurchaseOrders = await _mediator.Send(getAllMSTBPurchaseOrderQuery);


                supplierResource.PageSize = 0;
                supplierResource.Skip = 0;
                var getAllSupplierQuery = new GetAllSupplierQuery
                {
                    SupplierResource = supplierResource
                };
                var result = await _mediator.Send(getAllSupplierQuery);

                List<SupplierDto> resultSuppliers = new List<SupplierDto>();
                if (supplierResource.UserId.HasValue)
                {
                    UserSupplierResource userSupplierResource = new UserSupplierResource()
                    {
                        UserId = supplierResource.UserId,
                    };
                    var searchUserSupplierQuery = new SearchUserSupplierQuery
                    {
                        UserSupplierResource = userSupplierResource
                    };
                    var resultUserSupplier = await _mediator.Send(searchUserSupplierQuery);

                    var supplierIds = resultUserSupplier.Select(s => s.SupplierId.Value).ToArray();

                    resultSuppliers = result.Where(s => supplierIds.Contains(s.Id)).ToList();
                }

                //supplierResource.MobileNo = "D35491B5-EB46-4CC0-B3E9-08DBEBE34B55,6F7D9128-605E-4A0A-B3EA-08DBEBE34B55";
                //var supplierIds = supplierResource.MobileNo.Split(",").Select(s => Guid.Parse(s)).ToArray();
                //var resultSuppliers = result.Where(s => supplierIds.Contains(s.Id));

                ProductResource productResource = new ProductResource()
                {
                    PageSize = 0,
                    Skip = 0
                };

                var getAllProductCommand = new GetAllProductCommand
                {
                    ProductResource = productResource
                };
                var products = await _mediator.Send(getAllProductCommand);

                //var suppliers = purchaseOrders.Where(x => x.POCreatedDate.Year == purchaseOrderResource.Year
                //&& x.POCreatedDate.Month == purchaseOrderResource.Month);

                List<SupplierDto> supplier = new List<SupplierDto>();

                if (supplierResource.UserId.HasValue)
                {
                    resultSuppliers.ForEach(item =>
                    {
                        var mstbCheck = MstbPurchaseOrders.Where(x => x.SupplierId == item.Id).ToList();
                        var isComplted = MstbPurchaseOrders.Where(x => x.SupplierId == item.Id && x.IsCheck == false).ToList();
                        var grnCheck = purchaseOrders.Where(x => x.SupplierId == item.Id).ToList();
                        bool mstb = false;
                        bool grn = false;
                        bool completed = false;
                        if (mstbCheck.Count > 0)
                        {
                            mstb = true;
                            if (isComplted.Count > 0)
                            {
                                completed = false;
                            }
                            else
                            {
                                completed = true;
                            }
                        }
                        if (grnCheck.Count > 0)
                        {
                            grn = true;
                        }

                        //var stockCount = products.Where(x => x.Stock > 0).Count(x => x.SupplierId == item.Id);

                        var stockCount = products.Where(x => x.SupplierId == item.Id && x.Stock > 0).Count();

                        if (stockCount > 0)
                        {
                            supplier.Add(new SupplierDto
                            {
                                Id = item.Id,
                                SupplierName = item.SupplierName,
                                //ProductCount = products.Where(x => x.Stock > 0).Count(x => x.SupplierId == item.Id),
                                ProductCount = stockCount,
                                IsMstbGRN = mstb,
                                IsGRN = grn,
                                IsCompleted = completed
                            });
                        }
                    });
                }
                else
                {
                    result.ForEach(item =>
                    {
                        var mstbCheck = MstbPurchaseOrders.Where(x => x.SupplierId == item.Id).ToList();
                        var isComplted = MstbPurchaseOrders.Where(x => x.SupplierId == item.Id && x.IsCheck == false).ToList();
                        var grnCheck = purchaseOrders.Where(x => x.SupplierId == item.Id).ToList();
                        bool mstb = false;
                        bool grn = false;
                        bool completed = false;
                        if (mstbCheck.Count > 0)
                        {
                            mstb = true;
                            if (isComplted.Count > 0)
                            {
                                completed = false;
                            }
                            else
                            {
                                completed = true;
                            }
                        }
                        if (grnCheck.Count > 0)
                        {
                            grn = true;
                        }

                        //var stockCount = products.Where(x => x.Stock > 0).Count(x => x.SupplierId == item.Id);

                        var stockCount = products.Where(x => x.SupplierId == item.Id && x.Stock > 0).Count();

                        if (stockCount > 0)
                        {
                            supplier.Add(new SupplierDto
                            {
                                Id = item.Id,
                                SupplierName = item.SupplierName,
                                //ProductCount = products.Where(x => x.Stock > 0).Count(x => x.SupplierId == item.Id),
                                ProductCount = stockCount,
                                IsMstbGRN = mstb,
                                IsGRN = grn,
                                IsCompleted = completed
                            });
                        }
                    });
                }

                if (supplier != null)
                {
                    response.TotalCount = supplier.Count;
                    response.status = true;
                    response.StatusCode = 1;
                    response.message = "success";
                    response.Data = supplier;
                }
                else
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "failed";
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Add MSTB Settings.
        /// </summary>
        /// <param name="addMstbSettingCommand"></param>
        /// <returns></returns>
        [HttpPost("AddMSTBSettings")]
        [Produces("application/json", "application/xml", Type = typeof(MstbSettingDto))]
        public async Task<IActionResult> AddMSTBSettings(AddMstbSettingCommand addMstbSettingCommand)
        {
            var result = await _mediator.Send(addMstbSettingCommand);
            if (!result.Success)
            {
                return ReturnFormattedResponse(result);
            }
            //return CreatedAtAction("GetCustomerAddress", new { customerId = response.Data.CustomerId }, response.Data);
            //CustomerAddressResponseData response = new CustomerAddressResponseData();

            //if (result != null)
            //{
            //    response.status = true;
            //    response.StatusCode = 1;
            //    response.message = "Your address added successfully!";
            //    response.Data = result.Data;
            //}
            //else
            //{
            //    response.status = false;
            //    response.StatusCode = 0;
            //    response.message = "Please wait! Server is not responding.";
            //}
            return Ok(result);
        }

        /// <summary>
        /// Get Alert Message
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetPopupAlert")]
        public async Task<IActionResult> GetPopupAlert()
        {
            var result = "https://sainik.shyamfuture.in/page/popup-alert.html";
            //var result = "https://maitricomplex.in/page/popup-alert.html";
            return Ok(result);
        }

        /// <summary>
        /// Get MSTB Settings
        /// </summary>
        /// <param name="mstbSettingResource"></param>
        /// <returns></returns>

        [HttpGet("GetMSTBSettings")]
        public async Task<IActionResult> GetMSTBSettings([FromQuery] MstbSettingResource mstbSettingResource)
        {
            var getMstbSettingQuery = new GetMstbSettingQuery
            {
                MstbSettingResource = mstbSettingResource
            };
            var result = await _mediator.Send(getMstbSettingQuery);

            var paginationMetadata = new
            {
                totalCount = result.TotalCount,
                pageSize = result.PageSize,
                skip = result.Skip,
                totalPages = result.TotalPages
            };
            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            MstbSettingListResponseData response = new MstbSettingListResponseData();
            if (result.Count > 0)
            {
                response.TotalCount = result.TotalCount;
                response.PageSize = result.PageSize;
                response.Skip = result.Skip;
                response.TotalPages = result.TotalPages;

                response.status = true;
                response.StatusCode = 1;
                response.message = "Success";
                response.Data = result;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Please wait! Server is not responding.";
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete Customer Address.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        //[HttpDelete("CustomerAddress/{id}")]
        //public async Task<IActionResult> DeleteCustomerAddress(Guid Id)
        //{
        //    var deleteCustomerAddressCommand = new DeleteCustomerAddressCommand { Id = Id };
        //    var result = await _mediator.Send(deleteCustomerAddressCommand);
        //    //return ReturnFormattedResponse(result);            
        //    CustomerAddressListResponseData response = new CustomerAddressListResponseData();
        //    if (result.Success)
        //    {
        //        response.status = true;
        //        response.StatusCode = 1;
        //        response.message = "Your address deleted successfully!";
        //        response.Data = new CustomerAddressDto[0];
        //    }
        //    else
        //    {
        //        response.status = false;
        //        response.StatusCode = 0;
        //        response.message = "Please wait! Server is not responding.";
        //    }

        //    return Ok(response);
        //}


        /// <summary>
        /// Update MSTB Settings.
        /// </summary>
        /// <param name="updateMstbSettingCommand"></param>
        /// <returns></returns>
        [HttpPut("UpdateMstbSettings")]
        [Produces("application/json", "application/xml", Type = typeof(MstbSettingDto))]
        public async Task<IActionResult> UpdateMstbSettings(UpdateMstbSettingCommand updateMstbSettingCommand)
        {
            var result = await _mediator.Send(updateMstbSettingCommand);
            //return ReturnFormattedResponse(result);           

            MstbSettingResponseData response = new MstbSettingResponseData();

            if (result != null)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Default set successfully!";
                response.Data = result.Data;
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "failed.";
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete MSTB Settings.
        /// </summary>
        /// <param name="deleteMstbSettingCommand"></param>
        /// <returns></returns>
        [HttpDelete("DeleteMstbSettings")]
        [Produces("application/json", "application/xml", Type = typeof(MstbSettingDto))]
        public async Task<IActionResult> DeleteMstbSettings(DeleteMstbSettingCommand deleteMstbSettingCommand)
        {
            var result = await _mediator.Send(deleteMstbSettingCommand);
            //return ReturnFormattedResponse(result);           

            MstbSettingResponseData response = new MstbSettingResponseData();

            if (result.Success)
            {
                response.status = true;
                response.StatusCode = 1;
                response.message = "Deleted successfully!";
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Deletion failed.";
            }
            return Ok(response);
        }

        /// <summary>
        /// Add Auto Generate GRN
        /// </summary>
        /// <param name="autoGenerateGRNResource"></param>
        /// <returns></returns>
        [HttpPost("AutoGenerateGRN")]
        [Produces("application/json", "application/xml", Type = typeof(PurchaseOrderDto))]
        public async Task<IActionResult> AutoGenerateGRNSupplierWise(AutoGenerateGRNResource autoGenerateGRNResource)
        {

            ExlUploadPurchaseOrderResponseData response = new ExlUploadPurchaseOrderResponseData();


            List<PurchaseOrderItemDto> UnverifiedPurchaseOrderItems = new List<PurchaseOrderItemDto>();
            bool ResponseStatus = true;
            //IFormFile file = null;

            if (autoGenerateGRNResource.AutoGenerateGRNSupplierItems != null)
            {
                try
                {
                    foreach (var SupplierItems in autoGenerateGRNResource.AutoGenerateGRNSupplierItems)
                    {
                        AddPurchaseOrderCommand addPurchaseOrderCommand = new AddPurchaseOrderCommand();
                        UpdatePurchaseOrderCommand updatePurchaseOrderCommand = new UpdatePurchaseOrderCommand();
                        List<PurchaseOrderItemDto> VerifiedPurchaseOrderItems = new List<PurchaseOrderItemDto>();
                        List<PurchaseOrderItemDto> VerifiedPurchaseOrderItems1 = new List<PurchaseOrderItemDto>();

                        // Update exist GRN- check GRN exist or not  var date = dateAndTime.Date;
                        var existingPODetails = _purchaseOrderRepository.FindBy(c => c.SupplierId == SupplierItems.SupplierId && c.POCreatedDate.Date == DateTime.Now.Date).FirstOrDefault();
                        if (existingPODetails != null)
                        {

                            //updatePurchaseOrderCommand = existingPONumber;
                            var purchaseOrderItemsExist = _purchaseOrderItemRepository.FindBy(c => c.PurchaseOrderId == existingPODetails.Id).ToList();
                            existingPODetails.PurchaseOrderItems = purchaseOrderItemsExist;
                            updatePurchaseOrderCommand.SupplierId = existingPODetails.SupplierId;
                            updatePurchaseOrderCommand.Month = existingPODetails.Month;
                            updatePurchaseOrderCommand.Year = existingPODetails.Year;
                            updatePurchaseOrderCommand.BatchNo = existingPODetails.BatchNo;
                            updatePurchaseOrderCommand.POCreatedDate = existingPODetails.POCreatedDate;
                            updatePurchaseOrderCommand.DeliveryDate = existingPODetails.DeliveryDate;
                            updatePurchaseOrderCommand.PurchasePaymentType = existingPODetails.PurchasePaymentType;
                            updatePurchaseOrderCommand.DeliveryStatus = existingPODetails.DeliveryStatus;
                            updatePurchaseOrderCommand.Id = existingPODetails.Id;
                            updatePurchaseOrderCommand.OrderNumber = existingPODetails.OrderNumber;
                            updatePurchaseOrderCommand.InvoiceNo = existingPODetails.InvoiceNo;
                            updatePurchaseOrderCommand.TotalAmount = existingPODetails.TotalAmount;
                            updatePurchaseOrderCommand.TotalSaleAmount = existingPODetails.TotalSaleAmount;
                            updatePurchaseOrderCommand.TotalDiscount = existingPODetails.TotalDiscount;
                            updatePurchaseOrderCommand.TermAndCondition = existingPODetails.TermAndCondition;
                            updatePurchaseOrderCommand.Status = existingPODetails.Status;
                            updatePurchaseOrderCommand.IsPurchaseOrderRequest = existingPODetails.IsPurchaseOrderRequest;
                            updatePurchaseOrderCommand.Note = existingPODetails.Note;

                            var purchaseOrderItemsData = existingPODetails.PurchaseOrderItems
                            .Select(cs => new PurchaseOrderItemDto
                            {
                                ProductId = cs.ProductId,
                                UnitPrice = cs.UnitPrice,
                                PurchaseOrderId = cs.PurchaseOrderId,
                                Margin = cs.Margin,
                                UnitId = cs.UnitId,
                                WarehouseId = cs.WarehouseId,
                                TaxValue = cs.TaxValue,
                                Discount = cs.Discount,
                                Id = cs.Id,
                                DiscountPercentage = cs.DiscountPercentage,
                                ExpDate = cs.ExpDate,
                                Mrp = cs.Mrp,
                                SalesPrice = cs.SalesPrice,
                                Quantity = cs.Quantity,
                                Status = cs.Status

                            }).ToList();


                            if (purchaseOrderItemsData != null)
                            {
                                // updatePurchaseOrderCommand.PurchaseOrderItems = purchaseOrderItemsData;
                                VerifiedPurchaseOrderItems1 = purchaseOrderItemsData;

                            }


                            foreach (var ProductItems in SupplierItems.AutoGenerateGRNProductItems)
                            {
                                Boolean VerifyStatus = true;
                                string ProductCode = string.Empty, UnitName = string.Empty, WHName = "Pune - Maitri Complex", ProductName = string.Empty;

                                PurchaseOrderItemDto PurchaseOrderItems = new PurchaseOrderItemDto();
                                if (!string.IsNullOrEmpty(WHName))
                                {
                                    var findWH = _warehouseRepository.FindBy(c => c.Name == WHName).FirstOrDefault();
                                    if (findWH != null)
                                    {
                                        PurchaseOrderItems.WarehouseId = findWH.Id;
                                        PurchaseOrderItems.WarehouseName = WHName;
                                    }
                                    else
                                    {
                                        PurchaseOrderItems.WarehouseId = new Guid { };
                                    }

                                }

                                PurchaseOrderItems.ProductId = new Guid(ProductItems.ProductId.ToString());
                                PurchaseOrderItems.ProductName = ProductItems.ProductName;
                                PurchaseOrderItems.UnitId = new Guid(ProductItems.UnitId.ToString());
                                PurchaseOrderItems.UnitPrice = Convert.ToDecimal(ProductItems.UnitPrice);
                                PurchaseOrderItems.Mrp = Convert.ToDecimal(ProductItems.Mrp);
                                PurchaseOrderItems.Margin = Convert.ToDecimal(ProductItems.Margin);
                                PurchaseOrderItems.SalesPrice = Convert.ToDecimal(ProductItems.SalesPrice);
                                PurchaseOrderItems.Quantity = Convert.ToDecimal(ProductItems.Quantity);
                                //updatePurchaseOrderCommand.PurchaseOrderItems.Add(PurchaseOrderItems);
                                VerifiedPurchaseOrderItems1.Add(PurchaseOrderItems);



                            }

                            //var ss=  updatePurchaseOrderCommand.PurchaseOrderItems.GroupBy(x => x.ProductId).ToList();
                            var ss1 = VerifiedPurchaseOrderItems1.GroupBy(x => x.ProductId).Select(grp => grp.ToList()).ToList();
                           var s2=  VerifiedPurchaseOrderItems1.GroupBy(x => x.ProductId)
                            .Select(x => new PurchaseOrderItemDto
                            { 
                                ProductId =x.Max(y => y.ProductId),
                                UnitId=x.Max(y => y.UnitId),
                                UnitPrice=x.Max(y => y.UnitPrice),
                                Mrp=x.Max(y => y.Mrp),
                                Margin = x.Max(y=>y.Margin),
                                SalesPrice=x.Max(y => y.SalesPrice),
                                Quantity = x.Sum(y => y.Quantity),
                                WarehouseId=x.Max(y=>y.WarehouseId),
                                Total=x.Sum(y => y.Quantity* y.UnitPrice),
                                TotalSalesPrice= (decimal)x.Sum(y => y.Quantity * y.SalesPrice)

                            }).ToList();

                            updatePurchaseOrderCommand.PurchaseOrderItems = s2;
                           // var data= updatePurchaseOrderCommand.PurchaseOrderItems.Sum((x => x.UnitPrice * x.Quantity));
                            updatePurchaseOrderCommand.TotalAmount= updatePurchaseOrderCommand.PurchaseOrderItems.Sum(x=>x.Total);
                            updatePurchaseOrderCommand.TotalSaleAmount= updatePurchaseOrderCommand.PurchaseOrderItems.Sum(x=>x.TotalSalesPrice);


                            //List<List<PurchaseOrderItemDto>> list1 = ss.ToList();
                            //updatePurchaseOrderCommand.PurchaseOrderItems= list1;


                            var result = await _mediator.Send(updatePurchaseOrderCommand);
                            if (result.Success)
                            {
                                ResponseStatus = true;
                            }
                            else
                            {
                                ResponseStatus = false;
                            }

                        }//End Update exist GRN-
                        else
                        {
                            //Create new GRN -
                            addPurchaseOrderCommand.SupplierId = SupplierItems.SupplierId;
                            addPurchaseOrderCommand.Month = SupplierItems.Month;
                            addPurchaseOrderCommand.Year = SupplierItems.Year;
                            addPurchaseOrderCommand.BatchNo = "1";
                            addPurchaseOrderCommand.POCreatedDate = DateTime.Now;
                            addPurchaseOrderCommand.DeliveryDate = DateTime.Now;
                            addPurchaseOrderCommand.PurchasePaymentType = "Credit";
                            addPurchaseOrderCommand.DeliveryStatus = DeliveryStatus.Completely_Delivery;

                            foreach (var ProductItems in SupplierItems.AutoGenerateGRNProductItems)
                            {
                                Boolean VerifyStatus = true;
                                string ProductCode = string.Empty, UnitName = string.Empty, WHName = "Pune - Maitri Complex", ProductName = string.Empty;

                                PurchaseOrderItemDto PurchaseOrderItems = new PurchaseOrderItemDto();
                                if (!string.IsNullOrEmpty(WHName))
                                {
                                    var findWH = _warehouseRepository.FindBy(c => c.Name == WHName).FirstOrDefault();
                                    if (findWH != null)
                                    {
                                        PurchaseOrderItems.WarehouseId = findWH.Id;
                                        PurchaseOrderItems.WarehouseName = WHName;
                                    }
                                    else
                                    {
                                        PurchaseOrderItems.WarehouseId = new Guid { };
                                    }

                                }

                                PurchaseOrderItems.ProductId = new Guid(ProductItems.ProductId.ToString());
                                PurchaseOrderItems.ProductName = ProductItems.ProductName;
                                PurchaseOrderItems.UnitId = new Guid(ProductItems.UnitId.ToString());
                                // PurchaseOrderItems.UnitName = serviceDetails.Rows[i][2].ToString();
                                PurchaseOrderItems.UnitPrice = Convert.ToDecimal(ProductItems.UnitPrice);
                                PurchaseOrderItems.Mrp = Convert.ToDecimal(ProductItems.Mrp);
                                PurchaseOrderItems.Margin = Convert.ToDecimal(ProductItems.Margin);
                                PurchaseOrderItems.SalesPrice = Convert.ToDecimal(ProductItems.SalesPrice);
                                PurchaseOrderItems.Quantity = Convert.ToDecimal(ProductItems.Quantity);

                                VerifiedPurchaseOrderItems.Add(PurchaseOrderItems);


                                //if (VerifyStatus == true)
                                //{
                                //    VerifiedPurchaseOrderItems.Add(PurchaseOrderItems);
                                //}
                                //else
                                //{
                                //    UnverifiedPurchaseOrderItems.Add(PurchaseOrderItems);
                                //}
                            }

                            //New GRN No -------------

                            var getNewPurchaseOrderNumberQuery = new GetNewPurchaseOrderNumberQuery
                            {
                                isPurchaseOrder = true
                            };
                            var responseGRNNo = await _mediator.Send(getNewPurchaseOrderNumberQuery);
                            if (responseGRNNo != null)
                            {
                                addPurchaseOrderCommand.OrderNumber = responseGRNNo;
                            }

                            //------------------------
                            addPurchaseOrderCommand.PurchaseOrderItems = VerifiedPurchaseOrderItems;

                            decimal TotalSaleAmount = addPurchaseOrderCommand.PurchaseOrderItems.Sum(x => Convert.ToDecimal(x.SalesPrice * x.Quantity));
                            decimal TotalAmount = addPurchaseOrderCommand.PurchaseOrderItems.Sum(x => Convert.ToDecimal(x.UnitPrice * x.Quantity));
                            addPurchaseOrderCommand.TotalAmount = TotalAmount;
                            addPurchaseOrderCommand.TotalSaleAmount = TotalSaleAmount;

                            // if (ResponseStatus == true)
                            //{
                            var result = await _mediator.Send(addPurchaseOrderCommand);
                            if (result.Success)
                            {
                                ResponseStatus = true;
                            }
                            else
                            {
                                ResponseStatus = false;
                            }
                            //}
                            //else
                            //{
                            //    ResponseStatus = false;
                            //}
                        }
                    }



                    if (ResponseStatus == true)
                    {
                        response.status = true;
                        response.StatusCode = 1;
                        response.message = "Success";

                    }
                    else
                    {
                        response.status = false;
                        response.StatusCode = 0;
                        response.message = "Invalid";

                    }

                }
                catch (Exception ex)
                {
                    response.status = false;
                    response.StatusCode = 0;
                    response.message = "Invalid - " + ex.Message;
                }
            }
            else
            {
                response.status = false;
                response.StatusCode = 0;
                response.message = "Invalid - Please select file to upload";
            }

            return Ok(response);
            //===============================
        }
    }
}
