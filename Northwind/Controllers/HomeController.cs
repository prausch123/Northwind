using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Northwind.Models;
using System.Web.Security;

namespace Northwind.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            using(NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // return a list of discounts
                //return View(db.Discounts.ToList());
                // return the first 3 discounts
                //return View(db.Discounts.Take(3).ToList());
                // Filter by date, return the first 3
                DateTime now = DateTime.Now;
                return View(db.Discounts.Where(s => s.StartTime <= now && s.EndTime > now).ToList().Take(3));
            }
        }

        // GET: Home/SignOut
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction(actionName: "SignedOut", controllerName: "Home");
        }


        // GET: Home/SignedOut
        public ActionResult SignedOut()
        {
            return View();
        }
    }
}