using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

namespace WebApplication2.Controllers
{
    public class TareasHub : Hub
    {
        private readonly MyDbContext _context;

        public TareasHub(MyDbContext context)
        {
            _context = context;
        }

        public async Task EnviarConteo()
        {
            var eliminadas = await _context.Tareas.CountAsync(t => t.IsDeleted);
            var completadasNoEliminadas = await _context.Tareas.CountAsync(t => !t.IsDeleted && t.Completada);
            var pendientesNoEliminadas = await _context.Tareas.CountAsync(t => !t.IsDeleted && !t.Completada);

            var mensaje = new
            {
                Eliminadas = eliminadas,
                CompletadasNoEliminadas = completadasNoEliminadas,
                PendientesNoEliminadas = pendientesNoEliminadas
            };

            await Clients.All.SendAsync("ReceiveMessage", mensaje);
        }

        public override async Task OnConnectedAsync()
        {
            // Envía el conteo cuando un cliente se conecta
            await EnviarConteo();
            await base.OnConnectedAsync();
        }

        public async Task EnviarMensaje(string message)
        {
            // Enviar una notificación a todos los clientes conectados
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
        
    }
}
