using MeuTodo.Data;
using MeuTodo.Models;
using MeuTodo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuTodo.Controller
{
    //Controler pode ser para sites, tanto quanto para apis, então usamos um atributo pra marcar isso
    [ApiController]
    //Atributo de rota, cria um prefixo de rota. Sempre importante alterar a versão da sua api, pra nao quebrar o front
    [Route("v1")]
    public class TodoController : ControllerBase
    {

        #region Forma clássica de utilizar a ID
        AppDbContext _context = new AppDbContext();
        public TodoController(AppDbContext context)
        {
            _context = context;
        }
        #endregion

        [HttpGet]
        [Route(template:"todos")]
        public async Task<IActionResult> GetAsync([FromServices] AppDbContext context) //Isso é uma forma de usar a injeção de dependencia, chamando o FromServices, a outra forma é pelo construtor
        {
            var todos = await context
                        .Todos
                        .AsNoTracking() //Como a gente nao vai manipular, as gente colocar esse comando para otimizar
                        .ToListAsync();
            return Ok(todos);
        }

        [HttpGet]
        [Route(template: "todos/{id}")] //Chaves marcam os parametros
        public async Task<IActionResult> GetByIdAsync([FromServices] AppDbContext context, [FromRoute]int id)
        {
            var todo = await context
                        .Todos
                        .AsNoTracking() //Como a gente nao vai manipular, as gente colocar esse comando para otimizar
                        .FirstOrDefaultAsync(x => x.Id == id);
            return todo == null ? NotFound() : Ok(todo);
        }

        [HttpPost(template:"todos")]
        public async Task<IActionResult> PostAsync([FromServices] AppDbContext context,[FromBody] CreateTodoViewModel model)
        {
            if (!ModelState.IsValid) //Vai aplicar as validações no CreateTodo, como o required. Transforma o json em um objeto e o json para um objeto
                return BadRequest();

            var todo = new Todo
            {
                Date = System.DateTime.Now,
                Done = false,
                Title = model.Title,
            };

            try
            {
                await context.Todos.AddAsync(todo); //salvando só na memória
                await context.SaveChangesAsync(); //Salva no banco
                return Created(uri: $"vi1/todos/{todo.Id}", todo); //retorna a confirmação da criação
            }
            catch (System.Exception e)
            {

                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }

        [HttpPut(template:"todos/{id}")]
        public async Task<IActionResult> PutAsync([FromServices] AppDbContext context, [FromBody] CreateTodoViewModel model, [FromRoute] int ID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == ID);

            if (todo == null)
            {
                return NotFound();
            }

            try
            {
                todo.Title = model.Title;

                context.Todos.Update(todo); //salvando só na memória
                await context.SaveChangesAsync();
                return Ok(todo);
            }
            catch (System.Exception)
            {

                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }

        [HttpDelete(template: "todos/{id}")]
        public async Task<IActionResult> DeleteAsync([FromServices] AppDbContext context, [FromRoute] int ID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == ID);

            if (todo == null)
            {
                return NotFound();
            }

            try
            {
                context.Todos.Remove(todo);
                await context.SaveChangesAsync();
                return Ok(todo);
            }
            catch (System.Exception)
            {

                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }
    }
}
