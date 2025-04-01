using GestionEtudiants.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GestionEtudiants.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? matiereId = null, int? etudiantId = null)

        {
            // ✅ Nombre total d'étudiants
            int nombreEtudiants = await _context.Etudiants.CountAsync();

            // ✅ Nombre total de matières
            int nombreMatieres = await _context.Matieres.CountAsync();

            // 📊 Répartition des matières par classe (format compatible JSON)
            var matieresParClasse = await _context.Classes
                .Include(c => c.Matieres)
                .Select(c => new
                {
                    ClasseNom = c.Nom,
                    NbMatieres = c.Matieres.Count
                })
                .ToListAsync();

            ViewBag.MatieresParClasse = matieresParClasse;


            // 📤 Envoi des données à la vue via ViewBag
            ViewBag.NombreEtudiants = nombreEtudiants;
            ViewBag.NombreMatieres = nombreMatieres;

            var topEtudiants = await _context.Etudiants
    .Include(e => e.Classe)
    .Include(e => e.Notes)
        .ThenInclude(n => n.Matiere)
    .ToListAsync();

            var classement = topEtudiants
                .Where(e => e.Notes.Any())
                .Select(e => new
                {
                    NomComplet = $"{e.Nom} {e.Prenom}",
                    Classe = e.Classe?.Nom,
                    Moyenne = Math.Round(
                        e.Notes.Sum(n => n.Valeur * (n.Matiere?.Coefficient ?? 1)) /
                        e.Notes.Sum(n => (n.Matiere?.Coefficient ?? 1)), 2)
                })
                .OrderByDescending(e => e.Moyenne)
                .Take(5) // 🔝 top 5
                .ToList();

            ViewBag.Classement = classement;

            // 📥 Envoyer la liste des matières et étudiants pour les dropdowns
            ViewBag.Matieres = await _context.Matieres.ToListAsync();
            ViewBag.Etudiants = await _context.Etudiants.Include(e => e.Classe).ToListAsync();
            ViewBag.NombreClasses = await _context.Classes.CountAsync();


            // 📊 Calcul des moyennes pour le diagramme circulaire
            var moyennesStats = new { Inferieure10 = 0, Entre10et12 = 0, Superieure12 = 0 };

            var moyennesEtudiants = topEtudiants
                .Where(e => e.Notes.Any())
                .Select(e => e.Notes.Sum(n => n.Valeur * (n.Matiere?.Coefficient ?? 1)) /
                             e.Notes.Sum(n => (n.Matiere?.Coefficient ?? 1)))
                .ToList();

            moyennesStats = new
            {
                Inferieure10 = moyennesEtudiants.Count(m => m < 10),
                Entre10et12 = moyennesEtudiants.Count(m => m >= 10 && m <= 12),
                Superieure12 = moyennesEtudiants.Count(m => m > 12)
            };

            ViewBag.MoyennesStats = moyennesStats;


            return View();
        }
    }
}
