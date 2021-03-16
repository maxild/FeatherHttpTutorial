#define STARTUP
///////////////////////////////////
// Idiomatic StartUp Pattern
#if STARTUP
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            // ROUTING ZONE
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // API routes for Todos
                endpoints.MapGet("/api/todos", async context => await TodosApi.GetTodos(context));
                endpoints.MapPost("/api/todos", async context => await TodosApi.CreateTodo(context));
                endpoints.MapPost("/api/todos/{id}", TodosApi.UpdateCompleted);
                endpoints.MapDelete("/api/todos/{id}", TodosApi.DeleteTodo);

                // WHY are those endpoint required for Blazor WASM?
                // https://localhost:5001/_framework/aspnetcore-browser-refresh.js - - - 404
                endpoints.MapRazorPages();
                endpoints.MapControllers();

                // REQUIRED because blazor client sends: GET https://localhost:5001/
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }

    public static class TodosApi
    {
        // GET /api/todos
        public static async Task GetTodos(HttpContext http)
        {
            await using var db = new TodoDbContext();
            var todos = await db.Todos.ToListAsync();

            await http.Response.WriteAsJsonAsync(todos);
        }

        // POST /api/todos {body payload}
        public static async Task CreateTodo(HttpContext http)
        {
            var newTodo = await http.Request.ReadFromJsonAsync<TodoItem>();

            Debug.Assert(newTodo.Id == 0);
            Debug.Assert(!newTodo.IsComplete);
            // Debug.Assert(!string.IsNullOrWhiteSpace(newTodo.Name));

            await using var db = new TodoDbContext();
            await db.Todos.AddAsync(newTodo!);
            await db.SaveChangesAsync();

            Debug.Assert(newTodo.Id > 0); // Q: Is Id updated by SaveChanges?

            http.Response.StatusCode = 204;
        }

        // POST /api/todos/{id} {body payload}
        public static async Task UpdateCompleted(HttpContext http)
        {
            if (!http.Request.RouteValues.TryGet("id", out int id))
            {
                http.Response.StatusCode = 400;
                return;
            }

            // TODO: This can be done with 1 request to the DB
            await using var db = new TodoDbContext();
            var todo = await db.Todos.FindAsync(id);

            if (todo == null)
            {
                http.Response.StatusCode = 404;
                return;
            }

            var inputTodo = await http.Request.ReadFromJsonAsync<TodoItem>();
            // we just need to update the IsComplete value from the input/form
            todo.IsComplete = inputTodo!.IsComplete;

            await db.SaveChangesAsync();

            http.Response.StatusCode = 204;
        }

        // DELETE /api/todos/{id}
        public static async Task DeleteTodo(HttpContext http)
        {
            if (!http.Request.RouteValues.TryGet("id", out int id))
            {
                http.Response.StatusCode = 400;
                return;
            }

            // TODO: This can be done with single req to database
            await using var db = new TodoDbContext();
            var todo = await db.Todos.FindAsync(id);
            if (todo == null)
            {
                http.Response.StatusCode = 404;
                return;
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 204;
        }
    }

}

#else

///////////////////////////////////////////////////////////////
// No StartUp (FeatherHttp)
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// TODO: Are the 2 lines necessary?
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// TODO: Only in development builds
app.UseDeveloperExceptionPage();
app.UseWebAssemblyDebugging();

app.UseHttpsRedirection();

// Configure the app to serve Blazor WebAssembly framework files from the root path "/"
// (adds routes for gzip/brotli client artifacts via staticfiles CompositeFileProvider,
// Blazor-Environment setup).
// REQUIREMENT: ProjectReference to the BlazorWasm project
app.UseBlazorFrameworkFiles();

app.UseStaticFiles();

// ===== START: Endpoint ROUTING zone

// API routes for Todos
app.MapGet("/api/todos", GetTodos);
app.MapPost("/api/todos", CreateTodo);
app.MapPost("/api/todos/{id}", UpdateCompleted); // TODO: PUT!!!
app.MapDelete("/api/todos/{id}", DeleteTodo);

app.MapRazorPages();
app.MapControllers();
// app.MapFallbackToFile("{*path:nonfile}", "index.html");
app.MapFallbackToFile("index.html");

// ===== END: Endpoint ROUTING zone

await app.RunAsync();

////////////////////////////////////////////////////////////////////////////////

// GET /api/todos
static async Task GetTodos(HttpContext http)
{
    using var db = new TodoDbContext();
    var todos = await db.Todos.ToListAsync();

    await http.Response.WriteAsJsonAsync(todos);
}

// POST /api/todos
static async Task CreateTodo(HttpContext http)
{
    var todo = await http.Request.ReadFromJsonAsync<TodoItem>();

    using var db = new TodoDbContext();
    await db.Todos.AddAsync(todo!);
    await db.SaveChangesAsync();

    http.Response.StatusCode = 204;
}

// POST /api/todos/{id}
static async Task UpdateCompleted(HttpContext http)
{
    if (!http.Request.RouteValues.TryGet("id", out int id))
    {
        http.Response.StatusCode = 400;
        return;
    }

    using var db = new TodoDbContext();
    var todo = await db.Todos.FindAsync(id);

    if (todo == null)
    {
        http.Response.StatusCode = 404;
        return;
    }

    var inputTodo = await http.Request.ReadFromJsonAsync<TodoItem>();
    todo.IsComplete = inputTodo!.IsComplete;

    await db.SaveChangesAsync();

    http.Response.StatusCode = 204;
}

// DELETE /api/todos/{id}
static async Task DeleteTodo(HttpContext http)
{
    if (!http.Request.RouteValues.TryGet("id", out int id))
    {
        http.Response.StatusCode = 400;
        return;
    }

    // TODO: This can be done with single req to database
    using var db = new TodoDbContext();
    var todo = await db.Todos.FindAsync(id);
    if (todo == null)
    {
        http.Response.StatusCode = 404;
        return;
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    http.Response.StatusCode = 204;
}

#endif