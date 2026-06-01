using System.Windows;
using System.Windows.Controls;
using MirIgrushek.Models;
using MirIgrushek.Services;

namespace MirIgrushek.Windows
{
    /// <summary>
    /// Окно добавления и редактирования заказа (Модуль 4).
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        private readonly Order? _editing;
        private readonly bool _isEdit;

        public OrderEditWindow(Order? order)
        {
            InitializeComponent();
            _editing = order;
            _isEdit = order is not null;

            LoadDropdowns();
            InitializeForm();
        }

        // Загружает справочники в выпадающие списки.
        private void LoadDropdowns()
        {
            StatusBox.ItemsSource = OrderService.GetStatuses();
            PickupBox.ItemsSource = OrderService.GetPickupPoints();
            ClientBox.ItemsSource = OrderService.GetClients();
        }

        // Подготавливает форму под режим добавления или редактирования.
        private void InitializeForm()
        {
            if (_isEdit && _editing is not null)
            {
                Title = "Редактирование заказа";
                HeaderText.Text = "Редактирование заказа";

                ArticleBox.Text = _editing.OrderArticle;
                OrderDatePicker.SelectedDate = _editing.OrderDate;
                DeliveryDatePicker.SelectedDate = _editing.DeliveryDate;
                CodeBox.Text = _editing.ReceiveCode?.ToString();

                SelectReference(StatusBox, _editing.StatusId);
                SelectReference(PickupBox, _editing.PickupPointId);
                SelectReference(ClientBox, _editing.ClientUserId);
            }
            else
            {
                Title = "Добавление заказа";
                HeaderText.Text = "Добавление заказа";

                OrderDatePicker.SelectedDate = DateTime.Today;
                if (StatusBox.Items.Count > 0) StatusBox.SelectedIndex = 0;
                if (PickupBox.Items.Count > 0) PickupBox.SelectedIndex = 0;
                if (ClientBox.Items.Count > 0) ClientBox.SelectedIndex = 0;
            }
        }

        // Выбирает элемент справочника в ComboBox по Id.
        private static void SelectReference(ComboBox box, int id)
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

        // Сохранение заказа с валидацией.
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
                int? code = null;
                if (int.TryParse(CodeBox.Text.Trim(), out int parsedCode))
                {
                    code = parsedCode;
                }

                Order order = new Order
                {
                    OrderId = _isEdit ? _editing!.OrderId : 0,
                    OrderArticle = ArticleBox.Text.Trim(),
                    OrderDate = OrderDatePicker.SelectedDate!.Value,
                    DeliveryDate = DeliveryDatePicker.SelectedDate,
                    StatusId = ((Reference)StatusBox.SelectedItem).Id,
                    PickupPointId = ((Reference)PickupBox.SelectedItem).Id,
                    ClientUserId = ((Reference)ClientBox.SelectedItem).Id,
                    ReceiveCode = code
                };

                if (_isEdit)
                {
                    OrderService.Update(order);
                }
                else
                {
                    OrderService.Add(order);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить заказ.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Проверка обязательных полей заказа.
        private bool ValidateForm(out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(ArticleBox.Text))
            {
                error = "Укажите артикул заказа.";
                return false;
            }
            if (StatusBox.SelectedItem is null)
            {
                error = "Выберите статус заказа.";
                return false;
            }
            if (PickupBox.SelectedItem is null)
            {
                error = "Выберите адрес пункта выдачи.";
                return false;
            }
            if (ClientBox.SelectedItem is null)
            {
                error = "Выберите клиента.";
                return false;
            }
            if (OrderDatePicker.SelectedDate is null)
            {
                error = "Укажите дату заказа.";
                return false;
            }

            // Дата выдачи не может быть раньше даты заказа.
            if (DeliveryDatePicker.SelectedDate is DateTime delivery
                && delivery < OrderDatePicker.SelectedDate.Value)
            {
                error = "Дата выдачи не может быть раньше даты заказа.";
                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
