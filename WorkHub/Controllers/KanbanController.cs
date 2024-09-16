using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WorkHub.Models;
using WorkHub.Services;

namespace WorkHub.Controllers
{
    public class KanbanController : Controller
    {
        private readonly IMongoCollection<Tasks> _tasksCollection;
        private readonly IMongoCollection<CreateaProject> _projectsCollection;
        private readonly IMongoCollection<File> _taskFileCollection;
        private readonly IMongoCollection<Task> _taskCollection;
        private readonly IMongoCollection<TaskCompletionRequest> _taskCompletionRequestCollection;
        public readonly IMongoCollection<Comment> _commentCollection;

        public KanbanController()
        {
            _tasksCollection = MongoDBHelper.GetCollection<Tasks>("Tasks");
            _projectsCollection = MongoDBHelper.GetCollection<CreateaProject>("Project");
            _taskFileCollection = MongoDBHelper.GetCollection<File>("TaskFiles");
            _taskCollection = MongoDBHelper.GetCollection<Task>("Tasks");
            _taskCompletionRequestCollection = MongoDBHelper.GetCollection<TaskCompletionRequest>("TaskCompletionRequest");
            _commentCollection = MongoDBHelper.GetCollection<Comment>("Comment");
        }

        public ActionResult Index(string id)
        {
            List<CreateaProject> projects = _projectsCollection.Find(_ => true).ToList();
            ViewBag.Projects = projects;


            CreateaProject selectedProject = null;
            if (string.IsNullOrEmpty(id))
            {
                selectedProject = projects.FirstOrDefault();
            }
            else
            {
                selectedProject = projects.FirstOrDefault(p => p.Id == id);
            }

            Dictionary<string, long> taskFileCounts = new Dictionary<string, long>();
            Dictionary<string, long> taskCommentCounts = new Dictionary<string, long>();

            if (selectedProject != null)
            {
                ViewBag.SelectedProject = selectedProject;
                var KanbanColumns = new List<KanbanColumn>
                {
                    new KanbanColumn { Title = "To Do", Cards = GetCardsByColumnStat("To Do", selectedProject.Id) },
                    new KanbanColumn { Title = "In Progress", Cards = GetCardsByColumnStat("In Progress", selectedProject.Id) },
                    new KanbanColumn { Title = "Done", Cards = GetCardsByColumnStat("Done", selectedProject.Id) }
                };

                ;
                var projectTasks = _tasksCollection.Find(t => t.ProjectID == selectedProject.Id).ToList();

                // Iterate over each task and count the associated files
                foreach (var task in projectTasks)
                {
                    var fileCount = _taskFileCollection.CountDocuments(tf => tf.TaskID == task.Id);
                    var commentCount = _commentCollection.CountDocuments(c => c.RelatedId == task.Id);
                    taskFileCounts[task.Id] = fileCount;
                    taskCommentCounts[task.Id] = commentCount;
                }

                ViewBag.TaskFileCounts = taskFileCounts;
                ViewBag.TaskCommentCounts = taskCommentCounts;

                return View(KanbanColumns);
            }
            else
            {
                return RedirectToAction("Index"); // Redirect if no project is found
            }
        }

        [HttpPost]
        public ActionResult DeleteTask(string taskId, string projectId)
        {
            try
            {
                DeleteFiles(taskId);

                var filter = Builders<Tasks>.Filter.Eq(card => card.Id, taskId);
                _tasksCollection.DeleteOne(filter);

                var filter2 = Builders<TaskCompletionRequest>.Filter.Eq(tr => tr.TaskId, taskId);
                _taskCompletionRequestCollection.DeleteMany(filter2);

                return RedirectToAction("Index", new { id = projectId });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error deleting task: " + ex.Message;
                return View("Index");
            }
        }


        private void DeleteFiles(string taskid)
        {
            var database = MongoDBHelper.GetDatabase();
            // Step 1: Get the collection of TaskFiles
            var taskFilesCollection = MongoDBHelper.GetCollection<Models.File>("TaskFiles");

            // Step 2: Find all TaskFiles where TaskID == taskid
            var taskFilesToDelete = taskFilesCollection.Find(tf => tf.TaskID == taskid).ToList();

            foreach (var taskFile in taskFilesToDelete)
            {
                // Get the file path which is used as the filename in fs.files
                var filePath = taskFile.FilePath;

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
            var filter = Builders<Models.File>.Filter.In(f => f.Id, taskFilesToDelete.Select(tf => tf.Id));
            taskFilesCollection.DeleteMany(filter);
        }


        [HttpPost]
        public ActionResult UpdateCard(string id, string newStatus, string selectedProject)
        {
            try
            {
                var filter = Builders<Tasks>.Filter.Eq(card => card.Id, id);
                var update = Builders<Tasks>.Update.Set(card => card.Status, newStatus);

                _tasksCollection.UpdateOne(filter, update);

                return RedirectToAction("Index", new { id = ViewBag.SelectedProject.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating task: " + ex.Message;
                return RedirectToAction("Index", new { id = selectedProject });
            }
        }

        private List<Tasks> GetCardsByColumnStat(string status, string projectId)
        {
            var filter = Builders<Tasks>.Filter.And(
                Builders<Tasks>.Filter.Eq(card => card.Status, status),
                Builders<Tasks>.Filter.Eq(card => card.ProjectID, projectId)
            );
            return _tasksCollection.Find(filter).ToList();
        }
    }
}
