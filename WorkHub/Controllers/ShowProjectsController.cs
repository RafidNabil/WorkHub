using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkHub.Controllers
{
    public class ShowProjectsController : Controller
    {
        // GET: ShowProjects
        public ActionResult Index()
        {
            return View();
        }
    }
}