using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkHub.Models;
using WorkHub.Services;

namespace WorkHub.Controllers
{
    public class SuperAdminController : Controller
    {

        private IMongoCollection<UserSignUp> _userCollection;
        public SuperAdminController()
        {
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
        }

        public ActionResult ManageUsers()
        {
            var users = _userCollection.Find(_ => true).ToList();
            return View(users);
        }

        // Action to handle form submission for updating user roles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserRole(string id, string role)
        {
            var filter = Builders<UserSignUp>.Filter.Eq(u => u.Id, id);
            var update = Builders<UserSignUp>.Update.Set(u => u.Role, role);

            _userCollection.UpdateOne(filter, update);
            return RedirectToAction("AdminDashboar");

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
