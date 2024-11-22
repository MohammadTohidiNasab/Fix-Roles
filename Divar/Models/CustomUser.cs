namespace Divar.Models
{
    public class CustomUser : IdentityUser
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "وارد کردن نام الزامی است")]
        public string ?FirstName { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "وارد کردن نام خانوادگی الزامی است")]
        public string ?LastName { get; set; }

        [MaxLength(12)]
        [Required(ErrorMessage = "وارد کردن شماره تلفن الزامی است")]
        public string ?PhoneNumber { get; set; }

        public ICollection<Advertisement>? Advertisements { get; set; }
        public ICollection<Role> ?Roles { get; set; }

    }
}
