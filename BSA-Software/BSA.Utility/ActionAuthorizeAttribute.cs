using System.Web;
using System.Web.Http;


namespace KGERP.Utility
{
    public class ActionAuthorizeAttribute : AuthorizeAttribute
    {
        public int Feature { get; set; }
        public ActionAuthorizeAttribute(int Feature = 0)
        {
            this.Feature = Feature;
        }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
           //ILoggedInUser loggedInUser = HandleUnauthorizedRequest.
            if(string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
               return false;
            }

            if(Feature>=0)
            {
                return true;// loggedInUser.UserFeatureCodes.Contains(Feature);
            }
            else
            {
                return false;
            }
        }
    }
}
