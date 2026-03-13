using Microsoft.AspNetCore.Mvc;

namespace integration_test_harness.Controllers
{
  /// <summary>
  /// Controller for testing that the default route doesn't break known routes.
  /// </summary>
  [Route("valid-route")]
  public class ValidRouteController : ControllerBase
  {
    /// <summary>
    /// Handle unknown routes for all the verbs.
    /// </summary>
    [HttpDelete]
    [HttpGet]
    [HttpHead]
    [HttpOptions]
    [HttpPatch]
    [HttpPost]
    [HttpPut]
    public string Route()
    {
      return "Valid Route.";
    }
  }
}
