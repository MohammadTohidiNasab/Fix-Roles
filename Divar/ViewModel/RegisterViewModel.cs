// RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Divar.Models
{
    public class RegisterViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "لطفا ایمیل خود را وارد کنید")]
        public string Email { get; set; }

        [Required(ErrorMessage = "لطفا نام خود را وارد کنید")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "لطفا نام خانوادگی خود را وارد کنید")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "لطفا شماره تلفن خود را وارد کنید")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "لطفا پسورد را وارد کنید")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "پسوردها مطابقت ندارند")]
        public string ConfirmPassword { get; set; }
    }
}