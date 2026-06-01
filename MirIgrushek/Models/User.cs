namespace MirIgrushek.Models
{
    /// <summary>
    /// Пользователь системы (сотрудник или клиент) с указанием роли.
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
