using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkHub.Controllers
{
    public class SuperAdminController : Controller
    {

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

        public ActionResult SuperAdminDashboard()
        {
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

        public ActionResult SuperAdminIndex()
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
