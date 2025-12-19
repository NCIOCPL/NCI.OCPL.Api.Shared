using Microsoft.Extensions.Hosting;

using NCI.OCPL.Api.Common;

namespace integration_test_harness
{
    public class Program : NciApiProgramBase
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder<Startup>(args).Build().Run();
        }
    }
}
