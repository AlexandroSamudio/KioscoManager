using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json","application/problem+json")]
public abstract class BaseApiController : ControllerBase
{

}