﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Data.Dto
{
    public class UserSupplierDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? SupplierId { get; set; }
    }
}
