using System;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    public class ErrorController : Controller
    {
        private readonly string _message;

        public ErrorController(string message="")
        {
            _message=message;
        }
        public ActionResult Index()
        {
            // Fetch the last exception from the server
            Exception exception = Server.GetLastError();
            // Log the exception if needed

            // Display a generic error message
            
            if (!string.IsNullOrEmpty(_message) &&(int) _message.Length > 300)
            {
                ViewBag.ErrorMessage = "An error occurred, but no specific message was provided. Pleas contact with support team!";
            }
            else
            {
                ViewBag.ErrorMessage = _message;
            }
            

            return View();
        }

        public ActionResult NotFound()
        {
            // Handle 404 Not Found errors
            ViewBag.ErrorMessage = "Page not found!";

            return View();
        }
        // Add other action methods to handle different types of errors if needed
    }
}