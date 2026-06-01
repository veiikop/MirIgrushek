using Microsoft.Data.SqlClient;
using MirIgrushek.Models;

namespace MirIgrushek.Services
{
    /// <summary>
    /// Сервис работы с заказами (Модуль 4): чтение списка, справочников
    /// статусов и пунктов выдачи, операции добавления, редактирования, удаления.
    /// </summary>
    public static class OrderService
    {
        /// <summary>
        /// Загружает все заказы с названиями статуса, адресом пункта выдачи и ФИО клиента.
        /// </summary>
        public static List<Order> GetAll()
        {
            List<Order> orders = new List<Order>();

            const string sql = @"
                SELECT o.OrderId, o.OrderArticle, o.OrderDate, o.DeliveryDate,
                       o.PickupPointId, pp.Address,
                       o.ClientUserId, u.FullName,
                       o.ReceiveCode,
                       o.StatusId, st.StatusName
                FROM dbo.Orders o
                INNER JOIN dbo.PickupPoints  pp ON pp.PickupPointId = o.PickupPointId
                INNER JOIN dbo.Users         u  ON u.UserId = o.ClientUserId
                INNER JOIN dbo.OrderStatuses st ON st.StatusId = o.StatusId
                ORDER BY o.OrderId;";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32(0),
                    OrderArticle = reader.GetString(1),
                    OrderDate = reader.GetDateTime(2),
                    DeliveryDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    PickupPointId = reader.GetInt32(4),
                    PickupAddress = reader.GetString(5),
                    ClientUserId = reader.GetInt32(6),
                    ClientFullName = reader.GetString(7),
                    ReceiveCode = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    StatusId = reader.GetInt32(9),
                    StatusName = reader.GetString(10)
                });
            }

            return orders;
        }

        public static List<Reference> GetStatuses() =>
            ProductService.GetReferences("OrderStatuses", "StatusId", "StatusName");

        public static List<Reference> GetPickupPoints() =>
            ProductService.GetReferences("PickupPoints", "PickupPointId", "Address");

        public static List<Reference> GetClients()
        {
            // Клиенты для выпадающего списка заказа (роль "Авторизированный клиент").
            List<Reference> result = new List<Reference>();
            const string sql = @"
                SELECT u.UserId, u.FullName
                FROM dbo.Users u
                INNER JOIN dbo.Roles r ON r.RoleId = u.RoleId
                WHERE r.RoleName = N'Авторизированный клиент'
                ORDER BY u.FullName;";
            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Reference { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            }
            return result;
        }

        /// <summary>
        /// Добавляет новый заказ.
        /// </summary>
        public static void Add(Order order)
        {
            const string sql = @"
                INSERT INTO dbo.Orders
                    (OrderArticle, OrderDate, DeliveryDate, PickupPointId,
                     ClientUserId, ReceiveCode, StatusId)
                VALUES
                    (@article, @orderDate, @deliveryDate, @pickup,
                     @client, @code, @status);";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            FillParameters(command, order);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Обновляет существующий заказ.
        /// </summary>
        public static void Update(Order order)
        {
            const string sql = @"
                UPDATE dbo.Orders SET
                    OrderArticle  = @article,
                    OrderDate     = @orderDate,
                    DeliveryDate  = @deliveryDate,
                    PickupPointId = @pickup,
                    ClientUserId  = @client,
                    ReceiveCode   = @code,
                    StatusId      = @status
                WHERE OrderId = @id;";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            FillParameters(command, order);
            command.Parameters.AddWithValue("@id", order.OrderId);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Удаляет заказ (позиции OrderItems удаляются каскадно по FK).
        /// </summary>
        public static void Delete(int orderId)
        {
            const string sql = "DELETE FROM dbo.Orders WHERE OrderId = @id;";
            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", orderId);
            command.ExecuteNonQuery();
        }

        private static void FillParameters(SqlCommand command, Order order)
        {
            command.Parameters.AddWithValue("@article", order.OrderArticle);
            command.Parameters.AddWithValue("@orderDate", order.OrderDate);
            command.Parameters.AddWithValue("@deliveryDate", (object?)order.DeliveryDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@pickup", order.PickupPointId);
            command.Parameters.AddWithValue("@client", order.ClientUserId);
            command.Parameters.AddWithValue("@code", (object?)order.ReceiveCode ?? DBNull.Value);
            command.Parameters.AddWithValue("@status", order.StatusId);
        }
    }
}
