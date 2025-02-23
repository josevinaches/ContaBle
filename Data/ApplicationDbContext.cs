using ContaBle.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ContaBle.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configuración para la entidad Company: asegura que el nombre sea único
            builder.Entity<Company>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<Person>()
        .Property(p => p.Dni)
        .HasMaxLength(50) // Limita la longitud
        .HasColumnType("nvarchar(50)"); // Se usará nvarchar(50) en SQL Server

            base.OnModelCreating(builder);
            builder.Entity<Person>()
                .HasOne(p => p.User)
                .WithMany(u => u.Persons)
                .HasForeignKey(p => p.UserId);

            builder.Entity<Person>()
             .Property(p => p.CuotaFester)
              .HasColumnType("decimal(18,2)");

            builder.Entity<Person>()
                .Property(p => p.CuotaSocio)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Company>().HasData(
           new Company { Id = 1, Name = "Piratas Berberiscos" },
           new Company { Id = 2, Name = "Tuaregs" },
           new Company { Id = 3, Name = "Moros del Riff" },
           new Company { Id = 4, Name = "Negres" },
           new Company { Id = 5, Name = "Artillería del Islam" },
           new Company { Id = 6, Name = "Moros de Capeta" },
           new Company { Id = 7, Name = "Moros Mercaders" },
           new Company { Id = 8, Name = "Artillería Mora" },
           new Company { Id = 9, Name = "Moros Pak-kos" },
           new Company { Id = 10, Name = "Guardia Negra" },
           new Company { Id = 11, Name = "Beduins" },
           new Company { Id = 12, Name = "Artillería Cristiana" },
           new Company { Id = 13, Name = "Piratas Corsarios" },
           new Company { Id = 14, Name = "Contrabandistas" },
           new Company { Id = 15, Name = "Pescadors" },
           new Company { Id = 16, Name = "Caçadors" },
           new Company { Id = 17, Name = "Catalans" },
           new Company { Id = 18, Name = "Llauradors" },
           new Company { Id = 19, Name = "Marinos" },
           new Company { Id = 20, Name = "Destralers" },
           new Company { Id = 21, Name = "Voluntaris" },
           new Company { Id = 22, Name = "Almogàvers" }
               );

        }

        // Constructor sin parámetros
        public ApplicationDbContext()
        {
        }

        // Constructor con parámetros
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


    }
}