using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TCB;

namespace SimplyWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            DogApiProxy api = new DogApiProxy(new CircuitBreaker());
            services.AddSingleton(api);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var dogProxy = app.ApplicationServices.GetService(typeof(DogApiProxy)) as DogApiProxy;
                
                Console.WriteLine(DateTime.Now);
                var randomDogImageUrl = await dogProxy.GetRandomDogImageUrl();
                Console.WriteLine(randomDogImageUrl);
                
                await Task.Delay(250);
                await context.Response.WriteAsync("Hello World!");
            });
            
            
        }
    }
}
