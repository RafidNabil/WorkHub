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
    public class AdminController : Controller
    {
        private IMongoCollection<UserSignUp> _userCollection;
        public AdminController()
        {
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
        }

        // GET: Admin
        public ActionResult AdminIndex()
        {
            return View();
        }

        // Action to display the user list and update form
        public ActionResult AdminDashboar()
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
    }
}