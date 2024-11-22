namespace Divar.ViewModel
{
    public class AdminPanel
    {
        public List<Advertisement> Advertisements { get; set; }
        public List<Comment> Comments { get; set; }
        public List<CustomUser> Users { get; internal set; }
    }
}
