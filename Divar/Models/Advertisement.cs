namespace Divar.Models
{
    public enum CategoryType
    {
        کتاب,
        خانه,
        موبایل,
        ماشین
    }

    public class Advertisement
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "افزودن تیتر الزامی است")]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Content { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }

        [Required(ErrorMessage = "افزودن قیمت محصول الزامی است")]
        public int Price { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required(ErrorMessage = "افزودن دسته بندی محصول الزامی است")]
        public CategoryType Category { get; set; }

        public string? CustomUserId { get; set; } // Foreign key to User
        public CustomUser? CustomUser { get; set; }

        public string? MobileBrand { get; set; }
        public int SimCardsNumber { get; set; }

        public int? HomeSize { get; set; }
        public string? HomeAddress { get; set; }

        public string? BookAuthor { get; set; }

        public string? CarBrand { get; set; }
        public bool? GearboxType { get; set; }
    }
}
