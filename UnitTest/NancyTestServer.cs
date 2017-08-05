using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy;
using Nancy.Owin;

namespace UnitTest
{
    public class NancyTestServer
    {
        public IWebHost Host;
        public void Start()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
            Host = host;
            Host.Start();
        }

        public void Stop()
        {
            Host.Dispose();
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy());
        }
    }

    public class TestModule : NancyModule{

        public Response test()
        {
            var r = (Response)"Some string that goes into the body";
            r.StatusCode = HttpStatusCode.BadRequest;

            return r;
        }

        public TestModule()
        {
            Get("/", _ => "OK");

            Get("/badrequest/", _ => HttpStatusCode.BadRequest);

            Get("/nocontent/",  _ => HttpStatusCode.NoContent);

            Get("/BadGateway/", _ => HttpStatusCode.BadGateway);

            Get("/BandwidthLimitExceeded/", _ => HttpStatusCode.BandwidthLimitExceeded);

            Get("/Conflict/", _ => HttpStatusCode.Conflict);

            Get("/Forbidden/", _ => HttpStatusCode.Forbidden);
        }
    }
}