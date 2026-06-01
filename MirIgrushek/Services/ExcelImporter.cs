using ClosedXML.Excel;
using System.IO;
using Microsoft.Data.SqlClient;

namespace MirIgrushek.Services
{
    /// <summary>
    /// Импорт данных о товарах из Excel-файла (ресурс Tovar.xlsx) в базу данных.
    /// Демонстрирует требование Модуля 1: «импорт данных из Excel в БД через код».
    /// Существующие товары обновляются, отсутствующие — добавляются (UPSERT).
    /// </summary>
    public static class ExcelImporter
    {
        /// <summary>
        /// Импортирует товары из указанного xlsx-файла.
        /// Возвращает количество обработанных строк.
        /// Структура листа: Артикул | Наименование | Ед.изм | Цена | Поставщик |
        /// Производитель | Категория | Скидка | Кол-во | Описание | Фото.
        /// </summary>
        public static int ImportProducts(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл для импорта не найден.", filePath);
            }

            int processed = 0;

            using SqlConnection connection = Database.GetConnection();

            using XLWorkbook workbook = new XLWorkbook(filePath);
            IXLWorksheet worksheet = workbook.Worksheet(1);

            // Пропускаем строку заголовка (первую).
            foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
            {
                string article = row.Cell(1).GetString().Trim();
                if (string.IsNullOrEmpty(article))
                {
                    continue;
                }

                string name = row.Cell(2).GetString().Trim();
                string unitName = row.Cell(3).GetString().Trim();
                decimal price = row.Cell(4).GetValue<decimal>();
                string supplierName = row.Cell(5).GetString().Trim();
                string producerName = row.Cell(6).GetString().Trim();
                string categoryName = row.Cell(7).GetString().Trim();
                int discount = (int)row.Cell(8).GetValue<double>();
                int stock = (int)row.Cell(9).GetValue<double>();
                string description = row.Cell(10).GetString().Trim();
                string photo = row.Cell(11).GetString().Trim();

                // Получаем (или создаём) идентификаторы справочников.
                int categoryId = GetOrCreateReference(connection, "Categories", "CategoryId", "CategoryName", categoryName);
                int producerId = GetOrCreateReference(connection, "Producers", "ProducerId", "ProducerName", producerName);
                int supplierId = GetOrCreateReference(connection, "Suppliers", "SupplierId", "SupplierName", supplierName);
                int unitId = GetOrCreateReference(connection, "Units", "UnitId", "UnitName", unitName);

                // Путь к фото сохраняем в формате Images\имя_файла.
                string photoPath = string.IsNullOrEmpty(photo) ? string.Empty : $"Images\\{photo}";

                UpsertProduct(connection, article, name, categoryId, producerId,
                    supplierId, unitId, price, discount, stock, description, photoPath);

                processed++;
            }

            return processed;
        }

        // Возвращает Id справочной записи по имени; при отсутствии — создаёт её.
        private static int GetOrCreateReference(SqlConnection connection, string table,
            string idColumn, string nameColumn, string value)
        {
            string selectSql = $"SELECT {idColumn} FROM dbo.{table} WHERE {nameColumn} = @name;";
            using (SqlCommand select = new SqlCommand(selectSql, connection))
            {
                select.Parameters.AddWithValue("@name", value);
                object? found = select.ExecuteScalar();
                if (found is not null)
                {
                    return (int)found;
                }
            }

            string insertSql = $@"
                INSERT INTO dbo.{table} ({nameColumn}) VALUES (@name);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using SqlCommand insert = new SqlCommand(insertSql, connection);
            insert.Parameters.AddWithValue("@name", value);
            return (int)insert.ExecuteScalar();
        }

        // Вставляет товар или обновляет его, если артикул уже существует.
        private static void UpsertProduct(SqlConnection connection, string article, string name,
            int categoryId, int producerId, int supplierId, int unitId,
            decimal price, int discount, int stock, string description, string photoPath)
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM dbo.Products WHERE Article = @article)
                    UPDATE dbo.Products SET
                        ProductName = @name, CategoryId = @category, ProducerId = @producer,
                        SupplierId = @supplier, UnitId = @unit, Price = @price,
                        Discount = @discount, StockQuantity = @stock,
                        Description = @description, PhotoPath = @photo
                    WHERE Article = @article;
                ELSE
                    INSERT INTO dbo.Products
                        (Article, ProductName, CategoryId, ProducerId, SupplierId, UnitId,
                         Price, Discount, StockQuantity, Description, PhotoPath)
                    VALUES
                        (@article, @name, @category, @producer, @supplier, @unit,
                         @price, @discount, @stock, @description, @photo);";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@article", article);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@category", categoryId);
            command.Parameters.AddWithValue("@producer", producerId);
            command.Parameters.AddWithValue("@supplier", supplierId);
            command.Parameters.AddWithValue("@unit", unitId);
            command.Parameters.AddWithValue("@price", price);
            command.Parameters.AddWithValue("@discount", discount);
            command.Parameters.AddWithValue("@stock", stock);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@photo", photoPath);
            command.ExecuteNonQuery();
        }
    }
}
