using System.ComponentModel.DataAnnotations;

namespace StoreApi.Infra.DTOs {
    public class BookDto {
        [Required(ErrorMessage = "Il titolo è obbligatorio.")]
        [StringLength(300, ErrorMessage = "Il titolo può contenere al massimo 300 caratteri.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'autore è obbligatorio.")]
        [StringLength(200, ErrorMessage = "L'autore può contenere al massimo 200 caratteri.")]
        public string Author { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La categoria indicata non è valida.")]
        public int? CategoryId { get; set; }
    }
}
