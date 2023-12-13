#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeMate.Models;

namespace TimeMate.ViewModels
{
    public class InvitationRequestsViewModel
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string status { get; set; }
        public int InvitationId { get; set; }
    }
}
