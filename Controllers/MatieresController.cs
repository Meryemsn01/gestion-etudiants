using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionEtudiants.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestionEtudiants.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MatieresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatieresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matieres

        public IActionResult Index(int? classeId)
        {
            ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom");
            ViewBag.ClasseSelectionnee = classeId;
            return View();
        }


        // GET: Matieres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matiere = await _context.Matieres
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matiere == null)
            {
                return NotFound();
            }

            return View(matiere);
        }



        public IActionResult Create(int classeId)
        {
            var classe = _context.Classes.Find(classeId);
            if (classe == null) return NotFound();

            var matiere = new Matiere
            {
                ClasseId = classeId,
                Classe = classe, // ✅ Initialise la classe
                Nom = string.Empty // ✅ Evite les erreurs de champ vide
            };

            ViewBag.ClasseNom = classe.Nom;
            ViewBag.ClasseId = classeId;

            return View(matiere);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Coefficient,ClasseId")] Matiere matiere)
        {
            Console.WriteLine("🚀 Création d'une nouvelle matière...");
            Console.WriteLine($"📌 Nom reçu : {matiere.Nom}");
            Console.WriteLine($"📌 Coefficient reçu : {matiere.Coefficient}");
            Console.WriteLine($"📌 ClasseId reçu : {matiere.ClasseId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState invalide !");
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Erreur : {error.ErrorMessage}");
                }

                return View(matiere);
            }

            try
            {
                Console.WriteLine("✅ Ajout de la matière à la base de données...");
                _context.Add(matiere);
                await _context.SaveChangesAsync();
                Console.WriteLine("🎉 Matière enregistrée avec succès !");
                return RedirectToAction(nameof(Index), new { classeId = matiere.ClasseId });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Erreur lors de l'ajout de la matière : {ex.Message}");
                return View(matiere);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matiere = await _context.Matieres.FindAsync(id);
            if (matiere == null)
            {
                return NotFound();
            }

            ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom", matiere.ClasseId);
            return View(matiere);
        }




        // POST: Matieres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Coefficient,ClasseId")] Matiere matiere)
        {
            if (id != matiere.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(matiere);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { classeId = matiere.ClasseId });

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Matieres.Any(m => m.Id == matiere.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom", matiere.ClasseId);
            return View(matiere);
        }



        // GET: Matieres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matiere = await _context.Matieres
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matiere == null)
            {
                return NotFound();
            }

            return View(matiere);
        }

        // POST: Matieres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var matiere = await _context.Matieres.FindAsync(id);
            if (matiere != null)
            {
                _context.Matieres.Remove(matiere);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatiereExists(int id)
        {
            return _context.Matieres.Any(e => e.Id == id);
        }

        public IActionResult GetMatieresParClasse(int classeId)
        {
            var classe = _context.Classes
                .Include(c => c.Matieres)
                .FirstOrDefault(c => c.Id == classeId);

            if (classe == null)
                return NotFound();

            return PartialView("_MatieresParClasse", classe);
        }

       

    }

}
