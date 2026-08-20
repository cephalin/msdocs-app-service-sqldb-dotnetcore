using System.Text.Json;
using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DotNetCoreSqlDb.Controllers
{
    public class TodosController : Controller
    {
        private const string TodoItemsCacheKey = "TodoItemsList";

        private readonly ILogger<TodosController> _logger;
        private readonly MyDatabaseContext _context;
        private readonly IDistributedCache _cache;

        public TodosController(
            MyDatabaseContext context,
            IDistributedCache cache,
            ILogger<TodosController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // GET: Todos
        public async Task<IActionResult> Index()
        {
            return View(await BuildIndexViewModel());
        }

        // POST: Todos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Description", Prefix = "NewTodo")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(todo);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync(TodoItemsCacheKey);

                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Index), await BuildIndexViewModel(todo));
        }

        // POST: Todos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var todo = await _context.Todo.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(TodoItemsCacheKey);

            return RedirectToAction(nameof(Index));
        }

        private async Task<TodosIndexViewModel> BuildIndexViewModel(Todo? newTodo = null)
        {
            var cachedTodos = await _cache.GetStringAsync(TodoItemsCacheKey);
            IReadOnlyList<Todo> todos;

            if (cachedTodos != null)
            {
                _logger.LogInformation("Data from cache.");
                todos = JsonSerializer.Deserialize<List<Todo>>(cachedTodos)
                    ?? throw new JsonException("The cached Todo list was null.");
            }
            else
            {
                _logger.LogInformation("Data from database.");
                todos = await _context.Todo.AsNoTracking().ToListAsync();
                await _cache.SetStringAsync(TodoItemsCacheKey, JsonSerializer.Serialize(todos));
            }

            return new TodosIndexViewModel
            {
                NewTodo = newTodo ?? new Todo(),
                Todos = todos
            };
        }
    }
}
