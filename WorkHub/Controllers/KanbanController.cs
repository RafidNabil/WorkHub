﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkHub.Controllers
{
    public class KanbanController : Controller
    {
        // GET: Kanban
        public ActionResult Index()
        {
            return View();
        }
    }
}