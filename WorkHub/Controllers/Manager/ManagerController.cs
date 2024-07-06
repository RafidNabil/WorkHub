using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WorkHub.Models;
using WorkHub.Services;

namespace WorkHub.Controllers
{
    public class ManagerController : Controller
    {
        private readonly IMongoCollection<UserSignUp> _userCollection;
        private readonly IMongoCollection<CreateaProject> _projectCollection;

        public ManagerController()
        {
            // Initialize MongoDB collections
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
            _projectCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
        }

        // GET: Manager
        public ActionResult ManagerIndex()
        {
            return View();
        }

        // GET: Manager/CreateProject (This is for displaying the form)
        public ActionResult CreateProject()
        {
            try
            {
                // Fetch users from MongoDB
                var users = _userCollection.Find(_ => true).ToList();

                // Pass the users list to the view
                ViewBag.Users = users;

                // Return the view with the model (CreateaProject)
                return View(new CreateaProject());
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, or show appropriate messages to the user
                ModelState.AddModelError("", "An error occurred while fetching users.");
                // You might want to log the exception details
                // Logger.LogException(ex);

                // Return an empty view or handle the error gracefully
                return View(new CreateaProject());
            }
        }

        // POST: Manager/CreateProject (This is for handling the form submission)
        [HttpPost]
        public ActionResult CreateProject(CreateaProject model)
        {
            try
            {
                
                    model.Status = "Ongoing";
                    // Insert the model into MongoDB
                    _projectCollection.InsertOne(model);

                    // Optionally, you can redirect to another action upon successful submission
                    return RedirectToAction("ManagerDashboard");
                
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, or show appropriate messages to the user
                ModelState.AddModelError("", "An error occurred while saving the project.");
                // You might want to log the exception details
                // Logger.LogException(ex);
            }

            // If model state is not valid or an error occurred, return the view with validation errors
            // Also, ensure to fetch users again to populate the select list
            var users = _userCollection.Find(_ => true).ToList();
            ViewBag.Users = users;
            return View(model);
        }

        public ActionResult ViewProjects()
        {
            try
            {
                // Fetch projects from MongoDB
                var projects = _projectCollection.Find(_ => true).ToList();

                // Pass projects to the view
                return View(projects);
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, or show appropriate messages to the user
                ModelState.AddModelError("", "An error occurred while fetching projects.");
                // You might want to log the exception details
                // Logger.LogException(ex);

                // Return an empty view or handle the error gracefully
                return View(new List<CreateaProject>());
            }
        }

        public ActionResult ProjectDetails(string id)
        {
            try
            {
                // Fetch project details from MongoDB
                var project = _projectCollection.Find(p => p.Id == id).FirstOrDefault();
                if (project == null)
                {
                    return HttpNotFound();
                }

                // Fetch team member names using their IDs
                var teamMemberNames = new Dictionary<string, string>();
                foreach (var memberId in project.TeamMembers)
                {
                    // Assuming you have a method to fetch user details by ID from the Users collection
                    var user = _userCollection.Find(u => u.Id == memberId).FirstOrDefault();
                    if (user != null)
                    {
                        teamMemberNames.Add(user.Username, user.Role);
                    }
                    
                }

                // Pass project and team member names to the view
                ViewBag.TeamMemberNames = teamMemberNames;
                return View(project);
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, or show appropriate messages to the user
                ModelState.AddModelError("", "An error occurred while fetching project details.");
                // You might want to log the exception details
                // Logger.LogException(ex);

                // Return an empty view or handle the error gracefully
                return View(new CreateaProject());
            }
        }


        public ActionResult ManagerDashboard()
        {
            return View();
        }
    }
}
