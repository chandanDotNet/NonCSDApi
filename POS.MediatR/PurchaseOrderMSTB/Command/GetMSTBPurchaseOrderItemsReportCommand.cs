﻿using MediatR;
using POS.Data.Resources;
using POS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MediatR.PurchaseOrderMSTB.Command
{
    public class GetMSTBPurchaseOrderItemsReportCommand : IRequest<MSTBPurchaseOrderItemList>
    {
        public MSTBPurchaseOrderResource MSTBPurchaseOrderResource { get; set; }
    }
}
