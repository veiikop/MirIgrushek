using MirIgrushek.Models;

namespace MirIgrushek.Services
{
    /// <summary>
    /// Хранит текущего вошедшего пользователя на время работы приложения.
    /// Для гостя CurrentUser остаётся null.
    /// </summary>
    public static class Session
    {
        public static User? CurrentUser { get; set; }

        /// <summary>
        /// Признак гостевого режима (пользователь не авторизован).
        /// </summary>
        public static bool IsGuest => CurrentUser is null;

        /// <summary>
        /// ФИО для вывода в правом верхнем углу окон. Для гостя — "Гость".
        /// </summary>
        public static string DisplayName => CurrentUser?.FullName ?? "Гость";

        public static void Clear() => CurrentUser = null;
    }
}
