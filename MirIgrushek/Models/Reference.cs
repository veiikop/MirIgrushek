namespace MirIgrushek.Models
{
    /// <summary>
    /// Универсальный элемент справочника (Id + Название).
    /// Используется для выпадающих списков: категории, производители,
    /// поставщики, единицы измерения, статусы заказов, пункты выдачи.
    /// </summary>
    public class Reference
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Для удобного отображения в ComboBox.
        public override string ToString() => Name;
    }
}
