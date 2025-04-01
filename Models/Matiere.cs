using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionEtudiants.Models
{
    public class Matiere
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le coefficient est obligatoire")]
        public int Coefficient { get; set; } = 1;

        [Required]
        [ForeignKey("Classe")]
        public int ClasseId { get; set; }

        public virtual Classe? Classe { get; set; }

        public List<Note> Notes { get; set; } = new();

    }
}
