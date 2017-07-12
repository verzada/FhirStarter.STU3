using System.Web.Mvc;

namespace FhirStarter.Inferno.Controller
{
    public class HomeController : System.Web.Mvc.Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
    }
}