using System;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Web.Http;
using TiaCompilerCLI.Services;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TiaCompilerCLI.Controllers
{
    public class TiaApiController : ApiController
    {
        private readonly TIACompileService _tiaService;

        public TiaApiController()
        {
            _tiaService = TIACompileService.Instance;
        }

        // POST api/tiaapi/process
        [HttpPost]
        [Route("api/tiaapi/process")]
        public HttpResponseMessage Process([FromBody] TiaRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BlockName) || string.IsNullOrEmpty(request.Code))
            {
                var error = new ResponseData { Success = false, Result = "Input cannot be empty" };
                var errorJson = JsonConvert.SerializeObject(error);
                var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(errorJson, Encoding.UTF8, "application/json")
                };
                return errorResponse;
            }

            try
            {
                var result = _tiaService.Process(request.BlockName, request.Code);
                var json = JsonConvert.SerializeObject(result);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while processing request: {ex.Message}");
                var error = new ResponseData { Success = false, Result = $"Exception occurred while processing request: {ex.Message}", Errors = new List<ErrorMessage>() };
                var json = JsonConvert.SerializeObject(error);
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }
        }
    }

    public class TiaRequest
    {
        public string BlockName { get; set; }
        public string Code { get; set; }
    }
}
