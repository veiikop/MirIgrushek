using System.IO;
using System.Windows.Media.Imaging;

namespace MirIgrushek.Helpers
{
    /// <summary>
    /// Преобразует относительный путь к фото товара в готовое изображение.
    /// При отсутствии файла возвращает картинку-заглушку из ресурсов (picture.png).
    /// </summary>
    public static class ImageHelper
    {
        // Заглушка лежит в ресурсах сборки.
        private const string PlaceholderUri = "pack://application:,,,/Resources/picture.png";

        /// <summary>
        /// Возвращает изображение по относительному пути (например, "Images\\1.jpg").
        /// Путь отсчитывается от папки запущенного приложения.
        /// </summary>
        public static BitmapImage GetImage(string? relativePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(relativePath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                    if (File.Exists(fullPath))
                    {
                        return LoadFromFile(fullPath);
                    }
                }
            }
            catch
            {
                // При любой ошибке загрузки используем заглушку.
            }

            return LoadPlaceholder();
        }

        // Загружает изображение из файла, не блокируя сам файл на диске.
        private static BitmapImage LoadFromFile(string fullPath)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }

        private static BitmapImage LoadPlaceholder()
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(PlaceholderUri, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
