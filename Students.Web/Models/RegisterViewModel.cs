using System.ComponentModel.DataAnnotations;

namespace Students.Web.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Логин")]
        public string Login { get; set; }
    }
}