using megadeliciasapi.Models;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Data
{
     
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 2. REGISTRAR  16 TABLAS
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<EstadoOrden> EstadosOrden { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<DetalleOrden> DetalleOrdenes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<InventarioItem> InventarioItems { get; set; }
        public DbSet<PlatoIngrediente> PlatoIngredientes { get; set; }
        public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        public DbSet<CierreCaja> CierresCaja { get; set; }


        // 3. CONFIGURACIÓN DE RELACIONES
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relaciones 1-a-1 (Pago -> Factura y Pago -> MovimientoCaja) ---
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Factura)
                .WithOne(f => f.Pago)
                .HasForeignKey<Factura>(f => f.PagoId);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.MovimientoCaja)
                .WithOne(m => m.Pago)
                .HasForeignKey<MovimientoCaja>(m => m.PagoId);


           
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction); 

            
            modelBuilder.Entity<Orden>()
                .HasOne(o => o.Usuario)
                .WithMany(u => u.Ordenes)
                .HasForeignKey(o => o.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction); 

           
            modelBuilder.Entity<CierreCaja>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.CierresCaja)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            
            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MovimientosCaja)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}