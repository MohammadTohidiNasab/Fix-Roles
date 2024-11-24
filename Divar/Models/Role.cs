public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<AccessLevel> Permissions { get; set; }
}
