using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionEtudiants.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using SkiaSharp;
using System.IO;
using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GestionEtudiants.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EtudiantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EtudiantsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Etudiants
        // GET: Etudiants
        public async Task<IActionResult> Index(int? classeId = null)
        {
            // ✅ Récupérer toutes les classes (pour le menu de filtrage)
            var classes = await _context.Classes
                .OrderBy(c => c.Nom)
                .ToListAsync();

            ViewBag.Classes = classes;
            ViewBag.ClasseSelectionnee = classeId;

            // ✅ Si une classe est sélectionnée, on affiche uniquement ses étudiants
            if (classeId.HasValue)
            {
                var classe = await _context.Classes
                    .Include(c => c.Etudiants)
                        .ThenInclude(e => e.Notes)
                            .ThenInclude(n => n.Matiere)
                    .FirstOrDefaultAsync(c => c.Id == classeId.Value);

                if (classe == null)
                {
                    return NotFound();
                }

                return View(new List<Classe> { classe });
            }

            // ✅ Sinon, ne pas afficher les étudiants pour toutes les classes (page plus légère)
            return View(new List<Classe>());
        }
        // ✅ Action pour charger dynamiquement les étudiants
        public async Task<IActionResult> GetEtudiantsParClasse(int classeId)
        {
            var classe = await _context.Classes
                .Include(c => c.Etudiants)
                .FirstOrDefaultAsync(c => c.Id == classeId);

            if (classe == null) return NotFound();

            return PartialView("_EtudiantsParClasse", classe);
        }

        // GET: Etudiants/Details/5
        public async Task<IActionResult> Details(int? id, int? classeId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var etudiant = await _context.Etudiants
                .Include(e => e.Classe) // ✅ Charger la classe
                .FirstOrDefaultAsync(m => m.Id == id);

            if (etudiant == null)
            {
                return NotFound();
            }
            ViewBag.ClasseSelectionnee = classeId;
            return View(etudiant);
        }
        // GET: Etudiants/Create
        public IActionResult Create(int? classeId)
        {
            ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom");

            ViewBag.ClasseId = classeId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Etudiant etudiant, IFormFile? PhotoFile, int? classeId)
        {
            if (_context.Etudiants.Any(e => e.CIN == etudiant.CIN))
            {
                ModelState.AddModelError("CIN", "Ce CIN est déjà utilisé.");
            }
            if (_context.Etudiants.Any(e => e.CodeApogee == etudiant.CodeApogee))
            {
                ModelState.AddModelError("CodeApogee", "Ce Code Apogée est déjà utilisé.");
            }

            if (!ModelState.IsValid)
            {
                // **🔥 Recharge la liste des classes si une erreur est présente**
                ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom");
                ViewBag.ClasseId = classeId;
                return View(etudiant);
            }

            // Gestion de l’upload de la photo (Garde ton code existant ici)
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PhotoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(fileStream);
                }

                etudiant.Photo = "/uploads/" + uniqueFileName;
            }
            else
            {
                etudiant.Photo = "/uploads/default.png";
            }

            _context.Add(etudiant);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { classeId = classeId });
        }
        // GET: Etudiants/Edit/5
        public async Task<IActionResult> Edit(int? id, int? classeId)
        {
            if (id == null) return NotFound();

            var etudiant = await _context.Etudiants.FindAsync(id);
            if (etudiant == null) return NotFound();

            // ✅ Charger la liste des classes pour la dropdown
            ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom", etudiant.ClasseId);

            ViewBag.ClasseId = classeId;
            return View(etudiant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Etudiant etudiant, IFormFile? PhotoFile, int? classeId)
        {
            if (id != etudiant.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Récupérer l'étudiant actuel en base
                    var etudiantDb = await _context.Etudiants.FindAsync(id);
                    if (etudiantDb == null) return NotFound();

                    // ✅ Mise à jour des données
                    etudiantDb.Nom = etudiant.Nom;
                    etudiantDb.Prenom = etudiant.Prenom;
                    etudiantDb.DateNaissance = etudiant.DateNaissance;
                    etudiantDb.Email = etudiant.Email;
                    etudiantDb.Telephone = etudiant.Telephone;
                    etudiantDb.CIN = etudiant.CIN;
                    etudiantDb.CodeApogee = etudiant.CodeApogee;
                    etudiantDb.ClasseId = etudiant.ClasseId; // ✅ Mise à jour de la classe

                    // ✅ Gestion du changement de photo
                    if (PhotoFile != null && PhotoFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PhotoFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await PhotoFile.CopyToAsync(fileStream);
                        }

                        etudiantDb.Photo = "/uploads/" + uniqueFileName;
                    }

                    _context.Update(etudiantDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Etudiants.Any(e => e.Id == etudiant.Id)) return NotFound();
                    else throw;
                }
                ViewBag.ClasseId = classeId;
                return RedirectToAction(nameof(Index), new { classeId = classeId });

            }

            // ✅ Recharger la liste des classes pour la dropdown si erreur
            ViewBag.Classes = new SelectList(_context.Classes, "Id", "Nom", etudiant.ClasseId);
            ViewBag.ClasseId = classeId;
            return View(etudiant);
        }
        // GET: Etudiants/Delete/5
        // GET: Etudiants/Delete/5
        public async Task<IActionResult> Delete(int? id, int? classeId)
        {
            if (id == null) return NotFound();

            var etudiant = await _context.Etudiants
                .Include(e => e.Classe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (etudiant == null) return NotFound();

            ViewBag.ClasseId = classeId;
            return View(etudiant);
        }


        // POST: Etudiants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? classeId)
        {
            var etudiant = await _context.Etudiants.FindAsync(id);
            if (etudiant != null)
            {
                // Supprimer l'image associée sauf si c'est l'image par défaut
                if (!string.IsNullOrEmpty(etudiant.Photo) && etudiant.Photo != "/uploads/default.png")
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, etudiant.Photo.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Etudiants.Remove(etudiant);
                await _context.SaveChangesAsync();
            }
            ViewBag.ClasseId = classeId;
            return RedirectToAction(nameof(Index), new { classeId = classeId });

        }

        private bool EtudiantExists(int id)
        {
            return _context.Etudiants.Any(e => e.Id == id);
        }

        // Vérifie si le fichier est une image valide
        private bool IsImageFile(IFormFile file)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            return validExtensions.Contains(fileExtension);
        }
        public async Task<IActionResult> Moyenne(int id, int? classeId)
        {
            var etudiant = await _context.Etudiants
                .Include(e => e.Classe) // ✅ Charger la classe de l’étudiant
                .Include(e => e.Notes)
                .ThenInclude(n => n.Matiere) // ✅ Charger les matières des notes
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etudiant == null) return NotFound();
            ViewBag.ClasseId = classeId;
            return View(etudiant);
        }
        public async Task<IActionResult> ExportPDF(int id)
            {
                var etudiant = await _context.Etudiants
                    .Include(e => e.Notes)
                    .ThenInclude(n => n.Matiere)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (etudiant == null) return NotFound();

                MemoryStream workStream = new MemoryStream();
                Document doc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
                PdfWriter.GetInstance(doc, workStream).CloseStream = false;

                doc.Open();
                doc.Add(new Paragraph($"Bulletin de Notes de {etudiant.Nom} {etudiant.Prenom}"));
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Matière");
                table.AddCell("Note");
                table.AddCell("Coefficient");
                table.AddCell("Note Pondérée");

                foreach (var note in etudiant.Notes)
                {
                    table.AddCell(note.Matiere.Nom);
                    table.AddCell(note.Valeur.ToString());
                    table.AddCell(note.Matiere.Coefficient.ToString());
                    table.AddCell((note.Valeur * note.Matiere.Coefficient).ToString());
                }

                doc.Add(table);
                doc.Add(new Paragraph($"\nMoyenne Générale : {etudiant.Moyenne}"));
                doc.Close();

                byte[] byteInfo = workStream.ToArray();
                return File(byteInfo, "application/pdf", $"Bulletin_{etudiant.Nom}_{etudiant.Prenom}.pdf");
            }
         // Assure-toi d'avoir ce `using`
    public IActionResult ExportExcel(int id)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // ✅ Ajoute cette ligne avant d'instancier ExcelPackage

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Etudiant");

                // Ajoute des données à Excel
                worksheet.Cells["A1"].Value = "Nom";
                worksheet.Cells["B1"].Value = "Prénom";
                worksheet.Cells["C1"].Value = "Matière";
                worksheet.Cells["D1"].Value = "Note";

                var etudiant = _context.Etudiants
                    .Include(e => e.Notes)
                    .ThenInclude(n => n.Matiere)
                    .FirstOrDefault(e => e.Id == id);

                if (etudiant != null)
                {
                    int row = 2;
                    foreach (var note in etudiant.Notes)
                    {
                        worksheet.Cells[row, 1].Value = etudiant.Nom;
                        worksheet.Cells[row, 2].Value = etudiant.Prenom;
                        worksheet.Cells[row, 3].Value = note.Matiere.Nom;
                        worksheet.Cells[row, 4].Value = note.Valeur;
                        row++;
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Etudiant_{id}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
    }
    }
}
