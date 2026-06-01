using MirIgrushek.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MirIgrushek
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ранняя проверка соединения, чтобы не падать позже в окнах.
            if (!Database.TestConnection(out string error))
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных.\n\n" +
                    "Проверьте строку подключения в файле appsettings.json " +
                    "и работу SQL Server.\n\nПодробности: " + error,
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }
    }

}
