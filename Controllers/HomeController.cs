using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using TimeMate.Models;
#nullable disable

namespace TimeMate.Controllers
{
    [Log]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IActionResult Index()
        {

            _logger.LogInformation("Home/Index page accessed.");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [TypeFilter(typeof(CustomAuthenticationFilter))]
        public IActionResult SubmitFeedback()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [TypeFilter(typeof(CustomAuthenticationFilter))]

        public async Task<IActionResult> SubmitFeedback(Feedback feedback)
        {
            if (feedback != null && ModelState.IsValid) 
            {
                var UserName = User.FindFirstValue(ClaimTypes.Name);
                var email = User.FindFirstValue(ClaimTypes.Email);
               
                feedback.UserName = UserName;
                feedback.Email = email;

                var json = JsonConvert.SerializeObject(feedback);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var apiUrl = "https://localhost:7051/api/FeedBack"; 
                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Feedback submitted successfully
                        return RedirectToAction("Index");
                    }
                }
            }

            // Feedback Failed
            return View("Error");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}