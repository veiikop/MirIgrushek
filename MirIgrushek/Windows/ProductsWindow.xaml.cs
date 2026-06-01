using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MirIgrushek.Models;
using MirIgrushek.Services;

namespace MirIgrushek.Windows
{
    /// <summary>
    /// Главное окно со списком товаров. Внешний вид и доступный функционал
    /// зависят от роли пользователя (гость, клиент, менеджер, администратор).
    /// </summary>
    public partial class ProductsWindow : Window
    {
        private readonly UserRole _role;
        private List<Product> _allProducts = new List<Product>();

        // Флаг открытого окна редактирования (запрет на несколько окон сразу).
        private bool _editWindowOpen;

        public ProductsWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;

            ConfigureForRole();
            LoadReferencesForFilters();
            LoadProducts();
            ShowUserInfo();
        }

        // Настраивает видимость элементов под конкретную роль.
        private void ConfigureForRole()
        {
            // Поиск/фильтр/сортировка доступны только менеджеру и администратору.
            bool canSearch = _role == UserRole.Manager || _role == UserRole.Admin;
            ToolsPanel.Visibility = canSearch ? Visibility.Visible : Visibility.Collapsed;

            // Кнопка "Заказы" — для менеджера и администратора.
            OrdersButton.Visibility = canSearch ? Visibility.Visible : Visibility.Collapsed;

            // Добавление/удаление товаров — только администратор.
            bool isAdmin = _role == UserRole.Admin;
            AddProductButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteProductButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        // Заполняет фильтр поставщиков и список вариантов сортировки.
        private void LoadReferencesForFilters()
        {
            if (_role != UserRole.Manager && _role != UserRole.Admin)
            {
                return;
            }

            // Первый элемент фильтра — "Все поставщики" (сброс фильтра).
            List<Reference> suppliers = new List<Reference> { new Reference { Id = 0, Name = "Все поставщики" } };
            suppliers.AddRange(ProductService.GetSuppliers());
            SupplierFilter.ItemsSource = suppliers;
            SupplierFilter.SelectedIndex = 0;

            // Варианты сортировки по количеству на складе.
            SortBox.ItemsSource = new List<string>
            {
                "Без сортировки",
                "По возрастанию",
                "По убыванию"
            };
            SortBox.SelectedIndex = 0;
        }

        // Загружает товары из базы и применяет текущие фильтры.
        private void LoadProducts()
        {
            try
            {
                _allProducts = ProductService.GetAll();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить список товаров.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Выводит ФИО и роль пользователя в правом верхнем углу.
        private void ShowUserInfo()
        {
            UserNameText.Text = Session.DisplayName;
            RoleText.Text = _role switch
            {
                UserRole.Admin => "Администратор",
                UserRole.Manager => "Менеджер",
                UserRole.Client => "Авторизированный клиент",
                _ => "Гость"
            };
        }

        // Единый обработчик для поиска, фильтрации и сортировки в реальном времени.
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            // На этапе инициализации элементы могут быть ещё не готовы.
            if (!IsLoaded)
            {
                return;
            }
            ApplyFilters();
        }

        /// <summary>
        /// Применяет поиск, фильтр по поставщику и сортировку совместно.
        /// Поиск идёт по всем текстовым полям товара.
        /// </summary>
        private void ApplyFilters()
        {
            IEnumerable<Product> query = _allProducts;

            // --- Поиск по всем текстовым данным ---
            string search = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Article.ToLower().Contains(search) ||
                    p.ProductName.ToLower().Contains(search) ||
                    p.CategoryName.ToLower().Contains(search) ||
                    p.ProducerName.ToLower().Contains(search) ||
                    p.SupplierName.ToLower().Contains(search) ||
                    p.UnitName.ToLower().Contains(search) ||
                    (p.Description ?? string.Empty).ToLower().Contains(search));
            }

            // --- Фильтр по поставщику ---
            if (SupplierFilter.SelectedItem is Reference supplier && supplier.Id != 0)
            {
                query = query.Where(p => p.SupplierId == supplier.Id);
            }

            // --- Сортировка по количеству на складе ---
            if (SortBox.SelectedItem is string sort)
            {
                query = sort switch
                {
                    "По возрастанию" => query.OrderBy(p => p.StockQuantity),
                    "По убыванию" => query.OrderByDescending(p => p.StockQuantity),
                    _ => query
                };
            }

            List<Product> result = query.ToList();
            ProductsList.ItemsSource = result;
            CountText.Text = $"Показано товаров: {result.Count} из {_allProducts.Count}";
        }

        // Открытие формы редактирования двойным щелчком (только администратор).
        private void ProductsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != UserRole.Admin)
            {
                return;
            }
            if (ProductsList.SelectedItem is not Product selected)
            {
                return;
            }
            OpenEditWindow(selected);
        }

        // Добавление нового товара.
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            OpenEditWindow(null);
        }

        // Удаление выбранного товара с проверкой бизнес-правил.
        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product selected)
            {
                MessageBox.Show(
                    "Выберите товар в списке, который нужно удалить.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Товар, присутствующий в заказе, удалять нельзя.
            if (ProductService.IsUsedInOrders(selected.Article))
            {
                MessageBox.Show(
                    "Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.\n\n" +
                    "Сначала удалите связанные заказы либо измените их состав.",
                    "Удаление запрещено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Предупреждение о необратимой операции.
            MessageBoxResult confirm = MessageBox.Show(
                $"Вы действительно хотите удалить товар:\n«{selected.ProductName}»?\n\nЭто действие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                ProductService.Delete(selected.Article);
                MessageBox.Show("Товар успешно удалён.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить товар.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Открывает окно добавления/редактирования товара (только одно окно за раз).
        private void OpenEditWindow(Product? product)
        {
            if (_editWindowOpen)
            {
                MessageBox.Show(
                    "Окно редактирования уже открыто.\nЗавершите текущее редактирование.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _editWindowOpen = true;
            ProductEditWindow window = new ProductEditWindow(product)
            {
                Owner = this
            };
            bool? saved = window.ShowDialog();
            _editWindowOpen = false;

            // После добавления/редактирования обновляем список.
            if (saved == true)
            {
                LoadProducts();
            }
        }

        // Переход к окну заказов.
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow window = new OrdersWindow(_role)
            {
                Owner = this
            };
            window.ShowDialog();
        }

        // Выход на главный экран (окно входа).
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Session.Clear();
            LoginWindow login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}
