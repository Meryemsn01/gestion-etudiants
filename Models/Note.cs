using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionEtudiants.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Etudiant")]
        public int EtudiantId { get; set; }

        public Etudiant? Etudiant { get; set; } // Relation avec l'étudiant

        [Required]
        [ForeignKey("Matiere")]
        public int MatiereId { get; set; }

        public Matiere? Matiere { get; set; } // Relation avec la matière

        [Required]
        [Range(0, 20)]
        public double Valeur { get; set; }
    }
}
