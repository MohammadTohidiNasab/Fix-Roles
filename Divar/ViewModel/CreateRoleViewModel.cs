public class CreateRoleViewModel
{
    public string RoleName { get; set; }
    public List<AccessLevel> Permissions { get; set; } = new List<AccessLevel>();
}
