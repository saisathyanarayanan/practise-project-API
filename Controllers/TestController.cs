using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    // GET: api/test/method-one or api/test/method1
    [HttpGet("method-one")]
    [HttpGet("method1")]
    public ActionResult<string> GetMethodOne()
    {
        return Ok("METHOD ONE");
    }

    // GET: api/test/method-two or api/test/method2
    [HttpGet("method-two")]
    [HttpGet("method2")]
    public ActionResult<string> GetMethodTwo()
    {
        return Ok("METHOD 2");
    }
}
