using System.ComponentModel.DataAnnotations;

namespace GestionEtudiants.Models
{
    public class Classe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la classe est obligatoire")]
        public string Nom { get; set; } = string.Empty;

        public virtual ICollection<Matiere> Matieres { get; set; } = new List<Matiere>();

        public ICollection<Etudiant> Etudiants { get; set; } = new List<Etudiant>();
    }
}
