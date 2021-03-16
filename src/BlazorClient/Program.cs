using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // configure the root DOM element...document.getElementById('root') in index.js in React
            // we call it #root (default template calls is #app)
            builder.RootComponents.Add<App>("#root");

            // HttpClient is injected in the component tree (configured in DI here)
            // A hosted Blazor solution based on the Blazor WebAssembly project template uses the same
            // base address for the client and server apps. The client app's HttpClient.BaseAddress is
            // set to a URI of builder.HostEnvironment.BaseAddress by default.
            builder.Services.AddScoped(
                _ => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

            await builder.Build().RunAsync();
        }
    }
}
