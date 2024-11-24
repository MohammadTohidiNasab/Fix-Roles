public class RolePermission
{
    public int Id { get; set; }
    public string RoleId { get; set; } // لینک به IdentityRole
    public AccessLevel Permission { get; set; } // نوع مجوز
}
