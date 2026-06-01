using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MirIgrushek.Models;
using MirIgrushek.Services;

namespace MirIgrushek.Windows
{
    /// <summary>
    /// Окно просмотра заказов (Модуль 4). Менеджер видит список,
    /// администратор дополнительно может добавлять, редактировать и удалять заказы.
    /// </summary>
    public partial class OrdersWindow : Window
    {
        private readonly UserRole _role;
        private bool _editWindowOpen;

        public OrdersWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;

            // Управление заказами доступно только администратору.
            bool isAdmin = _role == UserRole.Admin;
            AddOrderButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteOrderButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            LoadOrders();
        }

        // Загружает заказы из базы данных.
        private void LoadOrders()
        {
            try
            {
                List<Order> orders = OrderService.GetAll();
                OrdersList.ItemsSource = orders;
                CountText.Text = $"Всего заказов: {orders.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить список заказов.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Редактирование заказа двойным щелчком (только администратор).
        private void OrdersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != UserRole.Admin)
            {
                return;
            }
            if (OrdersList.SelectedItem is not Order selected)
            {
                return;
            }
            OpenEditWindow(selected);
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenEditWindow(null);
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is not Order selected)
            {
                MessageBox.Show(
                    "Выберите заказ в списке, который нужно удалить.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Удалить заказ № {selected.OrderId}?\n\nЭто действие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                OrderService.Delete(selected.OrderId);
                MessageBox.Show("Заказ успешно удалён.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить заказ.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Открывает окно добавления/редактирования заказа (одно окно за раз).
        private void OpenEditWindow(Order? order)
        {
            if (_editWindowOpen)
            {
                MessageBox.Show(
                    "Окно редактирования заказа уже открыто.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _editWindowOpen = true;
            OrderEditWindow window = new OrderEditWindow(order)
            {
                Owner = this
            };
            bool? saved = window.ShowDialog();
            _editWindowOpen = false;

            if (saved == true)
            {
                LoadOrders();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
