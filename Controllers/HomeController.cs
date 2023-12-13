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
        try
        {
            if (feedback != null && ModelState.IsValid)
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var email = User.FindFirstValue(ClaimTypes.Email);

                feedback.UserName = userName;
                feedback.Email = email;

                var json = JsonConvert.SerializeObject(feedback);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = _httpClientFactory.CreateClient())
                {
                    var apiUrl = "http://localhost:5089/api/FeedBack"; // Replace with your API endpoint
                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Feedback submitted successfully
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Log or handle the unsuccessful response
                        _logger.LogError("Failed to submit feedback. Status code: {StatusCode}", response.StatusCode);
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            // Log or handle the exception
            _logger.LogError(ex, "Error submitting feedback");
        }

        // Handle any other errors or redirect to an error page
        return View("Error");
    }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}