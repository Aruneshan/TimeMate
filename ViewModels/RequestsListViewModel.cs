#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMate.ViewModels
{
    public class RequestsListViewModel
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string status { get; set; }
        public string Member { get; set; }
        public int projectId { get; set; }
        public int RequestId { get; set; }
        public bool CanChange { get; set; }
    }
}
