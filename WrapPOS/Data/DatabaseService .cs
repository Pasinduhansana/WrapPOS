using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WrapPOS.Models;
using System.Data.SQLite;
using System.IO;

namespace WrapPOS.Data
{
   public class DatabaseService
    {
        private static string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos_database.db");

        public ObservableCollection<Product> GetProducts()
{
            var products = new ObservableCollection<Product>();

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Products;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //  products.Add(new Product
                        //  {
                        //      ProductId = reader.GetInt32(0),
                        //      Name = reader.GetString(1),
                        //      BuyPrice = reader.GetDecimal(2),
                        //      SellPrice = reader.GetDecimal(3),
                        //      Stock = reader.GetInt32(4),
                        //      ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                        //      Units = reader.GetDecimal(6),
                        //      UOM = reader.GetString(7)
                        //  });

                        products.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            BuyPrice = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                            SellPrice = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                            Stock = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Units = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                            UOM = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                        });

                    }
                }
            }
            return products;
        }

        public void DeleteProduct(int productId)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
            DELETE FROM Products 
            WHERE ProductId = $productId;
        ";
                command.Parameters.AddWithValue("$productId", productId);

                command.ExecuteNonQuery();
            }
        }

        public void AddProduct(Product newProduct)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // SQL query to insert a new product into the Products table
                    string query = "INSERT INTO Products (Name, BuyPrice, SellPrice, Stock, ImagePath, Units, UOM) " +
                                   "VALUES (@Name, @BuyPrice, @SellPrice, @Stock, @ImagePath, @Units, @UOM)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@Name", newProduct.Name);
                        command.Parameters.AddWithValue("@BuyPrice", newProduct.BuyPrice);
                        command.Parameters.AddWithValue("@SellPrice", newProduct.SellPrice);
                        command.Parameters.AddWithValue("@Stock", newProduct.Stock);
                        command.Parameters.AddWithValue("@ImagePath", (object)newProduct.ImagePath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Units", newProduct.Units);
                        command.Parameters.AddWithValue("@UOM", newProduct.UOM);

                        // Execute the insert query
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that may have occurred
                Console.WriteLine($"Error adding product: {ex.Message}");
            }
        }

    }
}
