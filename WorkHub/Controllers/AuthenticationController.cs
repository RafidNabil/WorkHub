using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkHub.Services;
using WorkHub.Models;
using MongoDB.Driver;
using System.Web.Security;

namespace WorkHub.Controllers
{
    public class AuthenticationController : Controller
    {

        private IMongoCollection<UserSignUp> _userCollection;

        public AuthenticationController()
        {
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
        }

        // GET: Authentication
        public ActionResult SignIn()
        {
            if (Request.Cookies["signedIn"] != null && Request.Cookies["signedIn"].Value == "true")
            {
                var userRole = Request.Cookies["UserRole"].Value;

                if (userRole == "manager")
                    return RedirectToAction("ManagerDashboard", "Manager");
                else if (userRole == "admin")
                    return RedirectToAction("AdminDashboard", "Admin");
                else if (userRole == "developer")
                    return RedirectToAction("Dashboard", "Developer");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(UserSignUp model)
        {
            
                var user = _userCollection.AsQueryable().FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    if (user.Password == model.Password)
                    {
                    Response.Cookies["signedIn"].Value = "true";
                    Response.Cookies["signedIn"].Expires = DateTime.Now.AddDays(1);

                    Response.Cookies["UserRole"].Value = user.Role;
                    Response.Cookies["UserRole"].Expires = DateTime.Now.AddDays(1);

                    Response.Cookies["UserName"].Value = user.Username;
                    Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(1);

                    Response.Cookies["UserID"].Value = user.Id;
                    Response.Cookies["UserID"].Expires = DateTime.Now.AddDays(1);

                    Session["UserRole"] = user.Role;

                    if (user.Role == "manager")
                    return RedirectToAction("ManagerDashboard", "Manager");
                    else if(user.Role == "admin")
                        return RedirectToAction("AdminDashboard", "Admin");
                    else if(user.Role == "developer")
                        return RedirectToAction("Dashboard", "Developer");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            

            
        }

        public ActionResult SignUp()
        {
            
            /*if (Request.Cookies["signedUp"] != null && Request.Cookies["signedUp"].Value == "true")
            {

                return RedirectToAction("SignIn", "Authentication");
            }*/

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(UserSignUp model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _userCollection.AsQueryable().FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists. Please use a different email.");
                    return View(model);
                }

                _userCollection.InsertOne(model);
                Response.Cookies["signedUp"].Value = "true";
                Response.Cookies["signedUp"].Expires = DateTime.Now.AddDays(1);

                return RedirectToAction("SignIn", "Authentication");
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            // Clear the session
            Session.Clear();

            // Remove the authentication cookie
            if (Request.Cookies["signedIn"] != null)
            {
                var cookie = new HttpCookie("signedIn")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            if (Request.Cookies["UserRole"] != null)
            {
                var cookie = new HttpCookie("UserRole")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }


            return RedirectToAction("SignIn", "Authentication");
        }
    }
}