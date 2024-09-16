using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
        private readonly IMongoCollection<TaskCompletionRequest> _taskCompletionRequestCollection;
        public readonly IMongoCollection<Tasks> _tasksCollection;
        public readonly IMongoCollection<UserProfile> _userProfilesCollection;
        public readonly IMongoCollection<Comment> _commentCollection;

        public ManagerController()
        {
            // Initialize MongoDB collections
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
            _projectCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
            _taskCompletionRequestCollection = MongoDBHelper.GetCollection<TaskCompletionRequest>("TaskCompletionRequest");
            _tasksCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");
            _userProfilesCollection = MongoDBHelper.GetCollection<UserProfile>("UserProfiles");
            _commentCollection = MongoDBHelper.GetCollection<Comment>("Comment");
        }

        // GET: Manager
        public ActionResult ManagerIndex()
        {
            return View();
        }

        // GET: Manager/CreateProject (This is for displaying the form)
        public ActionResult CreateProject(string id, string title, string desc, string sdate, string edate, int act)
        {
            try
            {
                if (act == 1)
                {

                    var users = _userCollection.Find(_ => true).ToList();


                    ViewBag.Users = users;
                    ViewBag.act = 1;


                    return View(new CreateaProject());
                }
                else if (act == 2)
                {
                    var users = _userCollection.Find(_ => true).ToList();


                    ViewBag.act = 2;
                    ViewBag.Users = users;
                    CreateaProject model = new CreateaProject { ProjectName = title, Id = id, Description = desc, StartDate = DateTime.Parse(sdate), EndDate = DateTime.Parse(edate) };


                    return View(model);
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while fetching users.");



                return View(new CreateaProject());
            }

            return View(new CreateaProject());
        }


        [HttpPost]
        public ActionResult CreateProject(CreateaProject model, int func)
        {
            try
            {
                if (func == 1)
                {
                    if (model.Status == null)
                    {
                        model.Status = "Ongoing";
                    }

                    model.StartDate = model.StartDate.AddDays(1);
                    model.EndDate = model.EndDate.AddDays(1);
                    // Insert the model into MongoDB
                    _projectCollection.InsertOne(model);

                    // Optionally, you can redirect to another action upon successful submission
                    return RedirectToAction("ManagerDashboard");
                }
                else if (func == 2)
                {
                    // Fetch the existing project from MongoDB
                    var existingProject = _projectCollection.Find(p => p.Id == model.Id).FirstOrDefault();
                    if (existingProject == null)
                    {
                        ModelState.AddModelError("", "Project not found.");
                    }
                    else
                    {
                        // Update the existing project's fields
                        existingProject.ProjectName = model.ProjectName;
                        existingProject.Description = model.Description;
                        existingProject.StartDate = model.StartDate.AddDays(1);
                        existingProject.EndDate = model.EndDate.AddDays(1);
                        if (model.Status != null)
                        {
                            existingProject.Status = model.Status;
                        }
                        existingProject.TeamMembers = model.TeamMembers;

                        // Save the updated project back to MongoDB
                        _projectCollection.ReplaceOne(p => p.Id == existingProject.Id, existingProject);

                        return RedirectToAction("ManagerDashboard");
                    }
                }


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

        public ActionResult uploadProjectFiles(string id, HttpPostedFileBase[] attachments)
        {
            var existingProject = _projectCollection.Find(t => t.Id == id).FirstOrDefault();
            if (attachments != null && attachments.Length > 0)
            {
                UploadFiles(existingProject, attachments);
            }
            else
            {
                return RedirectToAction("ManagerDashboard");
            }

            return RedirectToAction("ProjectDetails", new { id = id });

        }

        private void UploadFiles(CreateaProject model, HttpPostedFileBase[] attachments)
        {
            var database = MongoDBHelper.GetDatabase();
            var bucket = new GridFSBucket(database);

            /*model.Attachments = new List<string>();*/

            foreach (var file in attachments)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // Generate a unique filename using GridFS ObjectId
                    var fileId = ObjectId.GenerateNewId();
                    var fileName = $"{fileId}{Path.GetExtension(file.FileName)}";

                    using (var stream = file.InputStream)
                    {
                        // Upload the file to GridFS bucket
                        bucket.UploadFromStream(fileName, stream);
                    }

                    // Create a TaskFile instance to store metadata
                    var projectFile = new Models.File
                    {
                        Id = fileId, // Use the ObjectId generated for the file
                        TaskID = model.Id.ToString(), // Assuming model has a Task ID
                        FileName = file.FileName,
                        FilePath = fileName, // You can store the filename or leave it empty as it is in GridFS
                        ContentType = file.ContentType,
                        Size = file.ContentLength,
                        UploadedDate = DateTime.Now
                    };

                    // Save the TaskFile metadata to the database
                    var projectFilesCollection = MongoDBHelper.GetCollection<Models.File>("ProjectFiles");
                    projectFilesCollection.InsertOne(projectFile);

                    // Store the file ID in the model's Attachments list
                    /*model.Attachments.Add(fileId.ToString());*/ // Add fileId to the list
                }
            }
        }

        public ActionResult DownloadFiles(string fileId)
        {
            try
            {
                // Parse the fileId to an ObjectId
                var objectId = new ObjectId(fileId);

                // Fetch the file metadata from TaskFiles collection
                var projectFilesCollection = MongoDBHelper.GetCollection<Models.File>("ProjectFiles");
                var projectFile = projectFilesCollection.Find(f => f.Id == objectId).FirstOrDefault();

                if (projectFile == null)
                {
                    return HttpNotFound("File not found.");
                }

                // Initialize the GridFS bucket
                var database = MongoDBHelper.GetDatabase();
                var bucket = new GridFSBucket(database);

                // Download the file from GridFS using the filename stored in TaskFiles
                byte[] fileBytes;
                using (var stream = new MemoryStream())
                {
                    bucket.DownloadToStreamByName(projectFile.FilePath, stream);
                    fileBytes = stream.ToArray();
                }

                // Return the file as a download
                return File(fileBytes, projectFile.ContentType, projectFile.FileName);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                ViewBag.Error = "Error downloading file: " + ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult DeleteFiles(string projectid, string[] fileIds)
        {
            if (fileIds != null && fileIds.Length > 0)
            {
                // Get the database instance from MongoDBHelper
                var database = MongoDBHelper.GetDatabase();

                // Get the TaskFiles to be deleted
                var projectFilesCollection = MongoDBHelper.GetCollection<Models.File>("ProjectFiles");
                var projectFilesToDelete = projectFilesCollection.Find(f => fileIds.Contains(f.Id.ToString())).ToList();

                foreach (var projectFile in projectFilesToDelete)
                {
                    // Get the file path which is used as the filename in fs.files
                    var filePath = projectFile.FilePath;

                    // Delete the file from fs.files using the FilePath
                    var filesCollection = database.GetCollection<BsonDocument>("fs.files");
                    var fileDocument = filesCollection.Find(Builders<BsonDocument>.Filter.Eq("filename", filePath)).FirstOrDefault();

                    if (fileDocument != null)
                    {
                        var fileId = fileDocument["_id"].AsObjectId;

                        // Delete the associated chunks from fs.chunks
                        var chunksCollection = database.GetCollection<BsonDocument>("fs.chunks");
                        chunksCollection.DeleteMany(Builders<BsonDocument>.Filter.Eq("files_id", fileId));

                        // Now delete the file from fs.files
                        filesCollection.DeleteMany(Builders<BsonDocument>.Filter.Eq("_id", fileId));
                    }
                }

                // Now delete the TaskFile records
                var filter = Builders<Models.File>.Filter.In(f => f.Id, projectFilesToDelete.Select(tf => tf.Id));
                projectFilesCollection.DeleteMany(filter);
            }

            return RedirectToAction("ProjectDetails", new { id = projectid });
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

                var project = _projectCollection.Find(p => p.Id == id).FirstOrDefault();
                if (project == null)
                {
                    return HttpNotFound();
                }


                var teamMemberNames = new Dictionary<string, string>();
                if (project.TeamMembers != null)
                {
                    foreach (var memberId in project.TeamMembers)
                    {

                        var user = _userCollection.Find(u => u.Id == memberId).FirstOrDefault();
                        if (user != null)
                        {
                            teamMemberNames.Add(user.Username, user.Role);
                        }

                    }
                }


                ViewBag.TeamMemberNames = teamMemberNames;

                ObjectId projectObjectId;
                if (!ObjectId.TryParse(id, out projectObjectId))
                {
                    return HttpNotFound();
                }


                var projectFilesCollection = MongoDBHelper.GetCollection<Models.File>("ProjectFiles");


                var projectFiles = projectFilesCollection.Find(file => file.TaskID == id).ToList();


                if (projectFiles == null || projectFiles.Count == 0)
                {

                    ViewBag.projectFiles = new List<Models.File>();
                }
                else
                {
                    ViewBag.TaskFiles = projectFiles;
                }

                var comments = _commentCollection.Find(c => c.RelatedId == id).ToList();

                Dictionary<string, string> commentUserPictures = new Dictionary<string, string>();
                foreach (var comment in comments)
                {
                    var user = _userProfilesCollection.Find(u => u.Id == comment.UserId).FirstOrDefault();
                    if (user != null)
                    {
                        commentUserPictures.Add(comment.Id, user.ProfilePicture);
                    }
                }

                ViewBag.CommentUserPictures = commentUserPictures;
                ViewBag.MyID = Request.Cookies["UserID"].Value.ToString();
                ViewBag.MyName = Request.Cookies["UserName"].Value.ToString();

                return View((project, projectFiles, comments));
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while fetching project details.");



                return View(new CreateaProject());
            }
        }

        public ActionResult PendingApproval()
        {
            var managerId = Request.Cookies["UserID"].Value.ToString();
            var pendingRequests = _taskCompletionRequestCollection.Find(r => r.ManagerId == managerId).ToList();

            Dictionary<string, Tasks> taskNames = new Dictionary<string, Tasks>();

            foreach (var request in pendingRequests)
            {
                var task = _tasksCollection.Find(t => t.Id == request.TaskId).FirstOrDefault();
                taskNames.Add(task.Id, task);
            }

            ViewBag.TaskNames = taskNames;

            Dictionary<string, UserSignUp> teamMemberNames = new Dictionary<string, UserSignUp>();

            foreach (var request in pendingRequests)
            {
                var user = _userCollection.Find(u => u.Id == request.DeveloperId).FirstOrDefault();
                if (user != null)
                {
                    if (!teamMemberNames.ContainsKey(user.Id))
                    {
                        teamMemberNames.Add(user.Id, user);
                    }
                }
            }

            ViewBag.TeamMemberNames = teamMemberNames;


            return View(pendingRequests);
        }

        public ActionResult MyRequests()
        {
            var myId = Request.Cookies["UserID"].Value.ToString();
            var pendingRequests = _taskCompletionRequestCollection.Find(r => r.DeveloperId == myId).ToList();

            Dictionary<string, Tasks> taskNames = new Dictionary<string, Tasks>();

            foreach (var request in pendingRequests)
            {
                var task = _tasksCollection.Find(t => t.Id == request.TaskId).FirstOrDefault();
                taskNames.Add(task.Id, task);
            }

            ViewBag.TaskNames = taskNames;

            Dictionary<string, UserSignUp> teamMemberNames = new Dictionary<string, UserSignUp>();

            foreach (var request in pendingRequests)
            {
                var user = _userCollection.Find(u => u.Id == request.DeveloperId).FirstOrDefault();
                if (user != null)
                {
                    if (!teamMemberNames.ContainsKey(user.Id))
                    {
                        teamMemberNames.Add(user.Id, user);
                    }
                }
            }

            ViewBag.TeamMemberNames = teamMemberNames;


            return View(pendingRequests);
        }

        public ActionResult RequestHandle(int func, string taskId, string requestId)
        {
            try
            {
                // Convert IDs to ObjectId
                var taskObjectId = new ObjectId(taskId);
                var requestObjectId = new ObjectId(requestId);

                var task = _tasksCollection.Find(t => t.Id == taskId).FirstOrDefault();
                var request = _taskCompletionRequestCollection.Find(r => r.RequestId == requestObjectId).FirstOrDefault();

                if (func == 1)
                {
                    if (task != null)
                    {
                        task.Status = "Done";
                        _tasksCollection.ReplaceOne(t => t.Id == taskId, task);

                    }
                    if (request != null)
                    {
                        request.Status = "Approved";
                        request.ReviewDate = DateTime.Now;
                        _taskCompletionRequestCollection.ReplaceOne(r => r.RequestId == requestObjectId, request);
                    }
                }

                if (func == 2)
                {
                    if (task != null)
                    {
                        task.Status = "In Progress";
                        _tasksCollection.ReplaceOne(t => t.Id == taskId, task);

                    }
                    if (request != null)
                    {
                        request.Status = "Declined";
                        request.ReviewDate = DateTime.Now;
                        _taskCompletionRequestCollection.ReplaceOne(r => r.RequestId == requestObjectId, request);
                    }
                }

                return RedirectToAction("TaskDetails", new { id = taskId });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error handling request: " + ex.Message;
                return View("Error");
            }
        }

        public ActionResult UserProfile()
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
        public ActionResult UpdateProfile(string UserId)
        {

            var user = _userProfilesCollection
                .Find(u => u.Id == UserId)
                .FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public ActionResult UpdateProfile(UserProfile model)
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

            return RedirectToAction("UserProfile");
        }

        public ActionResult AddComment(String id, int func, string content, string UserId, string UserName, string RelatedId)
        {

            Comment comment = new Comment
            {
                Content = content,
                UserId = UserId,
                UserName = UserName,
                RelatedId = RelatedId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            _commentCollection.InsertOne(comment);

            if (func == 1)
                return RedirectToAction("ProjectDetails", new { id = id });
            else if (func == 2)
                return RedirectToAction("TaskDetails", "Task", new { id = id });

            return RedirectToAction("ManagerDashboard");

        }

        public ActionResult DeleteComment(string id, string relatedId, int func)
        {
            try
            {
                _commentCollection.DeleteOne(c => c.Id == id);

                if (func == 1)
                    return RedirectToAction("TaskDetails", "Task", new { id = relatedId });
                else if (func == 2)
                    return RedirectToAction("ProjectDetails", new { id = relatedId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the comment.");
            }
            return View("ProjectDetails", new { id = relatedId });
        }

        public ActionResult ManagerDashboard()
        {
            return View();
        }

        public ActionResult DeteleProject(string id)
        {
            try
            {
                // Delete project from MongoDB
                _projectCollection.DeleteOne(p => p.Id == id);

                // Optionally, you can redirect to another action upon successful deletion
                return RedirectToAction("ViewProjects");
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, or show appropriate messages to the user
                ModelState.AddModelError("", "An error occurred while deleting the project.");
                // You might want to log the exception details
                // Logger.LogException(ex);
            }
            return View("ViewProjects");
        }
    }
}
