using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Modelo;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IHubContext<TareasHub> _hubContext;

        public TareasController(MyDbContext context, IHubContext<TareasHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private async Task<(int eliminadas, int completadasNoEliminadas, int pendientesNoEliminadas)> GetTareasCountAsync()
        {
            var eliminadas = await _context.Tareas.CountAsync(t => t.IsDeleted);
            var completadasNoEliminadas = await _context.Tareas.CountAsync(t => !t.IsDeleted && t.Completada);
            var pendientesNoEliminadas = await _context.Tareas.CountAsync(t => !t.IsDeleted && !t.Completada);

            return (eliminadas, completadasNoEliminadas, pendientesNoEliminadas);
        }

       

        // GET: api/Tareas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tarea>>> GetTareas()
        {
            
            return await _context.Tareas.Where(t => !t.IsDeleted).OrderBy(t => t.Id).ToListAsync();
        }

        // GET: api/Tareas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tarea>> GetTarea(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);

            if (tarea == null)
            {
                return NotFound();
            }

            return tarea;
        }

        // POST: api/Tareas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tarea>> PostTarea(Tarea tarea)
        {
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            // Obtener el conteo de tareas y enviar a los clientes
            var (eliminadas, completadasNoEliminadas, pendientesNoEliminadas) = await GetTareasCountAsync();

            var mensaje = new
            {
                Eliminadas = eliminadas,
                CompletadasNoEliminadas = completadasNoEliminadas,
                PendientesNoEliminadas = pendientesNoEliminadas
            };

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", mensaje);


            return CreatedAtAction("GetTarea", new { id = tarea.Id }, tarea);
        }

        // PUT: api/Tareas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTarea(int id, Tarea tarea)
        {
            if (id != tarea.Id)
            {
                return BadRequest();
            }

            _context.Entry(tarea).State = EntityState.Modified;

            try
            {
                
                await _context.SaveChangesAsync();

                // Obtener el conteo de tareas y enviar a los clientes
                var (eliminadas, completadasNoEliminadas, pendientesNoEliminadas) = await GetTareasCountAsync();

                var mensaje = new
                {
                    Eliminadas = eliminadas,
                    CompletadasNoEliminadas = completadasNoEliminadas,
                    PendientesNoEliminadas = pendientesNoEliminadas
                };

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", mensaje);


            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TareaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        

        // DELETE: api/Tareas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null || tarea.IsDeleted)
            {
                return NotFound();
            }

            tarea.IsDeleted = true;
            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();

            // Obtener el conteo de tareas y enviar a los clientes
            var (eliminadas, completadasNoEliminadas, pendientesNoEliminadas) = await GetTareasCountAsync();

            var mensaje = new
            {
                Eliminadas = eliminadas,
                CompletadasNoEliminadas = completadasNoEliminadas,
                PendientesNoEliminadas = pendientesNoEliminadas
            };

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", mensaje);

            return NoContent();
        }

        private bool TareaExists(int id)
        {
            return _context.Tareas.Any(e => e.Id == id);
        }
    }
}
