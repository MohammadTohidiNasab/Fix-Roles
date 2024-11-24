namespace Divar.ViewModel
{
    public class ManageRoleViewModel
    {
        public string CustomUserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<IdentityRole> AllRoles { get; set; }
        public List<string> UserRoles { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
