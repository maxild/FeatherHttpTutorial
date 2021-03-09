using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

var app = WebApplication.Create(args);

app.MapGet("/api/todos", GetTodos);
app.MapPost("/api/todos", CreateTodo);
app.MapPost("/api/todos/{id}", UpdateCompleted); // TODO: PUT!!!
app.MapDelete("/api/todos/{id}", DeleteTodo);

// app.MapGet("/", async http =>
// {
//     await http.Response.WriteAsync("Hello World!");
// });

await app.RunAsync();

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
    await db.Todos.AddAsync(todo);
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
    todo.IsComplete = inputTodo.IsComplete;

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