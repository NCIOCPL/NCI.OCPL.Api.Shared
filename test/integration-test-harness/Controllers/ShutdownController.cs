using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace integration_test_harness.Controllers
{

  /// <summary>
  /// Controller for shutting down the application.
  /// </summary>
  [Route("shutdown")]
  public class ShutdownController : ControllerBase
  {
    private readonly IHostApplicationLifetime _appLifetime;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="appLifetime">Application lifetime instance.</param>
    public ShutdownController(IHostApplicationLifetime appLifetime)
      => _appLifetime = appLifetime;

    /// <summary>
    /// Shutdown the application.
    /// </summary>
    [HttpPost]
    public async Task<string> Shutdown()
    {
      // Trigger application shutdown.
      await Task.Run(() => _appLifetime.StopApplication());

      return "Shutting down the application.";
    }
  }
}