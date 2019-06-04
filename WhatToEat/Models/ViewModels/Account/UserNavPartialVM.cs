using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WhatToEat.Models.ViewModels.Account
{
    public class UserNavPartialVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int DailyCalorie { get; set; }
       
    }
}