using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreatePersonDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age is required")]
    [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Sex is required")]
    [StringLength(20)]
    public string Sex { get; set; } = string.Empty;

    [Required(ErrorMessage = "Number is required")]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class PersonResponseDto
{
    public int Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class IdempotentApiResponse<T>
{
    public bool Success { get; set; } = true;
    public bool IsReplay { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
