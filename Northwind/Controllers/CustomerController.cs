using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Northwind.Models;
using Northwind.Security;
using System.Web.Security;
using System.Net;

namespace Northwind.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer/SignIn
        public ActionResult SignIn()
        {
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // create drop-down list box for company name
                ViewBag.CustomerID = new SelectList(db.Customers.OrderBy(c => c.CompanyName), "CustomerID", "CompanyName").ToList();
                return View();
            }
        }
        
        // POST: Customer/SignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn([Bind(Include = "CustomerId,Password")] CustomerSignIn customerSignIn, FormCollection Form, string ReturnUrl)
        {
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // find customer by CustomerId
                Customer customer = db.Customers.Find(customerSignIn.CustomerId);
                // hash & salt the posted password
                string str = UserAccount.HashSHA1(customerSignIn.Password + customer.UserGuid);
                // Compared posted Password to customer password
                if (str == customer.Password)
                {
                    // Passwords match
                    // authenticate user (this stores the CustomerID in an encrypted cookie) - normally, this would require HTTPS
                    FormsAuthentication.SetAuthCookie(customer.CustomerID.ToString(), false);
                    // send a cookie to the client to indicate that this is a customer
                    HttpCookie myCookie = new HttpCookie("role");
                    myCookie.Value = "customer";
                    Response.Cookies.Add(myCookie);

                    // if there is a return url, redirect to the url
                    if (ReturnUrl != null)
                    {
                        return Redirect(ReturnUrl);
                    }
                    // Redirect to Home page
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }
                else
                {
                    // Passwords do not match
                }
                // create drop-down list box for company name
                ViewBag.CustomerID = new SelectList(db.Customers.OrderBy(c => c.CompanyName), "CustomerID", "CompanyName").ToList();
                return View();
            }
        }

        // GET: Customer/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Customer/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Email,Password,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax")] CustomerRegister customerRegister)
        {
            // Add new customer to database
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                //create Customer from the CustomerRegister
                Customer customer = customerRegister.MapToCustomer();
                // first, make sure the CompanyName is unique
                if (db.Customers.Any(c => c.CompanyName == customer.CompanyName))
                {
                    // duplicate CompanyName
                    ModelState.AddModelError("CompanyName", "Duplicate Company Name");
                    return View();
                }
                // Generate guid for this customer
                customer.UserGuid = System.Guid.NewGuid();
                // Hash & Salt the customer Password using SHA-1 algorithm
                customer.Password = UserAccount.HashSHA1(customer.Password + customer.UserGuid);
                // Save customer to database
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction(actionName: "Index", controllerName: "Home");
                //return View();
            }
        }

        // GET: Customer/Orders
        [Authorize]
        public ActionResult Orders()
        {
            if (Request.Cookies["role"].Value != "customer")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.CustomerID = UserAccount.GetUserID();
            return View();
        }
        // GET: Customer/Account
        [Authorize]
        public ActionResult Account()
        {
            if (Request.Cookies["role"].Value != "customer")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //ViewBag.CustomerID = UserAccount.GetUserID();
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // find customer using CustomerID (stored in authentication ticket)
                Customer customer = db.Customers.Find(UserAccount.GetUserID());
                // display original values in textboxes when customer is editing data
                CustomerEdit EditCustomer = new CustomerEdit()
                {
                    CompanyName = customer.CompanyName,
                    ContactName = customer.ContactName,
                    ContactTitle = customer.ContactTitle,
                    Address = customer.Address,
                    City = customer.City,
                    Region = customer.Region,
                    PostalCode = customer.PostalCode,
                    Country = customer.Country,
                    Phone = customer.Phone,
                    Fax = customer.Fax,
                    Email = customer.Email
                };
                return View(EditCustomer);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Account([Bind(Include = "CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax,Email")] CustomerEdit UpdatedCustomer)
        {
            // For future version, make sure that an authenticated user is a customer
            if (Request.Cookies["role"].Value != "customer")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                Customer customer = db.Customers.Find(UserAccount.GetUserID());
                // if the customer is changing their CompanyName
                if (customer.CompanyName.ToLower() != UpdatedCustomer.CompanyName.ToLower())
                {
                    // Ensure that the CompanyName is unique
                    if (db.Customers.Any(c => c.CompanyName == UpdatedCustomer.CompanyName))
                    {
                        // duplicate CompanyName
                        ModelState.AddModelError("CompanyName", "Duplicate Company Name");
                        return View(UpdatedCustomer);
                    }
                    customer.CompanyName = UpdatedCustomer.CompanyName;
                }
                customer.Address = UpdatedCustomer.Address;
                customer.City = UpdatedCustomer.City;
                customer.ContactName = UpdatedCustomer.ContactName;
                customer.ContactTitle = UpdatedCustomer.ContactTitle;
                customer.Country = UpdatedCustomer.Country;
                customer.Email = UpdatedCustomer.Email;
                customer.Fax = UpdatedCustomer.Fax;
                customer.Phone = UpdatedCustomer.Phone;
                customer.PostalCode = UpdatedCustomer.PostalCode;
                customer.Region = UpdatedCustomer.Region;

                db.SaveChanges();
                return RedirectToAction(actionName: "Index", controllerName: "Home");
            }
        }

        public ActionResult ForgotPassword()
        {
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                ViewBag.CustomerID = new SelectList(db.Customers.OrderBy(c => c.CompanyName), "CustomerID", "CompanyName").ToList();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(FormCollection Form, string ReturnUrl)
        {
            using(NORTHWNDEntities db = new NORTHWNDEntities()){

                // Fetch Customer By ID
                Customer customer = db.Customers.Find(int.Parse(Form["CustomerId"]));
                // Generate Token To Send To Customer
                DateTime now = DateTime.Now;
                string Token = UserAccount.HashSHA1(now.ToString() + customer.UserGuid);
                //Get domain name authority (including port number, if required) that this app is running on
                String Authority = Request.Url.GetLeftPart(UriPartial.Authority);

                // Send Customer Email
                Gmailer gmailer = new Gmailer();
                gmailer.ToEmail = customer.Email;
                gmailer.Subject = "Northwind Password Reset";
                gmailer.Body = "<p>Hello " 
                + customer.ContactName + ",</p>" + "<p>Somebody recently asked to reset your Northwind Store password.</p>" + "<p><a href='" 
                + Authority + "/Customer/ChangePassword?token=" 
                + Token + "'>Click here to change your password.</a></p>" + "<p>If you didn't request a new password, <a href='" 
                + Authority + "/Customer/UnauthorizedForgetPasswordRequest?" 
                + Token + "'>let us know</a>.</p>";
                gmailer.IsHtml = true;
                gmailer.Send();

                // Add Token to the Database
                PasswordRequest pw = new PasswordRequest();
                pw.CustomerID = customer.CustomerID;
                pw.TimeCreated = now;
                pw.Token = Token;
                db.PasswordRequests.Add(pw);
                db.SaveChanges();


                // Redirect to Success Page
                ViewBag.Company = customer.CompanyName;
                ViewBag.Email = gmailer.ToEmail;
                return View("ForgotPasswordSent");
            }
        }

        [HttpGet]
        public ActionResult ChangePassword(string Token)
        {
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                // Find 
                PasswordRequest pw = db.PasswordRequests.Where(t => t.Token == Token).FirstOrDefault();
                if (pw == null)
                {
                    ViewBag.Error = "Incorrect or Expired Password Reset Request";
                    return View();
                }

                //removing all outdated requests
                DateTime OneDayAgo = DateTime.Now.AddDays(-1);
                //selecting old requests
                var OldRequests =
                    from pr in db.PasswordRequests
                    where pr.TimeCreated < OneDayAgo
                    select pr;
                //removing selected
                db.PasswordRequests.RemoveRange(OldRequests);
                db.SaveChanges();

                // Compare two times
                DateTime expires = DateTime.Parse(pw.TimeCreated.ToString()).AddDays(1);
                DateTime now = DateTime.Now;

                
                // Check if it's valid
                if (now > expires)
                {
                    ViewBag.Error = "Incorrect or Expired Password Reset Request";
                    return View();
                }

                ViewBag.Token = pw.Token;
            }

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection Form, string ReturnUrl) {

            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                string Token = Form["Token"];
                string Password = Form["password"];
                string PasswordVerify = Form["password-verify"];

                if (Password != "" && PasswordVerify != "" && Password.Equals(PasswordVerify))
                {
                    // Get Password Request
                    PasswordRequest pw = db.PasswordRequests.Where(p => p.Token == Token).FirstOrDefault();
                    Customer c = db.Customers.Find(pw.CustomerID);

                    // Update Customer Password
                    c.Password = UserAccount.HashSHA1(Password + c.UserGuid);

                    //db.PasswordRequests.Remove(db.PasswordRequests.Where(f => f.TimeCreated <= DateTime ));

                    // Delete Password Reset Request
                    db.PasswordRequests.Remove(pw);

                    // Update DB
                    db.SaveChanges();

                    // Send Customer Email
                    Gmailer gmailer = new Gmailer();
                    gmailer.ToEmail = c.Email;
                    gmailer.Subject = "Password Reset Successful";
                    gmailer.Body = "<p>Hello " + c.ContactName + ",</p>" +
                    "<p>Your password has been reset successfully</p>";
                    gmailer.IsHtml = true;
                    gmailer.Send();

                    // Forward to Success Page
                    return View("ForgotPasswordSuccess");
                }
                else
                {
                    TempData["Error"] = "Password Doesn't Match Or Is Invalid";
                    return RedirectToAction("ChangePassword", "Customer", new { Token = Token });
                }
            }
        }

        public ActionResult UnauthorizedForgetPasswordRequest(string Token)
        {
            using (NORTHWNDEntities db = new NORTHWNDEntities())
            {
                PasswordRequest pw = db.PasswordRequests.Where(t => t.Token == Token).FirstOrDefault();
                if (pw == null)
                {
                    ViewBag.Error = "Incorrect or Expired Password Reset Request";
                    return View();
                }

                db.PasswordRequests.Remove(pw);
                db.SaveChanges();

                return View();
            }
        }
    }
}