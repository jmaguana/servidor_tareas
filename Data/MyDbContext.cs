

namespace WebApplication2.Data
{
    using Microsoft.EntityFrameworkCore;
    using WebApplication2.Modelo;

    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Tarea> Tareas { get; set; }
    }
}
