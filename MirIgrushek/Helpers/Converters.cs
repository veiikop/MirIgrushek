using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MirIgrushek.Models;

namespace MirIgrushek.Helpers
{
    /// <summary>
    /// Цвет фона строки товара по правилам Руководства по стилю (Приложение 3):
    /// нет на складе — голубой; скидка больше 15% — #FFDEAD; иначе белый.
    /// </summary>
    public class ProductRowBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                if (product.StockQuantity == 0)
                {
                    // Товара нет на складе — голубой фон.
                    return new SolidColorBrush(Color.FromRgb(0xAD, 0xD8, 0xE6));
                }

                if (product.Discount > 15)
                {
                    // Скидка превышает 15% — фон #FFDEAD.
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xDE, 0xAD));
                }
            }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Преобразует относительный путь к фото в изображение (с заглушкой).
    /// </summary>
    public class PhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ImageHelper.GetImage(value as string);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Управляет видимостью блока итоговой цены: показывается только при наличии скидки.
    /// </summary>
    public class DiscountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount && discount > 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
