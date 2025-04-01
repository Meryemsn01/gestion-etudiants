using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionEtudiants.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Etudiant> Etudiants { get; set; }
        public DbSet<Matiere> Matieres { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Classe> Classes { get; set; }
        



    }
}
