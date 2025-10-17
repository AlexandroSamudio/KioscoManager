using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.TestInfrastructure;

public abstract class ControllerTestBase
{
  protected static T SetupController<T>(T controller, ClaimsPrincipal? user = null) where T : ControllerBase
  {
    var http = new DefaultHttpContext();
    if (user is not null)
    {
      http.User = user;
    }
    controller.ControllerContext = new ControllerContext { HttpContext = http };
    return controller;
  }
}
