using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkHub.Models;

namespace WorkHub.Controllers
{
    public class ManagerController : Controller
    {
        // GET: Manager
        public ActionResult ManagerIndex()
        {
            return View();
        }

        // GET: Manager/CreateProject
        public ActionResult CreateProject()
        {
            return View();
        }

        // POST: Manager/CreateProject
        [HttpPost]
        public ActionResult CreateProject(CreateaProject model)
        {
            if (ModelState.IsValid)
            {
                // Save the project to the database (code to save the project goes here)
                return RedirectToAction("ManagerIndex");
            }

            return View(model);
        }

        public ActionResult ManagerDashboard()
        {
            return View();
        }

    }
}