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
                return RedirectToAction("ManagerDashboard", "Manager");
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

                    Session["UserRole"] = user.Role;

                    return RedirectToAction("ManagerDashboard", "Manager");
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
        public ActionResult SignUp(UserSignUp user)
        {

            if (ModelState.IsValid)
            {
                var userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
                userCollection.InsertOne(user);

                Response.Cookies["signedUp"].Value = "true";
                Response.Cookies["signedUp"].Expires = DateTime.Now.AddDays(1);

                return RedirectToAction("SignIn", "Authentication");
            }

            return View(user);
        }
    }
}