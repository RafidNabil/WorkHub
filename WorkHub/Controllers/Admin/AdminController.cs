using Microsoft.Ajax.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WorkHub.Models;
using WorkHub.Services;

namespace WorkHub.Controllers
{
    public class AdminController : Controller
    {
        private IMongoCollection<UserSignUp> _userCollection;
        private IMongoCollection<CreateaProject> _projectCollection;
        private IMongoCollection<Tasks> _taskCollection;
        public readonly IMongoCollection<UserProfile> _userProfilesCollection;
        public readonly IMongoCollection<Comment> _commentCollection;
        public readonly IMongoCollection<Notification> _userNotificationsCollection;

        public AdminController()
        {
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
            _projectCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
            _taskCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");
            _userProfilesCollection = MongoDBHelper.GetCollection<UserProfile>("UserProfiles");
            _commentCollection = MongoDBHelper.GetCollection<Comment>("Comments");
            _userNotificationsCollection = MongoDBHelper.GetCollection<Notification>("Notifications");
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

        public ActionResult Statistics()
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

        public ActionResult GanttChart()
        {
            var projects = _projectCollection.Find(_ => true).ToList();
            var jsonProjects = JsonConvert.SerializeObject(projects);
            ViewBag.JsonProjects = jsonProjects;
            return View();
        }

        public ActionResult AdminDashboard()
        {
            var projects = _projectCollection.Find(_ => true).ToList();
            var users = _userCollection.Find(_ => true).ToList();
            var tasks = _taskCollection.Find(_ => true).ToList();

            
            int projectCount = projects.Count;

            
            int totalUsers = users.Count;

            
            int totalTaskss = tasks.Count;

            
            int finishedProjects = projects.Count(p => p.Status == "Finished");
            int ongoingProjects = projects.Count(p => p.Status == "Ongoing");
            int todoTasks = tasks.Count(p => p.Status == "To Do");
            int inprogressTasks = tasks.Count(p => p.Status == "In Progress");
            int doneTasks = tasks.Count(p => p.Status == "Done");

            
            int totalProjects = finishedProjects + ongoingProjects;
            double finishedPercentage = totalProjects > 0 ? Math.Round((double)finishedProjects / totalProjects * 100) : 0;
            double ongoingPercentage = totalProjects > 0 ? Math.Round((double)ongoingProjects / totalProjects * 100) : 0;
            int totaltasks = todoTasks + inprogressTasks + doneTasks;
            double todoPercentage = totaltasks > 0 ? Math.Round((double)todoTasks / totaltasks * 100) : 0;
            double inprogressPercentage = totaltasks > 0 ? Math.Round((double)inprogressTasks / totaltasks * 100) : 0;
            double donePercentage = totaltasks > 0 ? Math.Round((double)doneTasks / totaltasks * 100) : 0;


            
            ViewBag.ProjectCount = projectCount;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalTasks = totalTaskss;
            ViewBag.FinishedProjects = finishedProjects;
            ViewBag.OngoingProjects = ongoingProjects;
            ViewBag.FinishedPercentage = finishedPercentage;
            ViewBag.OngoingPercentage = ongoingPercentage;
            ViewBag.ToDoTasks = todoTasks;
            ViewBag.InProgressTasks = inprogressTasks;
            ViewBag.DoneTasks = doneTasks;
            ViewBag.ToDoPercentage = todoPercentage;
            ViewBag.InProgressPercentage = inprogressPercentage;
            ViewBag.DonePercentage = donePercentage;



            var latestProjects = _projectCollection.Find(_ => true)
                                                  .SortByDescending(p => p.Id)
                                                  .Limit(4)
                                                  .ToList();


            var latestTasks = _taskCollection.Find(_ => true)
                                            .SortByDescending(t => t.Id)
                                            .Limit(4)
                                            .ToList();

            ViewBag.LatestProjects = latestProjects;
            ViewBag.LatestTasks = latestTasks;

            return View();

        }


        public ActionResult AdminUserProfile()
        {
            var role = Request.Cookies["UserRole"].Value.ToString();
            var userId = Request.Cookies["UserID"].Value.ToString();
            ViewBag.Role = role;
            ViewBag.UserId = userId;

            List<Tasks> filteredTasks = new List<Tasks>();

            if (!string.IsNullOrEmpty(userId))
            {
                var taskCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");

                var filter = Builders<Tasks>.Filter.AnyEq(t => t.Assignee, userId);

                filteredTasks = taskCollection.Find(filter).ToList();
            }

            var user = _userProfilesCollection.Find(u => u.Id == userId).FirstOrDefault();
            return View((user, filteredTasks));
        }

        // GET: Manager
        public ActionResult AdminUpdateProfile(string UserId)
        {

            var user = _userProfilesCollection
                .Find(u => u.Id == UserId)
                .FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public ActionResult AdminUpdateProfile(UserProfile model)
        {

            var user = _userProfilesCollection
            .Find(u => u.Id == model.Id)
            .FirstOrDefault();

            if (user != null)
            {
                user.Username = model.Username;
                user.Email = model.Email;
                user.Status = model.Status;
                user.Country = model.Country;
                user.Languages = model.Languages.ToList();
                user.Phone = model.Phone;
                user.City = model.City;
                user.Designation = model.Designation;
                user.ProfilePicture = model.ProfilePicture;
                user.Facebook = model.Facebook;
                user.Twitter = model.Twitter;
                user.LinkedIn = model.LinkedIn;
                user.GitHub = model.GitHub;
                user.UpdatedAt = DateTime.Now;

                _userProfilesCollection.ReplaceOne(u => u.Id == model.Id, user);
            }

            return RedirectToAction("AdminUserProfile");
        }



        public ActionResult AdminIndex()
        {
            return View();
        }
    }
}
