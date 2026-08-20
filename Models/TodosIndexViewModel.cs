namespace DotNetCoreSqlDb.Models
{
    public class TodosIndexViewModel
    {
        public Todo NewTodo { get; set; } = new();
        public IReadOnlyList<Todo> Todos { get; set; } = Array.Empty<Todo>();
    }
}
