using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TimeMate.Controllers
{
    [Log]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<HomeController> _logger;


        public RoleController(RoleManager<IdentityRole> roleManager, ILogger<HomeController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if (model.Name == null)
            {
                ModelState.AddModelError("Name", "Role name cannot be null.");
                return View(model);
            }

            var existingRole = await _roleManager.FindByNameAsync(model.Name);

            if (existingRole != null)
            {
                ModelState.AddModelError("Name", "Role already exists");

                return View(model);
            }

            var role = new IdentityRole(model.Name);

            role.ConcurrencyStamp = Guid.NewGuid().ToString();
            
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            return RedirectToAction("Index");
        }

    }
}
