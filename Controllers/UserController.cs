using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;
using TimeMate.ViewModels;
#nullable disable

namespace TimeMate.Controllers
{
    [Log]

    public class UserController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private TimeMateContext _context;
        public UserController(TimeMateContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Partial View
        //SharedView

        //Admin Dashboard
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        //Project Manager Profile
        [Authorize(Roles = "Manager")]
        public IActionResult ProjectMangerProfile(string Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            if (_context.TimeMateUsers.SingleOrDefault(x => x.Email == Id) == null)
            {
                return NotFound();
            }
            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            ViewBag.Name = WorkArea.Name;
            ViewBag.WorkspaceId = WorkArea.WorkspaceId;
            var user = _context.TimeMateUsers.SingleOrDefault(x => x.Email == Id);
            var profile = new ProfileViewModel();
            profile.FullName = user.FirstName + ' ' + user.LastName;
            profile.Email = user.Email;
            return View(profile);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminProfile(string Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            if (_context.TimeMateUsers.SingleOrDefault(x => x.Email == Id) == null)
            {
                return NotFound();
            }
            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            ViewBag.Name = WorkArea.Name;
            ViewBag.WorkspaceId = WorkArea.WorkspaceId;
            var user = _context.TimeMateUsers.SingleOrDefault(x => x.Email == Id);
            var profile = new ProfileViewModel();
            profile.FullName = user.FirstName + ' ' + user.LastName;
            profile.Email = user.Email;

            return View(profile);
        }

        [Authorize(Roles = "Employee")]
        public IActionResult EmployeeProfile(string Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            if (_context.TimeMateUsers.SingleOrDefault(x => x.Email == Id) == null)
            {
                return NotFound();
            }
            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            var user = _context.TimeMateUsers.SingleOrDefault(x => x.Email == Id);
            var profile = new ProfileViewModel();
            profile.FullName = user.FirstName + ' ' + user.LastName;
            profile.Email = user.Email;

            return View(profile);
        }

        //Create workspace
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult CreateWorkspace()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateWorkspace(WorkspaceViewModel model)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                if (_context.Workspaces.Any(x => x.PmId == userid))
                {
                    var ws = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
                    ViewBag.Name = ws.Name;
                    ViewBag.WorkspaceId = ws.WorkspaceId;
                    ModelState.AddModelError("CustomError", "You have already defined Your Workspace");
                    return View(model);
                }

                var workspace = new WorkSpace();
                workspace.Name = model.Name;
                workspace.PmId = userid;
                workspace.DateCreated = DateTime.Now.Date;
                _context.Workspaces.Add(workspace);
                _context.SaveChanges();
                return RedirectToAction("Workspace");
            }

            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            ViewBag.Name = WorkArea.Name;
            ViewBag.WorkspaceId = WorkArea.WorkspaceId;
            return View(model);
        }

        [Authorize(Roles = "Manager")]
        //Workspace
        public IActionResult Workspace()
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            var workspace_view_model = new WorkspaceListViewModel();
            var countProject = _context.Projects.Where(x => x.WorkspaceId == workspace.WorkspaceId).Count();
            ViewBag.ProjectCount = countProject;
            // Count employee added
            var countEmployee = 0;
            var projects_list = _context.Projects.Where(x => x.WorkspaceId == workspace.WorkspaceId).ToList();
            var project_assignment_list = _context.ProjectAssignment.Where(x => x.status == "Accepted").ToList();
            foreach (var Project in projects_list)
            {
                countEmployee += project_assignment_list.Count(b => b.projectId == Project.projectId);
            }
            ViewBag.EmployeeCount = countEmployee;

            // Count employee pending
            var countEmployeePending = 0;
            var project_assignment_list_pending = _context.ProjectAssignment.Where(x => x.status == "Pending").ToList();
            foreach (var Project in projects_list)
            {
                countEmployeePending += project_assignment_list_pending.Count(b => b.projectId == Project.projectId);
            }
            ViewBag.EmployeeCountPending = countEmployeePending;
            //Productiivty


            var project_list = _context.Projects.Where(x => x.WorkspaceId == workspace.WorkspaceId).ToList();
            var EmployeeIds = new List<EmployeeIdViewModel>();
            var DifferentEmployeeInTimesheet = _context.TimeSheets.GroupBy(t => t.EmployeeId).Select(g => g.First()).ToList();
            var productivityList = new List<ProductivityViewModel>();
            foreach (var a in DifferentEmployeeInTimesheet)
            {
                var countHours = 0;
                var productivity = new ProductivityViewModel();

                foreach (var Project in project_list)
                {
                    var totalTimesheet = _context.TimeSheets.Where(x => x.status == "Accepetd" && x.projectId == Project.projectId).ToList();
                    for (var i = 0; i < totalTimesheet.Count(); i++)
                    {
                        if (a.EmployeeId == totalTimesheet[i].EmployeeId)
                        {
                            productivity.EmployeeName = _context.TimeMateUsers.SingleOrDefault(x => x.Id == a.EmployeeId).UserName;
                            countHours += totalTimesheet[i].HoursSpent;
                        }
                    }
                }
                productivity.WorkingHours = countHours;
                productivityList.Add(productivity);

            }

            //
            if (workspace != null)
            {
                workspace_view_model.WorkspaceName = workspace.Name;
                workspace_view_model.Username = _context.TimeMateUsers.SingleOrDefault(x => x.Id == userid).UserName;
                workspace_view_model.DateCreated = workspace.DateCreated.Date;
                workspace_view_model.WorkspaceId = workspace.WorkspaceId;
                //AddProductivityListInModel
                var productivity_list = new List<ProductivityViewModel>();
                foreach (var a in productivityList)
                {
                    if (a.WorkingHours != 0 && a.EmployeeName != null)
                    {
                        productivity_list.Add(a);
                    }
                }
                workspace_view_model.ProductivityList = productivity_list;
            }
            return View(workspace_view_model);
        }

        [Authorize(Roles = "Admin")]
        //Workspace
        public IActionResult AdminWorkSpace()
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            var workspace_view_model = new WorkspaceListViewModel();
            var countProject = _context.Projects.Where(x => x.WorkspaceId == workspace.WorkspaceId).Count();
            ViewBag.ProjectCount = countProject;
            //count eemployee added
            var countEmployee = 0;
            var projects_list = _context.Projects.Where(x => x.WorkspaceId == workspace.WorkspaceId).ToList();
            var project_assignment_list = _context.ProjectAssignment.Where(x => x.status == "Accepted").ToList();
            foreach (var a in projects_list)
            {
                foreach (var b in project_assignment_list)
                {
                    if (a.projectId == b.projectId)
                    {
                        countEmployee += countEmployee + 1;
                    }
                }
            }
            ViewBag.EmployeeCount = countEmployee;
            //count employee pending
            var countEmployeePending = 0;
            var project_assignment_list_pending = _context.ProjectAssignment.Where(x => x.status == "Pending").ToList();
            foreach (var a in projects_list)
            {
                foreach (var b in project_assignment_list_pending)
                {
                    if (a.projectId == b.projectId)
                    {
                        countEmployeePending += countEmployeePending + 1;
                    }
                }
            }
            ViewBag.EmployeeCountPending = countEmployeePending;
            //Productiivty
            var project_list = _context.Projects.Where(x => x.WorkspaceId == workspace.WorkspaceId).ToList();
            var EmployeeIds = new List<EmployeeIdViewModel>();
            var DifferentEmployeeInTimesheet = _context.TimeSheets.GroupBy(t => t.EmployeeId).Select(g => g.First()).ToList();
            var productivityList = new List<ProductivityViewModel>();
            foreach (var a in DifferentEmployeeInTimesheet)
            {
                var countHours = 0;
                var productivity = new ProductivityViewModel();

                foreach (var Project in project_list)
                {
                    var totalTimesheet = _context.TimeSheets.Where(x => x.status == "Accepetd" && x.projectId == Project.projectId).ToList();
                    for (var i = 0; i < totalTimesheet.Count(); i++)
                    {
                        if (a.EmployeeId == totalTimesheet[i].EmployeeId)
                        {
                            productivity.EmployeeName = _context.TimeMateUsers.SingleOrDefault(x => x.Id == a.EmployeeId).UserName;
                            countHours += totalTimesheet[i].HoursSpent;
                        }
                    }
                }
                productivity.WorkingHours = countHours;
                productivityList.Add(productivity);

            }

            //
            if (workspace != null)
            {
                workspace_view_model.WorkspaceName = workspace.Name;
                workspace_view_model.Username = _context.TimeMateUsers.SingleOrDefault(x => x.Id == userid).UserName;
                workspace_view_model.DateCreated = workspace.DateCreated.Date;
                workspace_view_model.WorkspaceId = workspace.WorkspaceId;
                //AddProductivityListInModel
                var productivity_list = new List<ProductivityViewModel>();
                foreach (var a in productivityList)
                {
                    if (a.WorkingHours != 0 && a.EmployeeName != null)
                    {
                        productivity_list.Add(a);
                    }
                }
                workspace_view_model.ProductivityList = productivity_list;
            }
            return View(workspace_view_model);
        }

        [Authorize(Roles = "Manager")]
        //Create Project/WorkspaceId
        public IActionResult CreateProject(int Id)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ProjectViewModel model = new ProjectViewModel();
            var shareddata = new SharedData();
            shareddata.WorkspaceName = workspace.Name;
            shareddata.WorkspaceId = workspace.WorkspaceId;
            model.SharedData = shareddata;
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateProject(ProjectViewModel model, int Id)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid && x.WorkspaceId == Id);

            if (ModelState.IsValid)
            {
                var Project = new Project();
                Project.Name = model.Name;
                Project.description = model.description;
                Project.DateCreated = DateTime.Now.Date;
                Project.WorkspaceId = workspace.WorkspaceId;

                _context.Projects.Add(Project);
                _context.SaveChanges();
                return RedirectToAction("Projects", new { Id = Id });
            }
            ViewBag.Name = workspace.Name;
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            return View(model);
        }

        [Authorize(Roles = "Manager,Admin")]
        //Projects/WorkspaceId
        public IActionResult Projects(string searchString, int Id, int page = 1, int pageSize = 5)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);

            List<Project> ProjectList = new List<Project>();

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredProjects = _context.Projects
                    .Where(s => s.Name.Contains(searchString) && s.WorkspaceId == workspace.WorkspaceId)
                    .ToList();

                ProjectList.AddRange(filteredProjects);
            }
            else
            {
                var allProjects = _context.Projects
                    .Where(s => s.WorkspaceId == workspace.WorkspaceId)
                    .ToList();

                ProjectList.AddRange(allProjects);
            }

            // Calculate total pages for pagination
            int totalProjects = ProjectList.Count();
            int totalPages = (int)Math.Ceiling((double)totalProjects / pageSize);

            // Paginate the results
            var paginatedProjects = ProjectList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var project_list = new ProjectListViewModel
            {
                ProjectsList = paginatedProjects
            };

            ViewBag.Name = workspace.Name;
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString; // Add searchString to ViewBag for the pagination links

            return View(project_list);
        }

        // [Authorize(Roles = "Manager,Admin")]
        // public IActionResult Projects(string searchString, int Id)
        // {
        //     var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);

        //     List<Project> ProjectList = new List<Project>();

        //     if (!string.IsNullOrEmpty(searchString))
        //     {
        //         // Filter projects based on both name and workspace
        //         ProjectList = _context.Projects
        //             .Where(p => p.Name.Contains(searchString) && p.WorkspaceId == workspace.WorkspaceId)
        //             .ToList();
        //     }
        //     else
        //     {
        //         // Get all projects for the workspace
        //         ProjectList = _context.Projects
        //             .Where(p => p.WorkspaceId == workspace.WorkspaceId)
        //             .ToList();
        //     }
        //     //Console.WriteLine($"Search String: {searchString}");
        //     // to Check filter works correctly
        //     var totalProjects = projectQuery.Count();
        //     var projectsList = projectQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        //     var project_list = new ProjectListViewModel
        //     {
        //         ProjectsList = ProjectList
        //     };

        //     ViewBag.Name = workspace.Name;
        //     ViewBag.WorkspaceId = workspace.WorkspaceId;

        //     var viewModel = new ProjectListViewModel
        //     {
        //         projectsList = projectsList,
        //         PageIndex = pageIndex,
        //         PageSize = pageSize,
        //         TotalProjects = totalProjects
        //     };


        //     return View(project_list);
        // }

        //Add Project Members
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult AddProjectMember(int Id)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.Name = workspace.Name;
            ViewBag.WorkspaceId = workspace.WorkspaceId;

            ViewBag.projectId = Id;
            return View();
        }

        [HttpPost]
        public IActionResult AddProjectMember(int Id, ProjectAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);

                if (_context.TimeMateUsers.SingleOrDefault(x => x.Email == model.searchEmail) == null)
                {
                    var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);

                    ViewBag.Name = workspace.Name;
                    ViewBag.WorkspaceId = workspace.WorkspaceId;

                    ModelState.AddModelError("CustomError", "User does not exist");

                    return View(model);
                }

                var individual = _context.TimeMateUsers.SingleOrDefault(x => x.Email == model.searchEmail);

                if (_context.ProjectAssignment.Any(x => x.projectId == Project.projectId && x.EmployeeId == individual.Id))
                {
                    var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);

                    ViewBag.Name = workspace.Name;
                    ViewBag.WorkspaceId = workspace.WorkspaceId;

                    ModelState.AddModelError("CustomError", "User with this email is already member of this Project");

                    return View(model);
                }
                //Check if user is ProjectManager
                var employee = _context.TimeMateUsers.SingleOrDefault(x => x.Email == model.searchEmail);
                var roleId = _context.UserRoles.SingleOrDefault(x => x.UserId == employee.Id).RoleId;
                var role = _context.Roles.SingleOrDefault(x => x.Id == roleId).Name;

                if (role != "employee")
                {
                    var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
                    ViewBag.Name = workspace.Name;
                    ViewBag.WorkspaceId = workspace.WorkspaceId;
                    ModelState.AddModelError("CustomError", "User with this email does not exist");
                    return View(model);
                }
                if (individual != null && Project != null)
                {
                    var Project_Assignment = new ProjectAssignment();
                    Project_Assignment.EmployeeId = individual.Id;
                    Project_Assignment.projectId = Project.projectId;
                    Project_Assignment.status = "Pending";
                    _context.ProjectAssignment.Add(Project_Assignment);
                    _context.SaveChanges();
                    return RedirectToAction("ProjectMembers", new { Id = Id });
                }
                else
                {

                }
            }
            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            ViewBag.Name = WorkArea.Name;
            ViewBag.WorkspaceId = WorkArea.WorkspaceId;
            return View(model);
        }


        [Authorize(Roles = "Manager,Admin")]
        //Members List
        public IActionResult ProjectMembers(int Id)
        {
            var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);
            var project_members = _context.ProjectAssignment.Where(x => x.projectId == Id).ToList();

            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.Name = workspace.Name;
            ViewBag.WorkspaceId = workspace.WorkspaceId;

            var project_members_viewmodel_list = new List<MembersList>();
            if (project_members != null && Project != null)
            {
                foreach (var a in project_members)
                {
                    var project_members_viewmodel = new MembersList();
                    project_members_viewmodel.MemberEmail = _context.TimeMateUsers.SingleOrDefault(x => x.Id == a.EmployeeId).Email;
                    project_members_viewmodel.ProjectAssignmentId = a.ProjectAssignmentId;
                    project_members_viewmodel.IndividualUserId = a.EmployeeId;
                    project_members_viewmodel.projectId = a.projectId;
                    project_members_viewmodel_list.Add(project_members_viewmodel);
                }
            }
            return View(project_members_viewmodel_list);
        }

        //RemoveProjectMember/ProjectAssignmentId
        public IActionResult RemoveProjectMember(int Id)
        {
            var project_assigned = _context.ProjectAssignment.SingleOrDefault(x => x.ProjectAssignmentId == Id);
            if (project_assigned != null)
            {
                _context.ProjectAssignment.Remove(project_assigned);
                _context.SaveChanges();
                return RedirectToAction("ProjectMembers", new { Id = Id });
            }
            return RedirectToAction("ProjectMembers", new { Id = Id });
        }

        //Create Task for Workspace
        //CreateTask/WorkspaceId
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult CreateTask(int Id)
        {
            ViewBag.WorkspaceId = Id;
            ViewBag.Name = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Id).Name;
            return View();
        }

        [HttpPost]
        public IActionResult CreateTask(int Id, CreateTaskViewModel model)
        {

            if (ModelState.IsValid)
            {
                var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var workspace = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Id && x.PmId == userid);
                if (workspace != null)
                {
                    if (_context.Tasks.Any(x => x.TaskName == model.TaskName))
                    {
                        ViewBag.Name = workspace.Name;
                        ViewBag.WorkspaceId = workspace.WorkspaceId;
                        ModelState.AddModelError("CustomError", "Task with this name already exist.");
                        return View(model);
                    }
                }
                if (workspace != null)
                {
                    TimeMate.Models.Tasks task = new TimeMate.Models.Tasks();
                    task.TaskName = model.TaskName;
                    task.WorkspaceId = workspace.WorkspaceId;
                    _context.Tasks.Add(task);
                    _context.SaveChanges();
                    return RedirectToAction("WorkspaceTasks", new { Id = Id });
                }
            }
            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            ViewBag.Name = WorkArea.Name;
            ViewBag.WorkspaceId = WorkArea.WorkspaceId;
            return View(model);
        }

        [Authorize(Roles = "Manager,Admin")]
        //Workspace Tasks/WorkspaceId
        public IActionResult WorkspaceTasks(int Id)
        {
            ViewBag.WorkspaceId = Id;
            ViewBag.Name = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Id).Name;
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Id && x.PmId == userid);
            var task_list = new List<TaskListViewModel>();
            if (workspace != null)
            {
                var tasklist = _context.Tasks.Where(x => x.WorkspaceId == workspace.WorkspaceId).ToList();
                foreach (var a in tasklist)
                {
                    var t = new TaskListViewModel();
                    t.Name = a.TaskName;
                    task_list.Add(t);
                }
            }
            return View(task_list);
        }

        //WeekSetting/WorkspaceId
        public IActionResult CreateWeekSetting(int Id)
        {
            ViewBag.WorkspaceId = Id;
            ViewBag.Name = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Id).Name;
            return View();
        }

        [HttpPost]
        public IActionResult CreateWeekSetting(int Id, CreateWeekSettingViewModel model)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            if (ModelState.IsValid)
            {
                if (workspace != null)
                {
                    if (_context.WeekSettings.Where(x => x.WorkspaceId == Id).Any())
                    {
                        ViewBag.Name = workspace.Name;
                        ViewBag.WorkspaceId = workspace.WorkspaceId;
                        ModelState.AddModelError("CustomError", "You have already defined Week Settings.");
                        return View(model);
                    }
                    else
                    {
                        var weekSetting = new WeekSetting();
                        weekSetting.WorkspaceId = Id;
                        weekSetting.startDate = model.startDate;
                        weekSetting.StartDay = model.StartDay.ToString();
                        weekSetting.EndDay = model.EndDay.ToString();
                        _context.WeekSettings.Add(weekSetting);
                        _context.SaveChanges();
                        return RedirectToAction("WeekSetting", new { Id = Id });
                    }

                }
            }
            ViewBag.Name = workspace.Name;
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            return View(model);
        }

        //Week Setting/WorkspaceId
        public IActionResult WeekSetting(int Id)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Id && x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            var showWeekSetting = new ShowWeekSetting();
            if (workspace != null)
            {
                var weekSetting = _context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == Id);
                if (weekSetting != null)
                {
                    showWeekSetting.WeekSettingId = weekSetting.WeekSettingId;
                    showWeekSetting.startDate = weekSetting.startDate;
                    showWeekSetting.StartDay = weekSetting.StartDay;
                    showWeekSetting.EndDay = weekSetting.EndDay;
                }
            }
            return View(showWeekSetting);
        }

        //EditWeekSetting/WeekSettingId
        public IActionResult EditWeekSetting(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            var weekSetting = _context.WeekSettings.SingleOrDefault(x => x.WeekSettingId == Id);
            if (weekSetting == null)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            var week_setting = new EditWeekSetting();
            week_setting.startDate = weekSetting.startDate;
            week_setting.StartDay = weekSetting.StartDay;
            week_setting.EndDay = weekSetting.EndDay;
            return View(week_setting);
        }

        [HttpPost]
        public IActionResult EditWeekSetting(int Id, EditWeekSetting model)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var weekSetting = _context.WeekSettings.SingleOrDefault(x => x.WeekSettingId == Id);
                weekSetting.startDate = model.startDate;
                weekSetting.StartDay = model.StartDay;
                weekSetting.EndDay = model.EndDay;
                _context.Update(weekSetting);
                _context.SaveChanges();
                return RedirectToAction("WeekSetting", new { Id = _context.WeekSettings.SingleOrDefault(x => x.WeekSettingId == Id).WorkspaceId });
            }
            var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
            ViewBag.Name = WorkArea.Name;
            ViewBag.WorkspaceId = WorkArea.WorkspaceId;
            return View(model);
        }

        //Delete Setting/WeekSettingId
        public IActionResult DeleteSetting(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            var weekSetting = _context.WeekSettings.SingleOrDefault(x => x.WeekSettingId == Id);
            if (weekSetting == null)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            return View(weekSetting);
        }

        public IActionResult DeleteSettingConfirmed(int Id)
        {
            var weekSetting = _context.WeekSettings.SingleOrDefault(x => x.WeekSettingId == Id);
            _context.WeekSettings.Remove(weekSetting);
            _context.SaveChanges();
            return RedirectToAction("Workspace");
        }
        //Individual Dashboard
        [Authorize(Roles = "Employee")]
        public IActionResult IndividualDashboard()
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var total_invitation_requests = _context.ProjectAssignment.Where(x => x.status == "Pending").ToList();
            List<InvitationRequestsViewModel> user_invitations = new List<InvitationRequestsViewModel>();
            foreach (var a in total_invitation_requests)
            {
                var u_invitation = new InvitationRequestsViewModel();
                if (a.EmployeeId == userid)
                {
                    u_invitation.ProjectName = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).Name;
                    u_invitation.ProjectDescription = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).description;
                    u_invitation.InvitationId = a.ProjectAssignmentId;
                    u_invitation.status = a.status;
                    user_invitations.Add(u_invitation);
                }
            }
            return View(user_invitations);
        }

        //Accept Invitations/InvitationId
        [Authorize(Roles = "Employee")]
        public IActionResult AcceptInvitation(int Id)
        {
            var project_assignment = _context.ProjectAssignment.SingleOrDefault(x => x.ProjectAssignmentId == Id);
            if (project_assignment != null)
            {
                project_assignment.status = "Accepted";
                _context.ProjectAssignment.Update(project_assignment);
                _context.SaveChanges();
                return RedirectToAction("IndividualDashboard");
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "Employee")]
        //Active Requests To show Fill Daily Timesheet
        //DailyTimesheets
        public IActionResult DailyTimesheets()
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var total_accepted_invitation = _context.ProjectAssignment.Where(x => x.status == "Accepted").ToList();
            List<DailyTimesheetViewModel> daily_timesheets = new List<DailyTimesheetViewModel>();
            foreach (var a in total_accepted_invitation)
            {
                var dailyTimesheet = new DailyTimesheetViewModel();
                if (a.EmployeeId == userid)
                {
                    dailyTimesheet.ProjectName = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).Name;
                    dailyTimesheet.ProjectDescription = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).description;
                    dailyTimesheet.projectId = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).projectId;
                    daily_timesheets.Add(dailyTimesheet);
                }
            }
            return View(daily_timesheets);
        }

        [Authorize(Roles = "Employee")]
        //FillTimesheet/projectId
        public IActionResult FillTimeSheet(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            if (_context.Projects.SingleOrDefault(x => x.projectId == Id) == null)
            {
                return NotFound();
            }
            var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);
            if (_context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == Project.WorkspaceId) == null)
            {
                return RedirectToAction("AccessDenie");
            }
            else
            {
                var workspace = _context.Workspaces.SingleOrDefault(y => y.WorkspaceId == Project.WorkspaceId);
                var taskList = _context.Tasks.Where(z => z.WorkspaceId == workspace.WorkspaceId).ToList();
                var listItems = new List<SelectListItem>();
                foreach (var a in taskList)
                {
                    listItems.Add(new SelectListItem { Text = a.TaskName, Value = a.TaskName });
                }
                ViewBag.TaskList = listItems;
                return View();
            }
        }

        [HttpPost]
        public IActionResult FillTimeSheet(int Id, CreateTimesheetViewModel model)
        {
            var p = _context.Projects.SingleOrDefault(x => x.projectId == Id);
            if (_context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == p.WorkspaceId) == null)
            {
                return RedirectToAction("AccessDenie");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);
                    if (Project != null)
                    {
                        var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var timeSheet = new TimeSheet();
                        timeSheet.Date = model.Date.Date;
                        timeSheet.Task = model.Task;
                        timeSheet.TaskType = model.TaskType;
                        timeSheet.HoursSpent = model.HoursSpent;
                        timeSheet.status = "Saved";
                        timeSheet.EmployeeId = userid;
                        timeSheet.projectId = Id;
                        _context.TimeSheets.Add(timeSheet);
                        _context.SaveChanges();
                        return RedirectToAction("ShowTimeSheet", new { Id = Id });
                    }
                }
                var accessId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var WorkArea = _context.Workspaces.SingleOrDefault(x => x.PmId == accessId);
                ViewBag.Name = WorkArea.Name;
                ViewBag.WorkspaceId = WorkArea.WorkspaceId;
                return View(model);
            }
        }

        [Authorize(Roles = "Employee")]
        //DisplayTimesheet/projectId
        public IActionResult ShowTimeSheet(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);
            var pmId = _context.Workspaces.SingleOrDefault(x => x.WorkspaceId == Project.WorkspaceId).PmId;
            //ProjectManagerId
            //Check if its EndDay of week
            //If it is than send this Id
            if (_context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == Project.WorkspaceId) == null)
            {
                return RedirectToAction("AccessDenie");
            }
            var startDate = _context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == Project.WorkspaceId).startDate;
            var startDay = _context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == Project.WorkspaceId).StartDay;
            var endDay = _context.WeekSettings.SingleOrDefault(x => x.WorkspaceId == Project.WorkspaceId).EndDay;
            var DateNow = DateTime.Now;
            if (DateNow.DayOfWeek.ToString() == endDay)
            {
                ViewBag.ProjectManagerId = pmId;
                ViewBag.projectId = Id;
            }

            if (Project == null)
            {
                return NotFound();
            }
            var listOftimeSheet = _context.TimeSheets.Where(x => x.projectId == Id).ToList();
            var listOfDisplayTimeSheet = new List<DisplayTimeSheet>();
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            foreach (var a in listOftimeSheet)
            {
                if (a.EmployeeId == userid && (a.status == "Saved" || a.status == "Rejected"))
                {
                    var displayTimesheet = new DisplayTimeSheet();
                    displayTimesheet.Date = a.Date;
                    displayTimesheet.Task = a.Task;
                    displayTimesheet.TaskType = a.TaskType;
                    displayTimesheet.HoursSpent = a.HoursSpent;
                    displayTimesheet.TimeSheetId = a.TimeSheetId;
                    displayTimesheet.status = a.status;
                    listOfDisplayTimeSheet.Add(displayTimesheet);
                }
            }
            return View(listOfDisplayTimeSheet);
        }


        [Authorize(Roles = "Employee")]
        //DisplayTimesheetViewOnly/projectId
        public IActionResult ShowTimeSheetViewOnly(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);

            if (Project == null)
            {
                return NotFound();
            }
            var listOftimeSheet = _context.TimeSheets.Where(x => x.projectId == Id).ToList();
            var listOfDisplayTimeSheet = new List<DisplayTimeSheet>();
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            foreach (var a in listOftimeSheet)
            {
                if (a.EmployeeId == userid && (a.status == "Submitted" || a.status == "Rejected"))
                {
                    var displayTimesheet = new DisplayTimeSheet();
                    displayTimesheet.Date = a.Date;
                    displayTimesheet.Task = a.Task;
                    displayTimesheet.TaskType = a.TaskType;
                    displayTimesheet.HoursSpent = a.HoursSpent;
                    displayTimesheet.TimeSheetId = a.TimeSheetId;
                    displayTimesheet.status = a.status;
                    listOfDisplayTimeSheet.Add(displayTimesheet);
                }
            }
            return View(listOfDisplayTimeSheet);
        }

        [Authorize(Roles = "Employee")]
        //EditTimeSheet/TimesheetId
        public IActionResult EditTimeSheet(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var timeSheet = _context.TimeSheets.SingleOrDefault(x => x.TimeSheetId == Id && x.EmployeeId == userid);
            if (timeSheet == null)
            {
                return NotFound();
            }
            var editTimeSheet = new EditTimeSheetViewModel();
            editTimeSheet.Date = timeSheet.Date;
            editTimeSheet.Task = timeSheet.Task;
            editTimeSheet.TaskType = timeSheet.TaskType;
            editTimeSheet.HoursSpent = timeSheet.HoursSpent;
            return View(editTimeSheet);
        }

        [HttpPost]
        public IActionResult EditTimeSheet(int Id, EditTimeSheetViewModel model)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var timeSheet = _context.TimeSheets.SingleOrDefault(x => x.TimeSheetId == Id && x.EmployeeId == userid);
            timeSheet.Date = model.Date;
            timeSheet.Task = model.Task;
            timeSheet.TaskType = model.TaskType;
            timeSheet.HoursSpent = model.HoursSpent;
            _context.TimeSheets.Update(timeSheet);
            _context.SaveChanges();
            return RedirectToAction("ShowTimeSheet", new { Id = _context.TimeSheets.SingleOrDefault(x => x.TimeSheetId == Id).projectId });
        }

        [Authorize(Roles = "Employee")]
        //Submit Timesheet To Manager
        //SubmitTimesheetToManager/ManagerId/projectId

        [HttpPost]
        public IActionResult SubmitTimesheetToManager(string Id, int id2)
        {
            if (_context.TimeMateUsers.SingleOrDefault(x => x.Id == Id) == null)
            {
                return NotFound();
            }
            if (_context.Projects.SingleOrDefault(x => x.projectId == id2) == null)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var totalTimeSheets = _context.TimeSheets.Where(x => x.EmployeeId == userid && x.projectId == id2).ToList();
            foreach (var a in totalTimeSheets)
            {
                if (a.status == "Saved")
                {
                    var timeSheet = _context.TimeSheets.SingleOrDefault(x => x.TimeSheetId == a.TimeSheetId);
                    timeSheet.status = "Submitted";
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("ShowTimeSheet", new { Id = id2 });
        }


        [Authorize(Roles = "Manager")]
        //Accept or Reject Timesheet
        //AcceptOrRejectTimesheet/individualId/projectId
        public IActionResult AcceptOrRejectTimesheet(string Id, int id2)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            var totalTimesheets = _context.TimeSheets.Where(x => x.EmployeeId == Id && x.projectId == id2 && x.status == "Submitted").ToList();
            //accept or reject method
            ViewBag.projectId = id2;
            ViewBag.individualId = Id;

            var list = new List<AcceptOrRejectTimesheetViewModel>();
            foreach (var a in totalTimesheets)
            {
                var timesheet = new AcceptOrRejectTimesheetViewModel();
                timesheet.WeekStartDate = a.Date;
                timesheet.TotalHours = a.HoursSpent;
                if (a.TaskType == "Development")
                {
                    timesheet.DevelopmentHours = a.HoursSpent;
                }
                if (a.TaskType == "Learning")
                {
                    timesheet.LearningHours = a.HoursSpent;
                }
                if (a.TaskType == "Meeting")
                {
                    timesheet.MeetingHours = a.HoursSpent;
                }
                if (a.TaskType == "Marketing")
                {
                    timesheet.MarketingHours = a.HoursSpent;
                }
                if (a.TaskType == "Documentation")
                {
                    timesheet.DocumentationHours = a.HoursSpent;
                }
                list.Add(timesheet);
            }
            if (list.Count > 0)
            {
                ViewBag.WeekStartDate = list[0].WeekStartDate.Date;
            }


            return View(list);
        }

        [Authorize(Roles = "Manager")]
        //AcceptTimesheet/individualId/projectId


        public IActionResult AcceptTimesheet(string Id, int id2)
        {
            if (Id == null)
            {
                return NotFound();
            }
            if (id2 == 0)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            var totalTimeSheets = _context.TimeSheets.Where(x => x.projectId == id2 && x.EmployeeId == Id).ToList();
            foreach (var a in totalTimeSheets)
            {
                if (a.status == "Submitted")
                {
                    var timeSheet = _context.TimeSheets.SingleOrDefault(x => x.TimeSheetId == a.TimeSheetId);
                    timeSheet.status = "Accepetd";
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("AcceptOrRejectTimesheet", new { Id = Id, id2 = id2 });
        }

        [Authorize(Roles = "Manager")]
        //RejectTimesheet/individualId/projectId

        public IActionResult RejectTimesheet(string Id, int id2)
        {
            if (Id == null)
            {
                return NotFound();
            }
            if (id2 == 0)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            var totalTimeSheets = _context.TimeSheets.Where(x => x.projectId == id2 && x.EmployeeId == Id).ToList();
            foreach (var a in totalTimeSheets)
            {
                if (a.status == "Submitted")
                {
                    var timeSheet = _context.TimeSheets.SingleOrDefault(x => x.TimeSheetId == a.TimeSheetId);
                    timeSheet.status = "Rejected";
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("AcceptOrRejectTimesheet", new { Id = Id, id2 = id2 });
        }

        [Authorize(Roles = "Employee")]
        //RequestToChangeProject/projectId
        public IActionResult RequestToChangeProject(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_context.ChangeProjectRequests.Any(x => x.projectId == Id && x.individualId == userid))
            {
                //don't do this;
                return RedirectToAction("ChangeProjectRequests");
            }
            else
            {
                if (_context.ProjectAssignment.SingleOrDefault(x => x.EmployeeId == userid && x.projectId == Id) == null)
                {
                    return NotFound();
                }
                else
                {
                    var requestToChangeProject = new ChangeProjectRequests();
                    requestToChangeProject.projectId = Id;
                    requestToChangeProject.individualId = userid;
                    requestToChangeProject.status = "Request Sent";
                    _context.ChangeProjectRequests.Add(requestToChangeProject);
                    _context.SaveChanges();
                    return RedirectToAction("ChangeProjectRequests");
                }
            }
        }

        //For Individual
        [Authorize(Roles = "Employee")]
        public IActionResult ChangeProjectRequests()
        {
            var totalRequests = _context.ChangeProjectRequests.ToList();
            var requestList = new List<RequestsListViewModel>();
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            foreach (var a in totalRequests)
            {
                if (a.individualId == userid)
                {
                    var request = new RequestsListViewModel();
                    request.ProjectName = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).Name;
                    request.ProjectDescription = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).description;
                    request.status = _context.ChangeProjectRequests.SingleOrDefault(x => x.changeProjectRequestId == a.changeProjectRequestId).status;
                    requestList.Add(request);
                }
            }
            return View(requestList);
        }

        [Authorize(Roles = "Manager")]
        //ChangeProjectRequest/For Project Manager
        public IActionResult ChangeProjectRequestsPm()
        {
            var totalRequests = _context.ChangeProjectRequests.ToList();
            var requestList = new List<RequestsListViewModel>();
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            foreach (var a in totalRequests)
            {
                if (_context.Workspaces.SingleOrDefault(x => x.WorkspaceId ==
                                _context.Projects.SingleOrDefault(y => y.projectId == a.projectId).WorkspaceId).PmId == userid)
                {
                    if (a.status == "Request Sent")
                    {
                        var request = new RequestsListViewModel();
                        request.ProjectName = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).Name;
                        request.ProjectDescription = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).description;
                        request.status = "Incomming Request";
                        request.Member = _context.TimeMateUsers.SingleOrDefault(x => x.Id == a.individualId).Email;
                        request.projectId = a.projectId;
                        request.RequestId = a.changeProjectRequestId;
                        request.CanChange = true; // Set the CanChange property to true
                        ViewBag.CanChange = true;
                        requestList.Add(request);
                    }
                    else if (a.status == "Accepted")
                    {
                        var request = new RequestsListViewModel();
                        request.ProjectName = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).Name;
                        request.ProjectDescription = _context.Projects.SingleOrDefault(x => x.projectId == a.projectId).description;
                        request.status = "Accepted";
                        request.Member = _context.TimeMateUsers.SingleOrDefault(x => x.Id == a.individualId).Email;
                        request.CanChange = false; // Set the CanChange property to false for not changing Project
                        ViewBag.CanChange = false;
                        requestList.Add(request);
                    }
                }
            }
            return View(requestList);
        }

        [Authorize(Roles = "Manager")]
        //View The Productivity of Employees
        //ProductivityOfEmployee/projectId
        public IActionResult ProductivityOfEmployee(int Id, ProductivityViewModel model)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workspace = _context.Workspaces.SingleOrDefault(x => x.PmId == userid);
            ViewBag.WorkspaceId = workspace.WorkspaceId;
            ViewBag.Name = workspace.Name;
            var totalTimesheet = _context.TimeSheets.Where(x => x.status == "Accepetd" && x.projectId == Id).ToList();
            var productivityList = new List<ProductivityViewModel>();
            //get timesheets with different ids
            var listOfDifferentIds = _context.TimeSheets.Where(x => x.projectId == Id).GroupBy(t => t.EmployeeId).Select(g => g.First()).ToList();
            foreach (var a in listOfDifferentIds)
            {
                var countHours = 0;
                var productivity = new ProductivityViewModel();
                for (var i = 0; i < totalTimesheet.Count(); i++)
                {
                    productivity.EmployeeName = _context.TimeMateUsers.SingleOrDefault(x => x.Id == a.EmployeeId).UserName;

                    if (a.EmployeeId == totalTimesheet[i].EmployeeId)
                    {
                        countHours += totalTimesheet[i].HoursSpent;
                        productivity.WorkingHours = countHours;
                        continue;
                    }

                }
                productivityList.Add(productivity);
            }
            return View(productivityList);
        }
        //Change Project
        //ChangeProject/projectId
        public IActionResult ChangeProject(int Id, int id2)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == 0)
            {
                return NotFound();
            }
            if (id2 == 0)
            {
                return NotFound();
            }
            if (_context.Projects.SingleOrDefault(x => x.projectId == Id && x.WorkspaceId == _context.Workspaces.SingleOrDefault(y => y.PmId == userid).WorkspaceId) == null)
            {
                return NotFound();
            }
            if (_context.ChangeProjectRequests.SingleOrDefault(x => x.changeProjectRequestId == id2) == null)
            {
                return NotFound();
            }
            var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id && x.WorkspaceId == _context.Workspaces.SingleOrDefault(y => y.PmId == userid).WorkspaceId);
            ViewBag.WorkspaceId = _context.Workspaces.SingleOrDefault(y => y.PmId == userid).WorkspaceId;
            ViewBag.Name = _context.Workspaces.SingleOrDefault(y => y.PmId == userid).Name;
            var p = new ChangeProjectViewModel();
            p.ProjectName = Project.Name;
            p.description = Project.description;
            return View(p);
        }
        [HttpPost]
        public IActionResult ChangeProject(int Id, int id2, ChangeProjectViewModel model)
        {
            var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_context.Projects.SingleOrDefault(x => x.projectId == Id && x.WorkspaceId == _context.Workspaces.SingleOrDefault(y => y.PmId == userid).WorkspaceId) == null)
            {
                return NotFound();
            }
            if (_context.ChangeProjectRequests.SingleOrDefault(x => x.changeProjectRequestId == id2) == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if (_context.Projects.Any(x => x.Name == model.ProjectName))
                {
                    ViewBag.Name = _context.Workspaces.SingleOrDefault(y => y.PmId == userid).Name;
                    ViewBag.WorkspaceId = _context.Workspaces.SingleOrDefault(y => y.PmId == userid).WorkspaceId;
                    ModelState.AddModelError("CustomError", "This Project already exist in your Workspace");
                    return View(model);
                }
                else
                {
                    var Project = _context.Projects.SingleOrDefault(x => x.projectId == Id);
                    Project.Name = model.ProjectName;
                    Project.description = model.description;
                    _context.Projects.Update(Project);
                    _context.SaveChanges();
                    var request = _context.ChangeProjectRequests.SingleOrDefault(x => x.changeProjectRequestId == id2);
                    request.status = "Accepted";
                    _context.ChangeProjectRequests.Update(request);
                    _context.SaveChanges();
                    return RedirectToAction("ChangeProjectRequestsPm");
                }
            }
            ViewBag.WorkspaceId = _context.Workspaces.SingleOrDefault(y => y.PmId == userid).WorkspaceId;
            ViewBag.Name = _context.Workspaces.SingleOrDefault(y => y.PmId == userid).Name;
            return View(model);
        }


        //Cannot Access the page
        //AccessDenie
        public IActionResult AccessDenie()
        {
            ViewData["Title"] = "Access denied";
            return View();
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult ExportTasks()
        {
            var tasks = _context.Tasks.ToList();
            var fileContents = "Id,Name,description,EmployeeId,EmployeeName,Deadline,Priority,IsCompleted\n";
            foreach (var task in tasks)
            {
                fileContents += $"{task.TaskId},{task.TaskName},{task.WorkspaceId},{task.Workspace}\n";
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(fileContents);
            return File(bytes, "text/csv", "tasks.csv");
        }

        // POST: Task/ImportTasks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTasks(IFormFile importFile)
        {
            try
            {
                if (importFile == null || importFile.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file");
                    return RedirectToAction(nameof(DailyTimesheets));
                }

                if (!importFile.FileName.EndsWith(".csv"))
                {
                    ModelState.AddModelError("File", "Invalid file format");
                    return RedirectToAction(nameof(DailyTimesheets));
                }

                using (var streamReader = new StreamReader(importFile.OpenReadStream()))
                {
                    string line;
                    while ((line = await streamReader.ReadLineAsync()) != null)
                    {
                        string[] values = line.Split(',');

                        // Ensure the values array has the expected length
                        if (values.Length == 8)
                        {
                            if (int.TryParse(values[3], out int taskId) && int.TryParse(values[7], out int workspaceId))
                            {
                                Tasks task = new Tasks
                                {
                                    TaskId = taskId,
                                    TaskName = values[2],
                                    WorkspaceId = workspaceId,
                                };
                                _context.Tasks.Add(task);
                            }
                            else
                            {
                                // Log or handle parsing errors
                                ModelState.AddModelError("File", "Error parsing TaskId or WorkspaceId");
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DailyTimesheets));
            }
            catch (Exception ex)
            {
                // Log the exception for further investigation
                Console.WriteLine($"Exception during file import: {ex}");
                ModelState.AddModelError("File", "Error importing file: " + ex.Message);
                return RedirectToAction(nameof(DailyTimesheets));
            }
        }

        private static List<Message> _messages = new List<Message>();

        // GET: /Message/
        public IActionResult MessageIndex()
        {
            return View(_messages);
        }

        // GET: /Message/Create
        public IActionResult MessageCreate()
        {
            return View();
        }

        // POST: /Message/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MessageCreate([Bind("Subject,Body,RecipientEmail")] Message message)
        {
            if (ModelState.IsValid)
            {
                message.Id = _messages.Count + 1;
                message.SentDate = DateTime.UtcNow;
                message.SenderEmail = User.Identity.Name;
                message.IsRead = false;
                message.IsArchived = false;
                _messages.Add(message);
                return RedirectToAction(nameof(MessageIndex));
            }
            return View(message);
        }

        // GET: /Message/Details/5
        public IActionResult MessageDetails(int Id)
        {
            var message = _messages.FirstOrDefault(m => m.Id == Id);
            if (message == null)
            {
                return NotFound();
            }
            message.IsRead = true;
            return View(message);
        }

        // GET: /Message/Archive/5
        public IActionResult Archive(int Id)
        {
            var message = _messages.FirstOrDefault(m => m.Id == Id);
            if (message == null)
            {
                return NotFound();
            }
            message.IsArchived = true;
            return RedirectToAction(nameof(MessageIndex));
        }
        public IActionResult UnArchive(int Id)
        {
            var message = _messages.FirstOrDefault(m => m.Id == Id);
            if (message != null)
            {
                message.IsArchived = false;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(MessageIndex));
        }

        public async Task<IActionResult> PermissionIndex()
        {
            return _context.Permission != null ?
                        View(await _context.Permission.ToListAsync()) :
                        Problem("Entity set 'ProjectContext.Permission'  is null.");
        }

        // GET: Permission/Details/5
        public async Task<IActionResult> PermissionDetails(int? Id)
        {
            if (Id == null || _context.Permission == null)
            {
                return NotFound();
            }

            var permission = await _context.Permission
                .FirstOrDefaultAsync(m => m.Id == Id);
            if (permission == null)
            {
                return NotFound();
            }

            return View(permission);
        }

        // GET: Permission/Create
        public IActionResult PermissionCreate()
        {
            return View();
        }

        // POST: Permission/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermissionCreate([Bind("Id,Name,description")] Permission permission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(permission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(PermissionIndex));
            }
            return View(permission);
        }

        // GET: Permission/Edit/5
        public async Task<IActionResult> PermissionEdit(int? Id)
        {
            if (Id == null || _context.Permission == null)
            {
                return NotFound();
            }

            var permission = await _context.Permission.FindAsync(Id);
            if (permission == null)
            {
                return NotFound();
            }
            return View(permission);
        }

        // POST: Permission/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermissionEdit(int Id, [Bind("Id,Name,description")] Permission permission)
        {
            if (Id != permission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(permission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PermissionExists(permission.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(PermissionIndex));
            }
            return View(permission);
        }

        // GET: Permission/Delete/5
        public async Task<IActionResult> PermissionDelete(int? Id)
        {
            if (Id == null || _context.Permission == null)
            {
                return NotFound();
            }

            var permission = await _context.Permission
                .FirstOrDefaultAsync(m => m.Id == Id);
            if (permission == null)
            {
                return NotFound();
            }

            return View(permission);
        }

        // POST: Permission/Delete/5
        [HttpPost, ActionName("PermissionDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermissionDeleteConfirmed(int Id)
        {
            if (_context.Permission == null)
            {
                return Problem("Entity set 'ProjectContext.Permission'  is null.");
            }
            var permission = await _context.Permission.FindAsync(Id);
            if (permission != null)
            {
                _context.Permission.Remove(permission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PermissionIndex));
        }

        private bool PermissionExists(int Id)
        {
            return (_context.Permission?.Any(e => e.Id == Id)).GetValueOrDefault();
        }

        public IActionResult DashBoard()
        {
            return View();
        }


        public IActionResult PermissionApprove(int permissionId)
        {
            var permission = _context.Permission.FirstOrDefault(p => p.Id == permissionId);

            if (permission == null)
            {
                return NotFound();
            }

            permission.IsApproved = true;

            _context.SaveChanges();

            return RedirectToAction("PermissionIndex");
        }

        public IActionResult PermissionReject(int permissionId)
        {
            var permission = _context.Permission.FirstOrDefault(p => p.Id == permissionId);

            if (permission == null)
            {
                return NotFound();
            }

            permission.IsApproved = false;

            _context.SaveChanges();

            return RedirectToAction("PermissionIndex");
        }


        public ActionResult Index()
        {
            return View();
        }

        // API endpoint to retrieve timesheet events data in JSON format
        public ActionResult GetTimesheetEvents()
        {
            var timesheetEvents = _context.TimeSheets
                .Select(e => new
                {
                    Id = e.TimeSheetId,
                    title = e.Task,
                    start = e.Date.ToString("yyyy-MM-dd"),
                    color = e.status == "Completed" ? "Green" : "Red"
                })
                .ToList();

            return Json(timesheetEvents);
        }
    }
}