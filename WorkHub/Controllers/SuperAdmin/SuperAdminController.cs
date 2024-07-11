using Microsoft.Ajax.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WorkHub.Models;
using WorkHub.Services;

namespace WorkHub.Controllers
{
    public class SuperAdminController : Controller
    {
        private IMongoCollection<UserSignUp> _userCollection;
        private IMongoCollection<CreateaProject> _projectCollection;
        public SuperAdminController()
        {
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
            _projectCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
        }

        public ActionResult ManageUsers()
        {
            var users = _userCollection.Find(_ => true).ToList();
            return View(users);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserRole(string id, string role)
        {
            var filter = Builders<UserSignUp>.Filter.Eq(u => u.Id, id);
            var update = Builders<UserSignUp>.Update.Set(u => u.Role, role);

            _userCollection.UpdateOne(filter, update);
            return RedirectToAction("manageusers");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            var filter = Builders<UserSignUp>.Filter.Eq(u => u.Id, id);

            _userCollection.DeleteOne(filter);
            return RedirectToAction("manageusers");
        }


        public ActionResult ViewProject()
        {
            var projects = _projectCollection.Find(_ => true).ToList();
            return View(projects);
        }

        public ActionResult UserActivity()
        {
            var projects = _projectCollection.Find(_ => true).ToList();
            var users = _userCollection.Find(_ => true).ToList();

            var userDictionary = users.ToDictionary(u => u.Id.ToString(), u => new { u.Username, u.Email });
            //Id converted to String
            foreach (var project in projects)
            {
                if (project.TeamMembers != null && project.TeamMembers.Any())
                {
                    project.TeamMembers = project.TeamMembers
                        .Select(id => userDictionary.ContainsKey(id) ? $"{userDictionary[id].Username} ({userDictionary[id].Email})" : id)
                        .ToList();
                }
            }
            return View(projects);
        }

        public ActionResult SystemUsage()
        {
            var projects = _projectCollection.Find(_ => true).ToList();
            var users = _userCollection.Find(_ => true).ToList();

            int totalUsers = users.Count;
            int totalProjects = projects.Count;
            int ongoingProjects = projects.Count(p => p.Status == "Ongoing");
            int finishedProjects = projects.Count(p => p.Status == "Finished");

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProjects = totalProjects;
            ViewBag.OngoingProjects = ongoingProjects;
            ViewBag.FinishedProjects = finishedProjects;

            return View();
        }

        public ActionResult SuperAdminDashboard()
        {
            var lastTwoProjects = _projectCollection.Find(_ => true)
                                                   .SortByDescending(p => p.StartDate)
                                                   .Limit(2)
                                                   .ToList();
            return View(lastTwoProjects);
        }

        public ActionResult SuperAdminIndex()
        {
            return View();
        }
    }
}
