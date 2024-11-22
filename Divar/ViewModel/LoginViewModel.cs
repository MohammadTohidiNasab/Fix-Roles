// LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Divar.Models
{
    public class LoginViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "لطفا ایمیل خود را وارد کنید")]
        public string Email { get; set; }

        [Required(ErrorMessage = "لطفا پسورد را وارد کنید")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}