using System.ComponentModel.DataAnnotations;

namespace Students.Web.Models
{
    public class LoginViewModel : RegisterViewModel
    {
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}