namespace Divar.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }
        [MaxLength(400)]
        public string Body { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
