using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WhatToEat.Models.ViewModels.Diary
{
    public class DiaryVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int Calorie { get; set; }
        public int Total { get { return Quantity * Calorie; } }
        public string Image { get; set; }
    }
}