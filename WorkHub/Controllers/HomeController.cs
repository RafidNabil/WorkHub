using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkHub.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult ManageUsers()
        {
            return View();
        }

        // GET: Home/CreateUser
        [HttpGet]
        public ActionResult CreateUser()
        {
            return View();
        }

        // POST: Home/CreateUser
        [HttpPost]
        public ActionResult CreateUser(string firstname, string lastname, string email, string password, bool isProjectManager)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    Password = password,
                    IsProjectManager = isProjectManager
                };

                return RedirectToAction("Index");
            }

            return View();
        }

        public ActionResult UpdateUser()
        {
            return View();
        }

        public ActionResult ApplicationSettings()
        {
            return View();
        }

        public ActionResult PermissionManagement()
        {
            return View();
        }

    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsProjectManager { get; set; }
    }
}
