using KGERP.Models;
using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using KGERP.Controllers;


namespace KGERP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ModelMapper.SetUp();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));

            CultureInfo newCulture = new CultureInfo("en-ZA", false);
            // NOTE: change the culture name en-ZA to whatever culture suits your needs

            newCulture.DateTimeFormat.DateSeparator = "/";
            newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            newCulture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy hh:mm:ss tt";

            System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;


            SetUpTimer();

        }

        private static System.Threading.Timer _timer;

        private void SetUpTimer()
        {
            TimeSpan timeToGo = DateTime.Now.AddDays(1).Date - DateTime.Now.Date; //timespan for 00:00 tomorrow 
            _timer = new System.Threading.Timer(x => SendEmail(), null, timeToGo, new TimeSpan(1, 0, 0, 0));
        }

        public void SendEmail()
        {
            //string body = "How are you?";
            //MailService.SendMail(string.Empty, string.Empty, "ashraf.erp@krishibidgroup.com", "Ashraf", string.Empty, string.Empty, "Test mail to Ashraf", body, out string sendStatus);

        }

        public bool IsReleased = true;

        //Custom Error Handdle
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Response.Clear();

            var httpException = exception as HttpException;
            RouteData routeData = new RouteData();

            routeData.Values["controller"] = "Error"; // Your error controller
            routeData.Values["action"] = "Index";     // Your error action

            if (httpException != null)
            {
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        routeData.Values["action"] = "NotFound"; // Custom action for 404 errors
                        break;
                    // Handle other HTTP errors if needed
                }
            }

            Server.ClearError();
            Response.TrySkipIisCustomErrors = true;
            IController errorController = new ErrorController(exception.Message.ToString()); // Your error controller
            errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }


    }
}
