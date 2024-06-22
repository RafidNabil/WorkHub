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
    public class CommentController : Controller
    {
        private IMongoCollection<Comments> _commentCollection;

        public CommentController()
        {
            _commentCollection = MongoDBHelper.GetCollection<Comments>("comments");
        }

        public ActionResult Index()
        {
            var comments = _commentCollection.Find(FilterDefinition<Comments>.Empty).Limit(10).ToList();
            return View(comments);
        }
    }
}