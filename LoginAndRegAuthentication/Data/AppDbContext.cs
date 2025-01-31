using Microsoft.EntityFrameworkCore;
using AuthJwtApi.Models.Entities;

namespace AuthJwtApi.Data
{
    public class AppDbContext : DbContext
    {
        // DbSet properties for each entity
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Constructor to pass options to the base DbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Configure relationships and seed data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship for UserRoles
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId }); // Composite primary key

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User) // One User
                .WithMany(u => u.UserRoles) // Has many UserRoles
                .HasForeignKey(ur => ur.UserId); // Foreign key to User

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role) // One Role
                .WithMany(r => r.UserRoles) // Has many UserRoles
                .HasForeignKey(ur => ur.RoleId); // Foreign key to Role

            // Configure one-to-many relationship for RefreshTokens
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User) // One User
                .WithMany(u => u.RefreshTokens) // Has many RefreshTokens
                .HasForeignKey(rt => rt.UserId); // Foreign key to User

            // Seed initial roles
            modelBuilder.Entity<Role>().HasData(        
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Admin" }
            );
        }
    }
}