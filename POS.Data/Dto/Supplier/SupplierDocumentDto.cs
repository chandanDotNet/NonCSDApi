﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Data.Dto
{
    public class SupplierDocumentDto
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string Name { get; set; }
        public string Documents { get; set; }
    }
}
