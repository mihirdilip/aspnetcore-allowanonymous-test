using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MvcController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public string Get()
        {
            return "Hello World from MVC!";
        }
    }

    public class HomeController : Controller
    {
        [AllowAnonymous]   
        public string Foo() => "Bar";
    }
}
