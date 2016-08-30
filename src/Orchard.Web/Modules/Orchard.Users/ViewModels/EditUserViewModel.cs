namespace Orchard.Users.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public RoleEntry[] Roles { get; set; }
    }

    public class RoleEntry
    {
        public string Role { get; set; }
        public bool IsSelected { get; set; }
    }
}
