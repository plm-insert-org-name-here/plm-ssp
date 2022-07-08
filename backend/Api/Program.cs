using System.Threading.Tasks;
using Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args);

            host.UseLogging();
            host.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

            return host;
        }
    }
}