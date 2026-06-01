using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using MirIgrushek.Helpers;
using MirIgrushek.Models;
using MirIgrushek.Services;

namespace MirIgrushek.Windows
{
    /// <summary>
    /// Окно добавления и редактирования товара (Модуль 3).
    /// Реализует валидацию, выпадающие списки и работу с изображением товара.
    /// </summary>
    public partial class ProductEditWindow : Window
    {
        private readonly Product? _editing;   // null — режим добавления
        private readonly bool _isEdit;

        // Каталог хранения изображений товаров рядом с приложением.
        private static readonly string ImagesFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        // Путь к новому выбранному файлу изображения (если пользователь его сменил).
        private string? _selectedImageFullPath;

        // Текущий относительный путь к фото (из БД при редактировании).
        private string? _currentRelativePhoto;

        public ProductEditWindow(Product? product)
        {
            InitializeComponent();
            _editing = product;
            _isEdit = product is not null;

            LoadDropdowns();
            InitializeForm();
        }

        // Загружает справочники в выпадающие списки.
        private void LoadDropdowns()
        {
            CategoryBox.ItemsSource = ProductService.GetCategories();
            ProducerBox.ItemsSource = ProductService.GetProducers();
            SupplierBox.ItemsSource = ProductService.GetSuppliers();
            UnitBox.ItemsSource = ProductService.GetUnits();
        }

        // Готовит форму под режим добавления или редактирования.
        private void InitializeForm()
        {
            if (_isEdit && _editing is not null)
            {
                Title = "Редактирование товара";
                HeaderText.Text = "Редактирование товара";

                // Артикул при редактировании доступен только для чтения.
                ArticleBox.Text = _editing.Article;
                ArticleBox.IsReadOnly = true;
                ArticleBox.Background = System.Windows.Media.Brushes.WhiteSmoke;

                NameBox.Text = _editing.ProductName;
                DescriptionBox.Text = _editing.Description;
                PriceBox.Text = _editing.Price.ToString("0.00", CultureInfo.CurrentCulture);
                StockBox.Text = _editing.StockQuantity.ToString();
                DiscountBox.Text = _editing.Discount.ToString();

                SelectReference(CategoryBox, _editing.CategoryId);
                SelectReference(ProducerBox, _editing.ProducerId);
                SelectReference(SupplierBox, _editing.SupplierId);
                SelectReference(UnitBox, _editing.UnitId);

                _currentRelativePhoto = _editing.PhotoPath;
                PhotoImage.Source = ImageHelper.GetImage(_editing.PhotoPath);
            }
            else
            {
                Title = "Добавление товара";
                HeaderText.Text = "Добавление товара";

                // При добавлении артикул генерируется автоматически и не редактируется.
                ArticleBox.Text = GenerateNextArticle();
                ArticleBox.IsReadOnly = true;
                ArticleBox.Background = System.Windows.Media.Brushes.WhiteSmoke;

                DiscountBox.Text = "0";
                StockBox.Text = "0";
                PriceBox.Text = "0,00";
                PhotoImage.Source = ImageHelper.GetImage(null);

                // По умолчанию выбираем первые элементы справочников.
                if (CategoryBox.Items.Count > 0) CategoryBox.SelectedIndex = 0;
                if (ProducerBox.Items.Count > 0) ProducerBox.SelectedIndex = 0;
                if (SupplierBox.Items.Count > 0) SupplierBox.SelectedIndex = 0;
                if (UnitBox.Items.Count > 0) UnitBox.SelectedIndex = 0;
            }
        }

        // Выбирает в ComboBox элемент справочника по его Id.
        private static void SelectReference(System.Windows.Controls.ComboBox box, int id)
        {
            foreach (object item in box.Items)
            {
                if (item is Reference reference && reference.Id == id)
                {
                    box.SelectedItem = item;
                    return;
                }
            }
        }

        // Генерирует новый артикул: 6 заглавных букв и цифр (как в исходных данных).
        private string GenerateNextArticle()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string candidate;
            // Подбираем уникальный артикул, отсутствующий в базе.
            do
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 6; i++)
                {
                    builder.Append(chars[random.Next(chars.Length)]);
                }
                candidate = builder.ToString();
            }
            while (ProductService.ArticleExists(candidate));

            return candidate;
        }

        // Выбор изображения товара с диска.
        private void ChoosePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Выберите изображение товара",
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImageFullPath = dialog.FileName;
                // Показываем предпросмотр выбранного изображения.
                PhotoImage.Source = ImageHelper.GetImage(dialog.FileName);
            }
        }

        // Сохранение товара с предварительной валидацией.
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(out string error))
            {
                MessageBox.Show(error, "Проверьте введённые данные",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Обрабатываем изображение (сжатие до 300x200 и сохранение в папку).
                string? relativePhoto = ProcessImage();

                Product product = new Product
                {
                    Article = ArticleBox.Text.Trim(),
                    ProductName = NameBox.Text.Trim(),
                    CategoryId = ((Reference)CategoryBox.SelectedItem).Id,
                    ProducerId = ((Reference)ProducerBox.SelectedItem).Id,
                    SupplierId = ((Reference)SupplierBox.SelectedItem).Id,
                    UnitId = ((Reference)UnitBox.SelectedItem).Id,
                    Price = decimal.Parse(PriceBox.Text.Replace('.', ','), CultureInfo.CurrentCulture),
                    Discount = int.Parse(DiscountBox.Text),
                    StockQuantity = int.Parse(StockBox.Text),
                    Description = DescriptionBox.Text.Trim(),
                    PhotoPath = relativePhoto
                };

                if (_isEdit)
                {
                    ProductService.Update(product);
                }
                else
                {
                    ProductService.Add(product);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить товар.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения формы.
        /// Цена и количество не могут быть отрицательными; цена допускает сотые.
        /// </summary>
        private bool ValidateForm(out string error)
        {
            error = string.Empty;
            StringBuilder messages = new StringBuilder();

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                messages.AppendLine("• Укажите наименование товара.");
            }

            if (CategoryBox.SelectedItem is null) messages.AppendLine("• Выберите категорию товара.");
            if (ProducerBox.SelectedItem is null) messages.AppendLine("• Выберите производителя.");
            if (SupplierBox.SelectedItem is null) messages.AppendLine("• Выберите поставщика.");
            if (UnitBox.SelectedItem is null) messages.AppendLine("• Выберите единицу измерения.");

            // Цена: число с возможными сотыми, не отрицательное.
            string priceText = PriceBox.Text.Trim().Replace('.', ',');
            if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal price))
            {
                messages.AppendLine("• Цена должна быть числом (допускаются сотые, например 199,99).");
            }
            else if (price < 0)
            {
                messages.AppendLine("• Цена не может быть отрицательной.");
            }

            // Количество: целое неотрицательное.
            if (!int.TryParse(StockBox.Text.Trim(), out int stock))
            {
                messages.AppendLine("• Количество на складе должно быть целым числом.");
            }
            else if (stock < 0)
            {
                messages.AppendLine("• Количество на складе не может быть отрицательным.");
            }

            // Скидка: целое от 0 до 100.
            if (!int.TryParse(DiscountBox.Text.Trim(), out int discount))
            {
                messages.AppendLine("• Скидка должна быть целым числом.");
            }
            else if (discount < 0 || discount > 100)
            {
                messages.AppendLine("• Скидка должна быть в диапазоне от 0 до 100%.");
            }

            if (messages.Length > 0)
            {
                error = "Исправьте следующие ошибки:\n\n" + messages.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Сохраняет выбранное изображение в папку приложения с ограничением 300x200.
        /// При замене удаляет старый файл. Возвращает относительный путь для БД.
        /// </summary>
        private string? ProcessImage()
        {
            // Если новое изображение не выбрано — оставляем прежний путь.
            if (string.IsNullOrEmpty(_selectedImageFullPath))
            {
                return _currentRelativePhoto;
            }

            Directory.CreateDirectory(ImagesFolder);

            // Уникальное имя файла на основе артикула.
            string extension = Path.GetExtension(_selectedImageFullPath);
            string fileName = $"{ArticleBox.Text.Trim()}{extension}";
            string destinationFull = Path.Combine(ImagesFolder, fileName);

            // Загружаем исходное изображение и масштабируем до 300x200.
            BitmapImage source = new BitmapImage();
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.UriSource = new Uri(_selectedImageFullPath, UriKind.Absolute);
            source.DecodePixelWidth = 300;
            source.DecodePixelHeight = 200;
            source.EndInit();

            // Сохраняем в формате PNG.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (FileStream stream = new FileStream(destinationFull, FileMode.Create))
            {
                encoder.Save(stream);
            }

            // Удаляем старый файл, если он отличается от нового.
            DeleteOldImageIfNeeded(fileName);

            return Path.Combine("Images", fileName);
        }

        // Удаляет прежнее изображение из папки, если оно заменяется на новое.
        private void DeleteOldImageIfNeeded(string newFileName)
        {
            if (string.IsNullOrEmpty(_currentRelativePhoto))
            {
                return;
            }

            string oldFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _currentRelativePhoto);
            string oldName = Path.GetFileName(oldFull);

            if (!string.Equals(oldName, newFileName, StringComparison.OrdinalIgnoreCase)
                && File.Exists(oldFull))
            {
                try
                {
                    File.Delete(oldFull);
                }
                catch
                {
                    // Не критично, если файл удалить не удалось.
                }
            }
        }

        // Кнопка "Назад" — закрыть без сохранения.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
