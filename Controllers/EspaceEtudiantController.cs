using GestionEtudiants.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SkiaSharp;


[Authorize(Roles = "Étudiant")]
public class EspaceEtudiantController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public EspaceEtudiantController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // 🧠 Récupérer l'étudiant lié à l'utilisateur connecté (Email)
        var etudiant = await _context.Etudiants
            .Include(e => e.Classe)
            .FirstOrDefaultAsync(e => e.Email == user.Email);

        if (etudiant == null)
            return NotFound("Étudiant non trouvé");

        // 📚 Récupérer les notes de l'étudiant avec les matières
        var notes = await _context.Notes
            .Where(n => n.EtudiantId == etudiant.Id)
            .Include(n => n.Matiere)
            .ToListAsync();

        // 🧮 Calcul de la moyenne pondérée
        double somme = 0;
        double totalCoef = 0;

        foreach (var note in notes)
        {
            if (note.Matiere != null)
            {
                somme += note.Valeur * note.Matiere.Coefficient;
                totalCoef += note.Matiere.Coefficient;
            }
        }

        double moyenne = totalCoef > 0 ? somme / totalCoef : 0;

        return View((etudiant, notes, moyenne));
    }


    [Authorize(Roles = "Étudiant")]
    public async Task<IActionResult> Releve()
    {
        var user = await _userManager.GetUserAsync(User);

        var etudiant = await _context.Etudiants
            .Include(e => e.Classe)
            .FirstOrDefaultAsync(e => e.Email == user.Email);

        var notes = _context.Notes
            .Include(n => n.Matiere)
            .Where(n => n.EtudiantId == etudiant.Id)
            .ToList();

        return View(notes);
    }

    // ✅ Action du bouton Télécharger


    [Authorize(Roles = "Étudiant")]
    public async Task<IActionResult> TelechargerReleve()
    {
        var user = await _userManager.GetUserAsync(User);

        var etudiant = await _context.Etudiants
            .Include(e => e.Notes)
            .ThenInclude(n => n.Matiere)
            .FirstOrDefaultAsync(e => e.Email == user.Email);

        if (etudiant == null) return NotFound();

        // Calcul moyenne
        double totalPoints = 0;
        double totalCoeff = 0;
        foreach (var note in etudiant.Notes)
        {
            totalPoints += note.Valeur * note.Matiere.Coefficient;
            totalCoeff += note.Matiere.Coefficient;
        }
        double moyenne = totalCoeff > 0 ? totalPoints / totalCoeff : 0;

        // Génération PDF
        MemoryStream workStream = new MemoryStream();
        Document doc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
        PdfWriter.GetInstance(doc, workStream).CloseStream = false;

        doc.Open();
        doc.Add(new Paragraph($"🎓 Relevé de Notes - {etudiant.Nom} {etudiant.Prenom}"));
        doc.Add(new Paragraph("\n"));

        PdfPTable table = new PdfPTable(4);
        table.WidthPercentage = 100;

        table.AddCell("Matière");
        table.AddCell("Note");
        table.AddCell("Coefficient");
        table.AddCell("Note Pondérée");

        foreach (var note in etudiant.Notes)
        {
            table.AddCell(note.Matiere.Nom);
            table.AddCell(note.Valeur.ToString("0.00"));
            table.AddCell(note.Matiere.Coefficient.ToString());
            table.AddCell((note.Valeur * note.Matiere.Coefficient).ToString("0.00"));
        }

        doc.Add(table);
        doc.Add(new Paragraph($"\n📊 Moyenne Générale : {moyenne:F2}"));
        doc.Close();

        byte[] byteInfo = workStream.ToArray();
        return File(byteInfo, "application/pdf", $"Releve_{etudiant.Nom}_{etudiant.Prenom}.pdf");
    }

    [HttpPost]
    [Authorize(Roles = "Étudiant")]
    public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var result = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);

        if (result.Succeeded)
        {
            TempData["Success"] = "✅ Mot de passe modifié avec succès.";
            return RedirectToAction("Profil"); // 👈 Redirection vers la même vue
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View("Profil"); // 👈 Pour rester sur la même vue en cas d'erreur
    }

    [Authorize(Roles = "Étudiant")]
    public async Task<IActionResult> Profil()
    {
        var user = await _userManager.GetUserAsync(User);

        var etudiant = await _context.Etudiants
            .Include(e => e.Classe)
            .FirstOrDefaultAsync(e => e.Email == user.Email);

        if (etudiant == null)
            return NotFound();

        return View(etudiant);
    }



}
