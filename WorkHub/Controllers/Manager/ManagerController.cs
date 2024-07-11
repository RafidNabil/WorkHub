using MongoDB.Bson;
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
    public class ManagerController : Controller
    {
        private readonly IMongoCollection<CreateaProject> _projectCollection;
        private readonly IMongoCollection<UserSignUp> _userCollection;

        public ManagerController()
        {
            _projectCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
        }

        [HttpGet]
        public ActionResult CreateProject()
        {
            var users = _userCollection.Find(FilterDefinition<UserSignUp>.Empty).ToList();
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public ActionResult CreateProject(CreateaProject model)
        {
            if (ModelState.IsValid)
            {
                model.Id = ObjectId.GenerateNewId().ToString();
                _projectCollection.InsertOne(model);
                return RedirectToAction("CreateProject", "Manager");
            }

            var users = _userCollection.Find(FilterDefinition<UserSignUp>.Empty).ToList();
            ViewBag.Users = users;
            return View(model);
        }

        public ActionResult ManagerIndex()
        {
            // Your code for displaying the manager index page
            return View();
        }

        public ActionResult ManagerDashboard()
        {
            return View();
        }
    }
}
