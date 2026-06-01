using System.Windows;
using MirIgrushek.Models;
using MirIgrushek.Services;

namespace MirIgrushek.Windows
{
    /// <summary>
    /// Окно входа — первое, что видит пользователь.
    /// Позволяет авторизоваться или перейти в режим гостя.
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Обрабатывает нажатие кнопки "Войти".
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Проверка заполнения полей.
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Введите логин и пароль для входа в систему.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                User? user = AuthService.Authenticate(login, password);

                if (user is null)
                {
                    MessageBox.Show(
                        "Неверный логин или пароль.\nПроверьте правильность ввода и повторите попытку.",
                        "Ошибка авторизации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Сохраняем пользователя в сессии и открываем нужный интерфейс.
                Session.CurrentUser = user;
                OpenInterfaceForRole(user.RoleName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка при обращении к базе данных.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Вход в роли гостя: только просмотр товаров без фильтрации/поиска.
        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            Session.Clear();
            ProductsWindow window = new ProductsWindow(UserRole.Guest);
            window.Show();
            Close();
        }

        // Открывает окно товаров с правами, соответствующими роли.
        private void OpenInterfaceForRole(string roleName)
        {
            UserRole role = roleName switch
            {
                "Администратор" => UserRole.Admin,
                "Менеджер" => UserRole.Manager,
                "Авторизированный клиент" => UserRole.Client,
                _ => UserRole.Guest
            };

            ProductsWindow window = new ProductsWindow(role);
            window.Show();
            Close();
        }
    }
}
