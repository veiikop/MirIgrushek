using Microsoft.Data.SqlClient;
using MirIgrushek.Models;

namespace MirIgrushek.Services
{
    /// <summary>
    /// Сервис работы с товарами: чтение списка, справочников и операции
    /// добавления, редактирования, удаления (Модули 2 и 3).
    /// </summary>
    public static class ProductService
    {
        /// <summary>
        /// Загружает все товары вместе с названиями из справочников.
        /// </summary>
        public static List<Product> GetAll()
        {
            List<Product> products = new List<Product>();

            const string sql = @"
                SELECT p.Article, p.ProductName,
                       p.CategoryId, c.CategoryName,
                       p.ProducerId, pr.ProducerName,
                       p.SupplierId, s.SupplierName,
                       p.UnitId, u.UnitName,
                       p.Price, p.Discount, p.StockQuantity,
                       p.Description, p.PhotoPath
                FROM dbo.Products p
                INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
                INNER JOIN dbo.Producers  pr ON pr.ProducerId = p.ProducerId
                INNER JOIN dbo.Suppliers  s ON s.SupplierId = p.SupplierId
                INNER JOIN dbo.Units      u ON u.UnitId = p.UnitId
                ORDER BY p.ProductName;";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    Article = reader.GetString(0),
                    ProductName = reader.GetString(1),
                    CategoryId = reader.GetInt32(2),
                    CategoryName = reader.GetString(3),
                    ProducerId = reader.GetInt32(4),
                    ProducerName = reader.GetString(5),
                    SupplierId = reader.GetInt32(6),
                    SupplierName = reader.GetString(7),
                    UnitId = reader.GetInt32(8),
                    UnitName = reader.GetString(9),
                    Price = reader.GetDecimal(10),
                    Discount = reader.GetInt32(11),
                    StockQuantity = reader.GetInt32(12),
                    Description = reader.IsDBNull(13) ? null : reader.GetString(13),
                    PhotoPath = reader.IsDBNull(14) ? null : reader.GetString(14)
                });
            }

            return products;
        }

        /// <summary>
        /// Универсальная загрузка справочника (Id + Название) из любой таблицы.
        /// </summary>
        public static List<Reference> GetReferences(string table, string idColumn, string nameColumn)
        {
            List<Reference> result = new List<Reference>();
            string sql = $"SELECT {idColumn}, {nameColumn} FROM dbo.{table} ORDER BY {nameColumn};";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Reference { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            }
            return result;
        }

        public static List<Reference> GetCategories() => GetReferences("Categories", "CategoryId", "CategoryName");
        public static List<Reference> GetProducers() => GetReferences("Producers", "ProducerId", "ProducerName");
        public static List<Reference> GetSuppliers() => GetReferences("Suppliers", "SupplierId", "SupplierName");
        public static List<Reference> GetUnits() => GetReferences("Units", "UnitId", "UnitName");

        /// <summary>
        /// Проверяет, существует ли товар с указанным артикулом.
        /// </summary>
        public static bool ArticleExists(string article)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE Article = @article;";
            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@article", article);
            return (int)command.ExecuteScalar() > 0;
        }

        /// <summary>
        /// Добавляет новый товар.
        /// </summary>
        public static void Add(Product product)
        {
            const string sql = @"
                INSERT INTO dbo.Products
                    (Article, ProductName, CategoryId, ProducerId, SupplierId, UnitId,
                     Price, Discount, StockQuantity, Description, PhotoPath)
                VALUES
                    (@article, @name, @category, @producer, @supplier, @unit,
                     @price, @discount, @stock, @description, @photo);";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            FillParameters(command, product);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Обновляет существующий товар (артикул не меняется).
        /// </summary>
        public static void Update(Product product)
        {
            const string sql = @"
                UPDATE dbo.Products SET
                    ProductName   = @name,
                    CategoryId    = @category,
                    ProducerId    = @producer,
                    SupplierId    = @supplier,
                    UnitId        = @unit,
                    Price         = @price,
                    Discount      = @discount,
                    StockQuantity = @stock,
                    Description   = @description,
                    PhotoPath     = @photo
                WHERE Article = @article;";

            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            FillParameters(command, product);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Проверяет, используется ли товар хотя бы в одном заказе.
        /// Товар, присутствующий в заказе, удалять нельзя.
        /// </summary>
        public static bool IsUsedInOrders(string article)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.OrderItems WHERE Article = @article;";
            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@article", article);
            return (int)command.ExecuteScalar() > 0;
        }

        /// <summary>
        /// Удаляет товар по артикулу.
        /// </summary>
        public static void Delete(string article)
        {
            const string sql = "DELETE FROM dbo.Products WHERE Article = @article;";
            using SqlConnection connection = Database.GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@article", article);
            command.ExecuteNonQuery();
        }

        // Заполняет параметры команды значениями товара (исключает дублирование кода).
        private static void FillParameters(SqlCommand command, Product product)
        {
            command.Parameters.AddWithValue("@article", product.Article);
            command.Parameters.AddWithValue("@name", product.ProductName);
            command.Parameters.AddWithValue("@category", product.CategoryId);
            command.Parameters.AddWithValue("@producer", product.ProducerId);
            command.Parameters.AddWithValue("@supplier", product.SupplierId);
            command.Parameters.AddWithValue("@unit", product.UnitId);
            command.Parameters.AddWithValue("@price", product.Price);
            command.Parameters.AddWithValue("@discount", product.Discount);
            command.Parameters.AddWithValue("@stock", product.StockQuantity);
            command.Parameters.AddWithValue("@description", (object?)product.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@photo", (object?)product.PhotoPath ?? DBNull.Value);
        }
    }
}
