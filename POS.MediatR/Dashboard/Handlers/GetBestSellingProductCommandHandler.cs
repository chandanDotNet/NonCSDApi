﻿using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Data.Dto;
using POS.MediatR.Dashboard.Commands;
using POS.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POS.MediatR.Dashboard.Handlers
{
    public class GetBestSellingProductCommandHandler : IRequestHandler<GetBestSellingProductCommand, List<BestSellingProductStatisticDto>>
    {
        private readonly ISalesOrderItemRepository _salesOrderItemRepository;

        public GetBestSellingProductCommandHandler(ISalesOrderItemRepository salesOrderItemRepository)
        {
            _salesOrderItemRepository = salesOrderItemRepository;
        }

        public async Task<List<BestSellingProductStatisticDto>> Handle(GetBestSellingProductCommand request, CancellationToken cancellationToken)
        {
            var bestSellingProductStatistics = await _salesOrderItemRepository.AllIncluding(c => c.SalesOrder, cs => cs.Product)
                .Where(c => c.SalesOrder.CreatedDate.Month == request.Month && c.SalesOrder.CreatedDate.Year == request.Year && c.SalesOrder.ProductMainCategoryId == request.ProductMainCategoryId && c.Product.BrandId!= new Guid("A9F6308B-C7F7-42A2-0AFD-08DC0694AD26"))
                .GroupBy(c => c.ProductId)
                .Select(cs => new BestSellingProductStatisticDto
                {
                    Name = cs.FirstOrDefault().Product.Name,
                    Count = cs.Sum(item => item.Quantity)
                })
                .OrderByDescending(c => c.Count)
                .Take(10)
                .ToListAsync();

            //using (var context = new DatabaseContext())
            //{
            //    var clientIdParameter = new SqlParameter("@ClientId", 4);

            //    var result = context.Database
            //        .SqlQuery<ResultForCampaign>("GetResultsForCampaign @ClientId", clientIdParameter)
            //        .ToList();
            //}

            //string connectionString = "Server=10.200.109.231,1433;Database=NonCSD_29_01_2024;user=sa; password=Shyam@2023;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=FALSE";

            //BestSellingProductStatisticDto bestSellingProductStatisticDto = new BestSellingProductStatisticDto();
            //List<BestSellingProductStatisticDto> bestSellingProductList = new List<BestSellingProductStatisticDto>();
            //using (SqlConnection con = new SqlConnection(connectionString))
            //{
            //    SqlCommand cmd = new SqlCommand("SP_BestSellingProductStatistic", con);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.AddWithValue("@ActionId", 1);

            //    con.Open();
            //    SqlDataReader rdr = cmd.ExecuteReader();

            //    while (rdr.Read())
            //    {
            //        bestSellingProductStatisticDto.Name = rdr["Name"].ToString();
            //        bestSellingProductStatisticDto.Count = (decimal)rdr["Count"];
            //        bestSellingProductList.Add(bestSellingProductStatisticDto);
            //    }
            //    con.Close();
            //}


            // return bestSellingProductList;
            return bestSellingProductStatistics;
        }


    }
}
