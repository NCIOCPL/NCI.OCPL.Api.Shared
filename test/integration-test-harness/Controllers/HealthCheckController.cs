using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using NCI.OCPL.Api.Common;

namespace integration_test_harness.Controllers
{
  [Route("HealthCheck")]
  public class HealthCheckController : ControllerBase
  {
    private readonly IHealthCheckService _healthSvc;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="healthCheckSvc">Elasticsearch health check instance.</param>
    /// <returns></returns>
    public HealthCheckController(IHealthCheckService healthCheckSvc)
      => _healthSvc = healthCheckSvc;


    [HttpGet]
    public async Task<bool> IsHealthy()
    {
      return await _healthSvc.IndexIsHealthy();
    }
  }
}