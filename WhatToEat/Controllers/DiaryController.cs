using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WhatToEat.Models.Data;
using WhatToEat.Models.ViewModels.Account;
using WhatToEat.Models.ViewModels.Diary;
using System.Net;
using System.Net.Mail;

namespace WhatToEat.Controllers
{
    public class DiaryController : Controller
    { 
        // GET: Diary
        public ActionResult Index()
        {
            // Init the Diary list
            var Diary = Session["diary"] as List<DiaryVM> ?? new List<DiaryVM>();

            // Check if diary is empty
            if (Diary.Count == 0 || Session["diary"] == null)
            {
                ViewBag.Message = "Your Diary is Empty.";
                return View();
            }

            // Calculate total and save to ViewBag

            int total = 0;

            foreach (var item in Diary)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Return view with list
            return View(Diary);
        }

        public ActionResult DiaryPartial()
        {
            // Init DiaryVM
            DiaryVM model = new DiaryVM();

            // Init quantity
            int qty = 0;

            // Init Calorie
            int calorie = 0;

            // Check for diary session
            if (Session["diary"] != null)
            {
                // Get total qty and Calorie
                var list = (List<DiaryVM>)Session["diary"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    calorie += item.Quantity * item.Calorie;
                }

                model.Quantity = qty;
                model.Calorie = calorie;

            }
            else
            {
                // Or set qty and Calorie to 0
                model.Quantity = 0;
                model.Calorie = 0;
            }

            // Return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToDiaryPartial(int id)
        {
            // Init diaryVM list
            List<DiaryVM> diary = Session["diary"] as List<DiaryVM> ?? new List<DiaryVM>();

            // Init DiaryVM
            DiaryVM model = new DiaryVM();

            using (Db db = new Db())
            {
                // Get the product
                ProductDTO product = db.Products.Find(id);

                // Check if the product is already in Diary
                var productInDiary = diary.FirstOrDefault(x => x.ProductId == id);

                // If not, add new
                if (productInDiary == null)
                {
                    diary.Add(new DiaryVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Calorie = product.Calorie,
                        Image = product.ImageName
                    });
                }
                else
                {
                    // If it is, increment
                    productInDiary.Quantity++;
                }
            }

            // Get total qty and Calorie and add to model

            int qty = 0;
            int calorie = 0; 

            foreach (var item in diary)
            {
                qty += item.Quantity;
                calorie += item.Quantity * item.Calorie;
            }

            model.Quantity = qty;
            model.Calorie = calorie;

            // Save Diary back to session
            Session["diary"] = diary;

            // Return partial view with model
            return PartialView(model);
        }

        // GET: /Diary/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            // Init Diary list
            List<DiaryVM> diary = Session["diary"] as List<DiaryVM>;

            using (Db db = new Db())
            {
                // Get DiaryVM from list
                DiaryVM model = diary.FirstOrDefault(x => x.ProductId == productId);

                // Increment qty
                model.Quantity++;

                // Store needed data
                var result = new { qty = model.Quantity, calorie = model.Calorie };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /Diary/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            // Init Diary
            List<DiaryVM> diary = Session["diary"] as List<DiaryVM>;

            using (Db db = new Db())
            {
                // Get model from list
                DiaryVM model = diary.FirstOrDefault(x => x.ProductId == productId);

                // Decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    diary.Remove(model);
                }

                // Store needed data
                var result = new { qty = model.Quantity, Calorie = model.Calorie };

                // Return json
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /Diary/RemoveProduct
        public void RemoveProduct(int productId)
        {
            // Init Diary list
            List<DiaryVM> diary = Session["diary"] as List<DiaryVM>;

            using (Db db = new Db())
            {
                // Get model from list
                DiaryVM model = diary.FirstOrDefault(x => x.ProductId == productId);

                // Remove model from list
                diary.Remove(model);
            }

        }

        // POST: /Diary/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            // Get Diary list
            List<DiaryVM> diary = Session["diary"] as List<DiaryVM>;

            // Get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to OrderDetailsDTO
                foreach (var item in diary)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }

            }
        }

        //        public ActionResult DailyCaloriePartial()
        //{
        //    // Init DiaryVM
        //    DiaryVM model = new DiaryVM();

        //    // Init Calorie
        //    int dailycalorie = 0;

        //    // Check for diary session
        //    if (Session["diary"] != null)
        //    {
        //        // Get total qty and Calorie
        //        var list = (List<DiaryVM>)Session["diary"];

        //        foreach (var item in list)
        //        {
        //            dailycalorie += item.DailyCalorie;
        //        }

        //        model.DailyCalorie = dailycalorie;

        //    }
        //    else
        //    {
        //        // Or set Calorie to 0
        //        model.DailyCalorie = 1;
        //    }

        //    // Return partial view with model
        //    return PartialView(model);
        //}
    }
    }

