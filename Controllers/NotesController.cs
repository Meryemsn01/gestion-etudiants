using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionEtudiants.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GestionEtudiants.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? classeId)
        {
            var classes = _context.Classes.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nom
            }).ToList();

            ViewBag.ClasseSelectionnee = classeId;
            return View(classes);
        }

        public IActionResult GetNotesParClasse(int classeId)
        {
            var classe = _context.Classes
                .Include(c => c.Matieres)
                    .ThenInclude(m => m.Notes)
                        .ThenInclude(n => n.Etudiant)
                .FirstOrDefault(c => c.Id == classeId);

            if (classe == null)
                return NotFound();

            ViewBag.ClasseId = classeId; // Pour le bouton "Ajouter une note"
            return PartialView("_NotesParClasse", classe);
        }

        public IActionResult Create(int matiereId, int? classeId)
        {
            var matiere = _context.Matieres
                .Include(m => m.Classe)
                .FirstOrDefault(m => m.Id == matiereId);

            if (matiere == null)
                return NotFound();

            // 🔍 Récupérer les IDs des étudiants ayant déjà une note pour cette matière
            var etudiantsAvecNote = _context.Notes
                .Where(n => n.MatiereId == matiereId)
                .Select(n => n.EtudiantId)
                .ToList();

            // ✅ Étudiants de la classe qui n'ont pas encore de note pour cette matière
            var etudiants = _context.Etudiants
                .Where(e => e.ClasseId == matiere.ClasseId && !etudiantsAvecNote.Contains(e.Id))
                .ToList();

                ViewBag.Etudiants = new SelectList(
                    etudiants.Select(e => new { Id = e.Id, NomComplet = e.Nom + " " + e.Prenom }),
                    "Id", "NomComplet"
                );

            ViewBag.MatiereId = matiereId;
            ViewBag.ClasseId = classeId ?? matiere.ClasseId;

            return View();
        }



        [HttpPost]
        public IActionResult Create(Note note)
        {
            // ✅ Vérifie si une note existe déjà pour cet étudiant et cette matière
            bool existe = _context.Notes.Any(n =>
                n.EtudiantId == note.EtudiantId &&
                n.MatiereId == note.MatiereId);

            if (existe)
            {
                ModelState.AddModelError("", "❌ Cet étudiant a déjà une note pour cette matière.");
            }

            if (ModelState.IsValid)
            {
                _context.Notes.Add(note);
                _context.SaveChanges();

                var classeId = _context.Matieres
                    .Where(m => m.Id == note.MatiereId)
                    .Select(m => m.ClasseId)
                    .FirstOrDefault();

                return RedirectToAction("Index", new { classeId });
            }

            // 🔄 Recharge les étudiants pour le formulaire (si erreur)
            var matiere = _context.Matieres
                .Include(m => m.Classe)
                .FirstOrDefault(m => m.Id == note.MatiereId);

            if (matiere != null)
            {
                var etudiants = _context.Etudiants
                    .Where(e => e.ClasseId == matiere.ClasseId)
                    .ToList();

                ViewBag.Etudiants = new SelectList(etudiants, "Id", "Nom", note.EtudiantId);
                ViewBag.ClasseId = matiere.ClasseId;
            }

            ViewBag.MatiereId = note.MatiereId;
            return View(note);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var note = await _context.Notes
                .Include(n => n.Matiere)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
                return NotFound();

            var classeId = note.Matiere.ClasseId;
            ViewBag.ClasseId = classeId;

            var etudiants = _context.Etudiants
                .Where(e => e.ClasseId == classeId)
                .ToList();

            ViewBag.Etudiants = new SelectList(etudiants, "Id", "Nom", note.EtudiantId);
            ViewBag.Matieres = new SelectList(_context.Matieres.Where(m => m.ClasseId == classeId), "Id", "Nom", note.MatiereId);

            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EtudiantId,MatiereId,Valeur")] Note note)
        {
            if (id != note.Id)
                return NotFound();

            var classeId = _context.Matieres.Where(m => m.Id == note.MatiereId).Select(m => m.ClasseId).FirstOrDefault();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { classeId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }

            var etudiants = _context.Etudiants
                .Where(e => e.ClasseId == classeId)
                .ToList();

            ViewBag.Etudiants = new SelectList(etudiants, "Id", "Nom", note.EtudiantId);
            ViewBag.Matieres = new SelectList(_context.Matieres.Where(m => m.ClasseId == classeId), "Id", "Nom", note.MatiereId);
            ViewBag.ClasseId = classeId;

            return View(note);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var note = await _context.Notes.Include(n => n.Etudiant).Include(n => n.Matiere).FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
                return NotFound();

            return View(note);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var note = await _context.Notes
                .Include(n => n.Etudiant)
                .Include(n => n.Matiere)
                .ThenInclude(m => m.Classe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (note == null)
                return NotFound();

            ViewBag.ClasseId = note.Matiere.ClasseId;
            return View(note);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes
                .Include(n => n.Matiere)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note != null)
            {
                var classeId = note.Matiere.ClasseId;
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { classeId });
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
