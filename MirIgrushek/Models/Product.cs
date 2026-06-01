namespace MirIgrushek.Models
{
    /// <summary>
    /// Товар. Объединяет данные из таблицы Products и связанных справочников
    /// (категория, производитель, поставщик, единица измерения) для удобного вывода.
    /// </summary>
    public class Product
    {
        public string Article { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int ProducerId { get; set; }
        public string ProducerName { get; set; } = string.Empty;

        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? PhotoPath { get; set; }

        /// <summary>
        /// Итоговая цена с учётом действующей скидки.
        /// </summary>
        public decimal FinalPrice => Math.Round(Price * (1 - Discount / 100m), 2);

        /// <summary>
        /// Признак наличия скидки (для зачёркивания основной цены).
        /// </summary>
        public bool HasDiscount => Discount > 0;
    }
}
