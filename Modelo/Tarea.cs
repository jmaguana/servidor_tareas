namespace WebApplication2.Modelo
{
    public class Tarea
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Completada { get; set; }
        public bool IsDeleted { get; set; }
    }
}
