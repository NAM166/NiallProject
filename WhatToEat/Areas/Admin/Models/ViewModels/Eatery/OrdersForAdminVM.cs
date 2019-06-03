using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhatToEat.Areas.Admin.Models.ViewModels.Eatery
{
    public class OrdersForAdminVM
    {
        public int OrderNumber { get; set; }
        public string Username { get; set; }
        public int Total { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}