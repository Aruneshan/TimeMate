﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMate.ViewModels
{
    public class WorkspaceViewModel
    {
        [Required]
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
