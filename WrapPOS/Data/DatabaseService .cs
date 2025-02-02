using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WrapPOS.Models;
using System.Data.SQLite;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                command.CommandText = "SELECT * FROM Product;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            BuyPrice = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                            SellPrice = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                            Stock = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            ImagePath = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Units = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                            UOM = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                            SupplierName = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Type = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                            Discount = reader.IsDBNull(11) ? 0.0 : reader.GetDouble(11),
                            Colour = reader.IsDBNull(12) ? null : reader.GetString(12),
                            Barcode = reader.IsDBNull(13) ? null : reader.GetString(13)
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
            DELETE FROM Product 
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

                    string query = "INSERT INTO Product (Name, Description, BuyPrice, SellPrice, Stock, ImagePath, Units, UOM, SupplierName, Type, Discount, Colour, Barcode) " +
                                   "VALUES (@Name, @Description, @BuyPrice, @SellPrice, @Stock, @ImagePath, @Units, @UOM, @SupplierName, @Type, @Discount, @Colour, @Barcode);" +
                                    "SELECT last_insert_rowid();"; // Get last inserted ID;

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@Name", newProduct.Name);
                        command.Parameters.AddWithValue("@Description", newProduct.Description);
                        command.Parameters.AddWithValue("@BuyPrice", newProduct.BuyPrice);
                        command.Parameters.AddWithValue("@SellPrice", newProduct.SellPrice);
                        command.Parameters.AddWithValue("@Stock", newProduct.Stock);
                        command.Parameters.AddWithValue("@ImagePath", (object)newProduct.ImagePath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Units", newProduct.Units);
                        command.Parameters.AddWithValue("@UOM", newProduct.UOM);
                        command.Parameters.AddWithValue("@SupplierName", (object)newProduct.SupplierName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Type", newProduct.Type);
                        command.Parameters.AddWithValue("@Discount", newProduct.Discount);
                        command.Parameters.AddWithValue("@Colour", (object)newProduct.Colour ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Barcode", (object)newProduct.Barcode ?? DBNull.Value);

                        // Execute the insert query
                        //command.ExecuteNonQuery();
                        newProduct.ProductId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that may have occurred
                Console.WriteLine($"Error adding product: {ex.Message}");
            }
        }

        public void UpdateProduct(Product updatedProduct)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    string query = "UPDATE Product SET " +
                                   "Name = @Name, " +
                                   "Description = @Description, " +
                                   "BuyPrice = @BuyPrice, " +
                                   "SellPrice = @SellPrice, " +
                                   "Stock = @Stock, " +
                                   "ImagePath = @ImagePath, " +
                                   "Units = @Units, " +
                                   "UOM = @UOM, " +
                                   "SupplierName = @SupplierName, " +
                                   "Type = @Type, " +
                                   "Discount = @Discount, " +
                                   "Colour = @Colour, " +
                                   "Barcode = @Barcode " +
                                   "WHERE ProductId = @Id"; // Assuming the product has an 'Id' to uniquely identify it.

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@Id", updatedProduct.ProductId); // Ensure 'Id' is part of the product model
                        command.Parameters.AddWithValue("@Name", updatedProduct.Name);
                        command.Parameters.AddWithValue("@Description", updatedProduct.Description);
                        command.Parameters.AddWithValue("@BuyPrice", updatedProduct.BuyPrice);
                        command.Parameters.AddWithValue("@SellPrice", updatedProduct.SellPrice);
                        command.Parameters.AddWithValue("@Stock", updatedProduct.Stock);
                        command.Parameters.AddWithValue("@ImagePath", (object)updatedProduct.ImagePath ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Units", updatedProduct.Units);
                        command.Parameters.AddWithValue("@UOM", updatedProduct.UOM);
                        command.Parameters.AddWithValue("@SupplierName", (object)updatedProduct.SupplierName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Type", updatedProduct.Type);
                        command.Parameters.AddWithValue("@Discount", updatedProduct.Discount);
                        command.Parameters.AddWithValue("@Colour", (object)updatedProduct.Colour ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Barcode", (object)updatedProduct.Barcode ?? DBNull.Value);

                        // Execute the update query
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that may have occurred
                Console.WriteLine($"Error updating product: {ex.Message}");
            }
        }

        public ObservableCollection<Inventory> GetInventoryItems()
        {
            var inventoryItems = new ObservableCollection<Inventory>();

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var query = @"SELECT i.InventoryId, i.ProductId, i.Quantity, i.PurchaseDate, i.ExpiryDate,
                             p.Name, p.Description, p.Units, p.UOM, p.SellPrice, p.SupplierName , p.Type , p.Colour , p.Barcode
                      FROM Inventory i
                      INNER JOIN Product p ON i.ProductId = p.ProductId";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var inventoryItem = new Inventory
                        {
                            InventoryId = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            PurchaseDate = reader.GetDateTime(3),
                            ExpiryDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            Product = new Product
                            {
                                Name = reader.GetString(5),
                                Description = reader.GetString(6),
                                Units= reader.GetInt32(7),
                                UOM=reader.GetString(8),
                                SellPrice=reader.GetDecimal(9),
                                SupplierName =reader.GetString(10),
                                Type=reader.GetString(11),
                                Colour=reader.GetString(12),
                                Barcode=reader.GetString(13),
                            }
                        };
                        inventoryItems.Add(inventoryItem);
                    }
                }
            }

            return inventoryItems;
        }


        public void AddInventory(Inventory inventory)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // SQL query to insert a new inventory record
                    string query = "INSERT INTO Inventory (ProductId, Quantity, PurchaseDate, ExpiryDate) " +
                                   "VALUES (@ProductId, @Quantity, @PurchaseDate, @ExpiryDate)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@ProductId", inventory.ProductId);
                        command.Parameters.AddWithValue("@Quantity", inventory.Quantity);
                        command.Parameters.AddWithValue("@PurchaseDate", inventory.PurchaseDate);
                        command.Parameters.AddWithValue("@ExpiryDate", inventory.ExpiryDate ?? (object)DBNull.Value); // Handle nullable ExpiryDate

                        // Execute the insert query
                        command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding inventory: " + ex.Message);
            }
        }

        public void DeleteInventory(int inventoryId)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // SQL query to delete an inventory record
                    string query = "DELETE FROM Inventory WHERE InventoryId = @InventoryId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameter to avoid SQL injection
                        command.Parameters.AddWithValue("@InventoryId", inventoryId);

                        // Execute the delete query
                        command.ExecuteNonQuery();
                        GetInventoryItems();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting inventory: " + ex.Message);
            }
        }
    }
}
