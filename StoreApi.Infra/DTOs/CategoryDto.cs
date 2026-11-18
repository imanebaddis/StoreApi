using System.ComponentModel.DataAnnotations;

namespace StoreApi.Infra.DTOs;

public class CategoryDto
{
    [Required(ErrorMessage = "Il nome della categoria è obbligatorio.")]
    [StringLength(80, ErrorMessage = "Il nome della categoria può contenere al massimo 80 caratteri.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descrizione può contenere al massimo 500 caratteri.")]
    public string? Description { get; set; }
}
