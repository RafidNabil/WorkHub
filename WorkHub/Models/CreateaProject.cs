using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class CreateaProject
    {
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public List<int> TeamMembers { get; set; }
    }
}