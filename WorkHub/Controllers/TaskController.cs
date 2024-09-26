using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkHub.Models;
using WorkHub.Services;
using System.IO;
using MongoDB.Driver.GridFS;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace WorkHub.Controllers
{
    public class TaskController : Controller
    {

        public readonly IMongoCollection<Tasks> _tasksCollection;
        public readonly IMongoCollection<CreateaProject> _projectsCollection;
        public readonly IMongoCollection<UserSignUp> _userCollection;
        public readonly IMongoCollection<Models.File> _taskFileCollection;
        public readonly IMongoCollection<TaskCompletionRequest> _taskCompletionRequestCollection;
        public readonly IMongoCollection<Comment> _commentCollection;
        public readonly IMongoCollection<UserProfile> _userProfilesCollection;
        public readonly IMongoCollection<Notification> _userNotificationsCollection;

        public TaskController()
        {
            _tasksCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");
            _projectsCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
            _userCollection = MongoDBHelper.GetCollection<UserSignUp>("Users");
            _taskFileCollection = MongoDBHelper.GetCollection<Models.File>("TaskFile");
            _taskCompletionRequestCollection = MongoDBHelper.GetCollection<TaskCompletionRequest>("TaskCompletionRequest");
            _commentCollection = MongoDBHelper.GetCollection<Comment>("Comment");
            _userProfilesCollection = MongoDBHelper.GetCollection<UserProfile>("UserProfiles");
            _userNotificationsCollection = MongoDBHelper.GetCollection<Notification>("Notifications");
        }

        public ActionResult TasksList()
        {
            try
            {
                var projects = _projectsCollection.Find(_ => true).ToList();

                ViewBag.Projects = projects;

                var tasks = _tasksCollection.Find(_ => true).ToList();
                var projectNames = new List<string>();

                for (int i = 0; i < tasks.Count; i++)
                {
                    var project = _projectsCollection.Find(p => p.Id == tasks[i].ProjectID).FirstOrDefault();
                    if (project != null)
                    {
                        projectNames.Add(project.ProjectName);
                    }
                    else
                    {
                        projectNames.Add(" ");
                    }
                }

                ViewBag.ProjectNames = projectNames;
                return View(tasks);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while fetching tasks.");
                return View(new List<Tasks>());
            }
        }

        public ActionResult TaskDetails(string id)
        {
            try
            {

                var task = _tasksCollection.Find(t => t.Id == id).FirstOrDefault();
                if (task == null)
                {
                    return HttpNotFound();
                }

                var users = new Dictionary<string, string>();

                if (task.Assignee != null)
                {
                    for (int i = 0; i < task.Assignee.Count; i++)
                    {
                        var user = _userCollection.Find(u => u.Id == task.Assignee[i]).FirstOrDefault();
                        if (user != null)
                        {
                            users.Add(user.Username, user.Role);
                        }
                    }
                }

                var project = _projectsCollection.Find(p => p.Id == task.ProjectID).FirstOrDefault();
                ViewBag.ProjectName = project.ProjectName;


                var teamMembers = project.TeamMembers;
                foreach (var user in teamMembers)
                {
                    UserSignUp memberinfo = _userCollection.Find(u => u.Id == user).FirstOrDefault();

                    if (memberinfo.Role == "manager")
                    {
                        ViewBag.ManagerId = memberinfo.Id;
                    }
                }

                ViewBag.Users = users;

                ObjectId taskObjectId;
                if (!ObjectId.TryParse(id, out taskObjectId))
                {
                    return HttpNotFound();
                }


                var taskFilesCollection = MongoDBHelper.GetCollection<Models.File>("TaskFiles");


                var taskFiles = taskFilesCollection.Find(file => file.TaskID == id).ToList();


                if (taskFiles == null || taskFiles.Count == 0)
                {

                    ViewBag.TaskFiles = new List<Models.File>();
                }
                else
                {
                    ViewBag.TaskFiles = taskFiles;
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


                return View((task, taskFiles, comments));
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while fetching project details.");



                return View(new CreateaProject());
            }
        }

        public ActionResult DownloadFile(string fileId)
        {
            try
            {
                var objectId = new ObjectId(fileId);

                var taskFilesCollection = MongoDBHelper.GetCollection<Models.File>("TaskFiles");
                var taskFile = taskFilesCollection.Find(f => f.Id == objectId).FirstOrDefault();

                if (taskFile == null)
                {
                    return HttpNotFound("File not found.");
                }

                var database = MongoDBHelper.GetDatabase();
                var bucket = new GridFSBucket(database);

                byte[] fileBytes;
                using (var stream = new MemoryStream())
                {
                    bucket.DownloadToStreamByName(taskFile.FilePath, stream);
                    fileBytes = stream.ToArray();
                }

                return File(fileBytes, taskFile.ContentType, taskFile.FileName);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error downloading file: " + ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult DeleteFiles(string taskid, string[] fileIds)
        {
            if (fileIds != null && fileIds.Length > 0)
            {
                var database = MongoDBHelper.GetDatabase();

                var taskFilesCollection = MongoDBHelper.GetCollection<Models.File>("TaskFiles");
                var taskFilesToDelete = taskFilesCollection.Find(f => fileIds.Contains(f.Id.ToString())).ToList();

                foreach (var taskFile in taskFilesToDelete)
                {
                    var filePath = taskFile.FilePath;

                    var filesCollection = database.GetCollection<BsonDocument>("fs.files");
                    var fileDocument = filesCollection.Find(Builders<BsonDocument>.Filter.Eq("filename", filePath)).FirstOrDefault();

                    if (fileDocument != null)
                    {
                        var fileId = fileDocument["_id"].AsObjectId;

                        var chunksCollection = database.GetCollection<BsonDocument>("fs.chunks");
                        chunksCollection.DeleteMany(Builders<BsonDocument>.Filter.Eq("files_id", fileId));

                        filesCollection.DeleteMany(Builders<BsonDocument>.Filter.Eq("_id", fileId));
                    }
                }

                var filter = Builders<Models.File>.Filter.In(f => f.Id, taskFilesToDelete.Select(tf => tf.Id));
                taskFilesCollection.DeleteMany(filter);
            }

            return RedirectToAction("TaskDetails", new { id = taskid });
        }

        public ActionResult AddTask(int act, string id, string projectID, string title, string desc, string repID, string repName, string dueDate, string AssignDate, string estTime, string status)
        {
            try
            {
                if (act == 1)
                {

                    var projects = _projectsCollection.Find(_ => true).ToList();


                    ViewBag.Projects = projects;
                    ViewBag.Name = Request.Cookies["UserName"].Value.ToString();
                    ViewBag.ID = Request.Cookies["UserID"].Value.ToString();
                    ViewBag.act = 1;


                    var users = _userCollection.Find(_ => true).ToList();


                    ViewBag.Users = users;


                    return View();
                }
                else if (act == 2)
                {

                    var projects = _projectsCollection.Find(_ => true).ToList();
                    Tasks model = _tasksCollection.Find(t => t.Id == id).FirstOrDefault();


                    ViewBag.Projects = projects;
                    ViewBag.Name = Request.Cookies["UserName"].Value.ToString();
                    ViewBag.ID = Request.Cookies["UserID"].Value.ToString();
                    ViewBag.act = 2;


                    var users = _userCollection.Find(_ => true).ToList();


                    ViewBag.Users = users;

                    //Tasks model = new Tasks { Id = id, TaskTitle = title, Description = desc, Reporter = repName, ReporterID = repID, DueDate = DateTime.Parse(dueDate), AssignDate = DateTime.Parse(AssignDate), Status = status, ProjectID = projectID, EstimationTime = Convert.ToDouble(estTime) };

                    // Return the view with the model (CreateaProject)
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while fetching users.");



                return View(new Tasks());
            }

            return View();
        }

        [HttpGet]
        public ActionResult TaskFilter(int func, string status, string project, string assignDateComparison, DateTime? assignDate, string dueDateComparison, DateTime? dueDate, string searchQuery)
        {
            List<Tasks> filteredTasks = new List<Tasks>();
            if (func == 1)
            {

                var taskCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");


                var filter = Builders<Tasks>.Filter.Empty;


                if (!string.IsNullOrEmpty(status))
                {
                    filter = filter & Builders<Tasks>.Filter.Eq(t => t.Status, status);
                }


                if (!string.IsNullOrEmpty(project) && project != "Choose...")
                {
                    filter = filter & Builders<Tasks>.Filter.Eq(t => t.ProjectID, project);
                }


                if (assignDate.HasValue)
                {
                    var startOfDay = assignDate.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                    switch (assignDateComparison)
                    {
                        case "=":
                            filter = filter & Builders<Tasks>.Filter.Gte(t => t.AssignDate, startOfDay) &
                                                Builders<Tasks>.Filter.Lte(t => t.AssignDate, endOfDay);
                            break;
                        case "<=":
                            filter = filter & Builders<Tasks>.Filter.Lte(t => t.AssignDate, endOfDay);
                            break;
                        case ">=":
                            filter = filter & Builders<Tasks>.Filter.Gte(t => t.AssignDate, startOfDay);
                            break;
                    }
                }




                if (dueDate.HasValue)
                {
                    var startOfDay = dueDate.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                    switch (dueDateComparison)
                    {
                        case "=":
                            filter = filter & Builders<Tasks>.Filter.Gte(t => t.DueDate, startOfDay) &
                                                Builders<Tasks>.Filter.Lte(t => t.DueDate, endOfDay);
                            break;
                        case "<=":
                            filter = filter & Builders<Tasks>.Filter.Lte(t => t.DueDate, endOfDay);
                            break;
                        case ">=":
                            filter = filter & Builders<Tasks>.Filter.Gte(t => t.DueDate, startOfDay);
                            break;
                    }
                }



                filteredTasks = taskCollection.Find(filter).ToList();



            }
            else if (func == 2 && !string.IsNullOrEmpty(searchQuery))
            {
                var taskCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");


                var titleFilter = Builders<Tasks>.Filter.Regex(t => t.TaskTitle, new BsonRegularExpression(searchQuery, "i"));


                var reporterFilter = Builders<Tasks>.Filter.Regex(t => t.Reporter, new BsonRegularExpression(searchQuery, "i"));


                var titleMatches = taskCollection.Find(titleFilter).ToList();


                var reporterMatches = taskCollection.Find(reporterFilter).ToList();


                filteredTasks.AddRange(titleMatches);


                foreach (var task in reporterMatches)
                {
                    if (!filteredTasks.Any(t => t.Id == task.Id))
                    {
                        filteredTasks.Add(task);
                    }
                }
            }
            else if (func == 3)
            {
                string userId = string.Empty;

                if (Request.Cookies["UserID"] != null)
                {
                    userId = Request.Cookies["UserID"].Value;
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    var taskCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");

                    var filter = Builders<Tasks>.Filter.AnyEq(t => t.Assignee, userId);

                    filteredTasks = taskCollection.Find(filter).ToList();
                }
            }



            var projects = _projectsCollection.Find(_ => true).ToList();

            ViewBag.Projects = projects;

            var projectNames = new List<string>();

            for (int i = 0; i < filteredTasks.Count; i++)
            {
                var projectCollection = _projectsCollection.Find(p => p.Id == filteredTasks[i].ProjectID).FirstOrDefault();
                if (projectCollection != null)
                {
                    projectNames.Add(projectCollection.ProjectName);
                }
                else
                {
                    projectNames.Add(" ");
                }
            }

            ViewBag.ProjectNames = projectNames;




            return View("TasksList", filteredTasks);
        }

        [HttpPost]
        public ActionResult Create(int func, string id, Tasks model, HttpPostedFileBase[] attachments)
        {


            try
            {
                if (func == 1)
                {
                    model.AssignDate = DateTime.Now;
                    if (model.DueDate == null)
                    {
                        model.DueDate = DateTime.Now;
                    }

                    _tasksCollection.InsertOne(model);

                    if (attachments != null && attachments.Length > 0)
                    {
                        UploadFiles(model, attachments);
                    }

                    foreach (var assigneeId in model.Assignee)
                    {
                        var notification = new Notification
                        {
                            UserId = assigneeId,
                            Message = $"New task assigned to you: {model.TaskTitle}. ID: {model.Id}",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        _userNotificationsCollection.InsertOne(notification);
                    }

                    return RedirectToAction("TasksList");
                }

                else if (func == 2)
                {
                    var existingTask = _tasksCollection.Find(t => t.Id == model.Id).FirstOrDefault();
                    if (existingTask == null)
                    {
                        ModelState.AddModelError("", "Project not found.");
                    }
                    else
                    {
                        model.DueDate = model.DueDate.AddDays(1);
                        
                        if (existingTask.Assignee == null)
                        {
                            existingTask.Assignee = new List<string>();
                        }

                        
                        if (model.Assignee != null && model.Assignee.Any())
                        {
                            
                            var newAssignees = model.Assignee.Except(existingTask.Assignee).ToList();

                            
                            if (newAssignees.Any())
                            {
                                foreach (var assigneeId in newAssignees)
                                {
                                    var notification = new Notification
                                    {
                                        UserId = assigneeId,
                                        Message = $"New task assigned to you: {model.TaskTitle}",
                                        CreatedAt = DateTime.UtcNow,
                                        IsRead = false
                                    };

                                    _userNotificationsCollection.InsertOne(notification);
                                }
                            }
                        }

                        
                        UpdateExistingTask(existingTask, model);

                        
                        _tasksCollection.ReplaceOne(p => p.Id == existingTask.Id, existingTask);

                        
                        if (attachments != null && attachments.Length > 0)
                        {
                            UploadFiles(model, attachments);
                        }

                        
                        return RedirectToAction("TasksList");

                    }
                }

                else if (func == 3)
                {
                    var existingTask = _tasksCollection.Find(t => t.Id == id).FirstOrDefault();
                    if (attachments != null && attachments.Length > 0)
                    {
                        UploadFiles(existingTask, attachments);
                    }

                    return RedirectToAction("TaskDetails", new { id = id });
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while saving the project.");

            }


            return View(model);
        }

        private void UploadFiles(Tasks model, HttpPostedFileBase[] attachments)
        {
            var database = MongoDBHelper.GetDatabase();
            var bucket = new GridFSBucket(database);

            model.Attachments = new List<string>();

            foreach (var file in attachments)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileId = ObjectId.GenerateNewId();
                    var fileName = $"{fileId}{Path.GetExtension(file.FileName)}";

                    using (var stream = file.InputStream)
                    {
                        bucket.UploadFromStream(fileName, stream);
                    }

                    var taskFile = new Models.File
                    {
                        Id = fileId, 
                        TaskID = model.Id.ToString(), 
                        FileName = file.FileName,
                        FilePath = fileName, 
                        ContentType = file.ContentType,
                        Size = file.ContentLength,
                        UploadedDate = DateTime.Now
                    };

                    var taskFilesCollection = MongoDBHelper.GetCollection<Models.File>("TaskFiles");
                    taskFilesCollection.InsertOne(taskFile);

                    model.Attachments.Add(fileId.ToString());
                }
            }
        }

        public void UpdateExistingTask(Tasks existingTask, Tasks model)
        {
            if (model.TaskTitle != null)
                existingTask.TaskTitle = model.TaskTitle;
            if (model.Description != null)
                existingTask.Description = model.Description;
            if (model.Status != null)
                existingTask.Status = model.Status;
            if (model.Reporter != null)
                existingTask.Reporter = model.Reporter;
            if (model.ReporterID != null)
                existingTask.ReporterID = model.ReporterID;
            if (model.DueDate != Convert.ToDateTime("0001-01-01"))
                existingTask.DueDate = model.DueDate;
            if (model.Attachments != null)
                existingTask.Attachments = model.Attachments;
            if (model.EstimationTime != 0)
                existingTask.EstimationTime = model.EstimationTime;
            if (model.ProjectID != null)
                existingTask.ProjectID = model.ProjectID;
            if (model.Assignee != null)
                existingTask.Assignee = model.Assignee;
        }

        [HttpPost]
        public ActionResult MakeRequest(string taskId, string developerId, string managerId, string comment)
        {
            var collection = MongoDBHelper.GetCollection<Task>("Tasks");
            var objectId = new ObjectId(taskId);

            var filter = Builders<Task>.Filter.Eq("_id", objectId);
            var update = Builders<Task>.Update.Set("Status", "Pending Approval");

            collection.UpdateOne(filter, update);

            TaskCompletionRequest request = new TaskCompletionRequest
            {
                TaskId = taskId,
                DeveloperId = developerId,
                ManagerId = managerId,
                RequestDate = DateTime.Now,
                Comment = comment,
                Status = "Pending"
            };

            _taskCompletionRequestCollection.InsertOne(request);

            return RedirectToAction("TaskDetails", new { id = taskId });
        }

        [HttpPost]
        public ActionResult CreateFromKanban(KanbanCard newCard, string ProjectId)
        {
            try
            {
                string status = newCard.ColumnNo;

                if (status == "0") status = "To Do";
                else if (status == "1") status = "In Progress";
                else if (status == "2") status = "Done";

                Tasks task = new Tasks
                {
                    TaskTitle = newCard.Header,
                    Description = newCard.Body,
                    Status = status,
                    Reporter = Request.Cookies["UserName"].Value,
                    ReporterID = Request.Cookies["UserID"].Value,
                    AssignDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Attachments = null,
                    EstimationTime = 0,
                    ProjectID = ProjectId,
                };

                _tasksCollection.InsertOne(task);

                return RedirectToAction("Index", "Kanban", new { id = ProjectId });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding task: " + ex.Message;
                return View("Index", "Kanban");
            }
        }


    }
}