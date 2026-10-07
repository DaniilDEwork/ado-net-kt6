using System.ComponentModel.DataAnnotations;

namespace Kt6UpdateApi.Models;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100, ErrorMessage = "Имя должно содержать не больше 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите электронную почту")]
    [StringLength(100, ErrorMessage = "Почта должна содержать не больше 100 символов")]
    [EmailAddress(ErrorMessage = "Укажите правильный адрес электронной почты")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите возраст")]
    [Range(0, 120, ErrorMessage = "Возраст должен быть от 0 до 120")]
    public int? Age { get; set; }
}
