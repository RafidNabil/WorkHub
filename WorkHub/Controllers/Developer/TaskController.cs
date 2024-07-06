using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkHub.Controllers.Developer
{
    public class TaskController : Controller
    {
        // GET: Task
        public ActionResult Index()
        {
            return View();
        }
    }
}


/*using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Web.Mvc;
using WorkHub.Models;
using WorkHub.Services;

namespace WorkHub.Controllers
{
    public class TasksController : Controller
    {
        private IMongoCollection<Task> _taskCollection;

        public TasksController()
        {
            _taskCollection = MongoDBHelper.GetCollection<Task>("Tasks");
        }

        // GET: Tasks
        public ActionResult Index(string search = null, string filterStatus = null)
        {
            var filter = Builders<Task>.Filter.Empty;

            if (!string.IsNullOrEmpty(search))
            {
                filter &= Builders<Task>.Filter.Regex("TaskName", new BsonRegularExpression(search, "i"));
            }

            if (!string.IsNullOrEmpty(filterStatus))
            {
                filter &= Builders<Task>.Filter.Eq("Status", filterStatus);
            }

            var tasks = _taskCollection.Find(filter).ToList();
            return View(tasks);
        }

        // GET: Tasks/Details/5
        public ActionResult Details(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return HttpNotFound();
            }

            var task = _taskCollection.Find(t => t.Id == objectId).FirstOrDefault();
            if (task == null)
            {
                return HttpNotFound();
            }
            return View(task);
        }

        // GET: Tasks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Task task)
        {
            if (ModelState.IsValid)
            {
                _taskCollection.InsertOne(task);
                return RedirectToAction("Index");
            }
            return View(task);
        }

        // GET: Tasks/Edit/5
        public ActionResult Edit(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return HttpNotFound();
            }

            var task = _taskCollection.Find(t => t.Id == objectId).FirstOrDefault();
            if (task == null)
            {
                return HttpNotFound();
            }
            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, Task taskIn)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                var filter = Builders<Task>.Filter.Eq(t => t.Id, objectId);
                _taskCollection.ReplaceOne(filter, taskIn);
                return RedirectToAction("Index");
            }
            return View(taskIn);
        }

        // GET: Tasks/Delete/5
        public ActionResult Delete(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return HttpNotFound();
            }

            var task = _taskCollection.Find(t => t.Id == objectId).FirstOrDefault();
            if (task == null)
            {
                return HttpNotFound();
            }
            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return HttpNotFound();
            }

            var filter = Builders<Task>.Filter.Eq(t => t.Id, objectId);
            _taskCollection.DeleteOne(filter);
            return RedirectToAction("Index");
        }
    }
}

*/
