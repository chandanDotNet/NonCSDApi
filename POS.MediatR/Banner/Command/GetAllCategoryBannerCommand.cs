﻿using MediatR;
using POS.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MediatR.Banner.Command
{
    public class GetAllCategoryBannerCommand : IRequest<List<CategoryBannerDto>>
    {
        public string Type { get; set; }
    }
}
