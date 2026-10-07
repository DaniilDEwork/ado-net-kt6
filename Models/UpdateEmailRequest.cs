using System.ComponentModel.DataAnnotations;

namespace Kt6UpdateApi.Models;

public class UpdateEmailRequest
{
    [Required(ErrorMessage = "Укажите текущее имя пользователя")]
    [StringLength(100, ErrorMessage = "Имя должно содержать не больше 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите электронную почту")]
    [StringLength(100, ErrorMessage = "Почта должна содержать не больше 100 символов")]
    [EmailAddress(ErrorMessage = "Укажите правильный адрес электронной почты")]
    public string Email { get; set; } = string.Empty;
}
