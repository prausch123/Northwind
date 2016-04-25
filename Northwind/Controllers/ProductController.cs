using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Northwind.Models;
using System.Net;

namespace Northwind.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product/Category
        public ActionResult Category()
        {
            // retrieve a list of all categories
            using(NORTHWNDEntities db = new NORTHWNDEntities()) {
                return View(db.Categories.OrderBy(c => c.CategoryName).ToList());
            }
        }

        // GET: Product/Product/1
        public ActionResult Product(int? id)
        {
            // if there is no "category" id, return Http Bad Request
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // save the select category name to the ViewBag
                ViewBag.Filter = db.Categories.Find(id).CategoryName;
                // retrieve list of products
                return View(db.Products.Where(p => p.CategoryID == id && p.Discontinued == false).OrderBy(p => p.ProductName).ToList());
            }
        }

        // GET: Product/Search
        public ActionResult SearchForm()
        {
            return View();
        }

        // POST: Product/SearchResults
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchResults(FormCollection Form)
        {
            string SearchString = Form["SearchString"];
            ViewBag.Filter = "Product";
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                return View("Product", db.Products.Where(p => p.ProductName.Contains(SearchString) && p.Discontinued == false).OrderBy(p => p.ProductName).ToList());
            }
        }

        // GET: Product/Discount
        public ActionResult Discount()
        {
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // Filter by date
                DateTime now = DateTime.Now;
                return View(db.Discounts.Where(s => s.StartTime <= now && s.EndTime > now).ToList());
            }
        }
    }
}