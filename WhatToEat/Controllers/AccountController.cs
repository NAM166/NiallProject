﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WhatToEat.Models.Data;
using WhatToEat.Models.ViewModels.Account;
using WhatToEat.Models.ViewModels.Diary;
using WhatToEat.Models.ViewModels.Eatery;


namespace WhatToEat.Controllers
{

    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            // Confirm user is not logged in

            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            // Return view
            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the user is valid

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    DailyCalorie = model.DailyCalorie,
                    Username = model.Username,
                    Password = model.Password
                };

                // Add the DTO
                db.Users.Add(userDTO);

                // Save
                db.SaveChanges();

                // Add to UserRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();
            }

            // Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            // Redirect
            return Redirect("~/account/login");
        }

        // GET: /account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                // Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    DailyCalorie = dto.DailyCalorie
                };
            }

            // Return partial view with model
            return PartialView(model);
        }


        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                // Build model
                model = new UserProfileVM(dto);
            }

            // Return view with model
            return View("UserProfile", model);
        }

        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Check if passwords match
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string username = User.Identity.Name;

                // Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.DailyCalorie = model.DailyCalorie;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // Save
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited your profile!";

            // Redirect
            return Redirect("~/account/user-profile");
        }

        // GET: /account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            // Init list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                // Get user id
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init products dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    int total = 0;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Loop though list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product calorie
                        int calorie = product.Calorie;

                        // Get product name
                        string productName = product.Name;

                        // Add to products dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * calorie;
                    }

                    // Add to OrdersForUserVM list
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }

            }

            // Return view with list of OrdersForUserVM
            return View(ordersForUser);
        }

        //public ActionResult DailyCaloriePartial()
        //{
        //    // Init VM
        //    UserVM model = new UserVM();

        //    // Init Calorie
        //    int dailycalorie = 0;

        //    // Check for diary session
        //    if (Session["diary"] != null)
        //    {
        //        // Get DailyCalorie
        //        var list = (List<UserVM>)Session["diary"];

        //        foreach (var item in list)
        //        {
        //            dailycalorie += item.DailyCalorie;
        //        }

        //        model.DailyCalorie = dailycalorie;

        //    }
        //    else
        //    {
        //        // Or set Calorie to 0
        //        model.DailyCalorie = 0;
        //    }

        //    // Return partial view with model
        //    return PartialView(model);
        //}
    }
}