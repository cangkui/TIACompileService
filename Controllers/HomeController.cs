using System.Web.Http;

namespace TiaImportExample.Controllers
{
    public class HomeController: ApiController
    {
        // GET api/home
        [HttpGet]
        [Route("api/home")]
        public IHttpActionResult Get()
        {
            return Ok("Hello, World!");
        }
        // POST api/home
        [HttpPost]
        [Route("api/home")]
        public IHttpActionResult Post([FromBody] string value)
        {
            return Ok($"Received: {value}");
        }
    }
}
