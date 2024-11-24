public class Role : IdentityRole
{
    public ICollection<AccessLevel> Permissions { get; set; }
}
