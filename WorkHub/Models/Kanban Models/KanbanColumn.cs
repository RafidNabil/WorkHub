using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class KanbanColumn
    {
        public string Title { get; set; }
        public List<Tasks> Cards { get; set; }
    }
}