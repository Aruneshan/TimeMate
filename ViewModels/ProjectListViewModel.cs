#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeMate.Models;

namespace TimeMate.ViewModels
{
    public class ProjectListViewModel
    {
        public List<Project> ProjectsList { get; set; }
        public string SearchString { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalProjects { get; set; }
    }
}
