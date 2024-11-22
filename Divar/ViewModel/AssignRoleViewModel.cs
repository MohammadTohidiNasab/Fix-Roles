using Microsoft.AspNetCore.Mvc.Rendering;

public class AssignRoleViewModel
{
    public string UserId { get; set; }  // Make sure to keep UserId
    public List<SelectListItem> Users { get; set; } // New property for users
    public List<string> AvailableRoles { get; set; }
    public List<string> SelectedRoles { get; set; } = new List<string>();
}
