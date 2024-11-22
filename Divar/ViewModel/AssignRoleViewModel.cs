public class AssignRoleViewModel
{
    public string UserId { get; set; }
    public List<string> AvailableRoles { get; set; }
    public List<string> SelectedRoles { get; set; } = new List<string>();
}
