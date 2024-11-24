public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string Permission { get; set; }

    public Role Role { get; set; }
}
