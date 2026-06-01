namespace MirIgrushek.Models
{
    /// <summary>
    /// Заказ. Содержит данные для вывода по макету Модуля 4
    /// (артикул, статус, адрес пункта выдачи, дата заказа, дата доставки).
    /// </summary>
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderArticle { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public int PickupPointId { get; set; }
        public string PickupAddress { get; set; } = string.Empty;

        public int ClientUserId { get; set; }
        public string ClientFullName { get; set; } = string.Empty;

        public int? ReceiveCode { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
