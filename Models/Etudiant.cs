using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GestionEtudiants.Models
{
    [Index(nameof(CIN), IsUnique = true)]  // 🔥 Assure l'unicité du CIN
    [Index(nameof(CodeApogee), IsUnique = true)]  // 🔥 Assure l'unicité du Code Apogée

    public class Etudiant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Nom { get; set; }

        [Required]
        public required string Prenom { get; set; }

        [Required]
        public int ClasseId { get; set; }  // Clé étrangère vers Classe

        [ForeignKey("ClasseId")]
        public Classe? Classe { get; set; }  // Relation avec Classe

        public DateTime DateNaissance { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        [Phone]
        public required string Telephone { get; set; }

        public string? Photo { get; set; } // Chemin de la photo dans wwwroot/uploads

        [NotMapped]
        public IFormFile? PhotoFile { get; set; } // Permet l’upload depuis le formulaire

        
        public virtual List<Note> Notes { get; set; } = new List<Note>();

        [NotMapped] // Cette propriété ne sera pas stockée en base
        public double Moyenne
        {
            get
            {
                if (Notes == null || !Notes.Any()) return 0; // Si l'étudiant n'a pas de notes, moyenne = 0

                double sommeNotes = Notes?.Sum(n => n.Valeur * (n.Matiere?.Coefficient ?? 1)) ?? 0;
                double sommeCoefficients = Notes?.Sum(n => n.Matiere?.Coefficient ?? 1) ?? 0;

                return sommeCoefficients > 0 ? Math.Round(sommeNotes / sommeCoefficients, 2) : 0;
            }
        }



        [Display(Name = "CIN (si applicable)")]
        public string? CIN { get; set; }

        [Display(Name = "Code Apogée (si applicable)")]
        public string? CodeApogee { get; set; }

        // Validation personnalisée pour s'assurer qu'un étudiant a au moins un identifiant unique
        public bool HasValidIdentifier()
        {
            return !string.IsNullOrEmpty(CIN) || !string.IsNullOrEmpty(CodeApogee);
        }



    }

   
}
