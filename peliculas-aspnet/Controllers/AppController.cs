using Microsoft.AspNetCore.Mvc;

namespace PeliculasAspnet.Controllers;

[ApiController]
[Route("/")]
public class AppController : ControllerBase
{
    [HttpGet]
    public ContentResult Get()
    {
        return Content("Hello World!");
    }
}
