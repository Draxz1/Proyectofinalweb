using megadeliciasapi.Models;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // REGISTRAR TABLAS
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
        
        // ⭐ NUEVO: Solo si vas a crear tabla separada
        public DbSet<Producto> Productos { get; set; }


        // CONFIGURACIÓN DE RELACIONES
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relaciones 1-a-1 (Pago -> Factura y Pago -> MovimientoCaja) ---
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Factura)
                .WithOne(f => f.Pago)
                .HasForeignKey<Factura>(f => f.PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.MovimientoCaja)
                .WithOne(m => m.Pago)
                .HasForeignKey<MovimientoCaja>(m => m.PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Relaciones con Usuario (evitar cascadas múltiples) ---
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Orden>()
                .HasOne(o => o.Usuario)
                .WithMany(u => u.Ordenes)
                .HasForeignKey(o => o.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CierreCaja>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.CierresCaja)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MovimientosCaja)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Configuración de Productos (tabla nueva) ---
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Categoria).IsRequired().HasMaxLength(50);
                entity.Property(p => p.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Activo).HasDefaultValue(true);
                entity.Property(p => p.FechaCreacion).HasDefaultValueSql("GETDATE()");
                
                entity.HasIndex(p => p.Nombre);
                entity.HasIndex(p => p.Categoria);
            });
        }
    }
}