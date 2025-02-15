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

        public void UpdateInventory_Qty(int productID,int quantity)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // SQL query to delete an inventory record
                    string query = "Update Inventory set Quantity = (Quantity-@Quantity) WHERE ProductId = @ProductId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameter to avoid SQL injection
                        command.Parameters.AddWithValue("@ProductId", productID);
                        command.Parameters.AddWithValue("@Quantity", quantity);

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

        public ObservableCollection<Sales> GetSales()
        {
            var salesList = new ObservableCollection<Sales>();

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var query = @"SELECT s.SalesId, s.SalesDate, s.TotalAmount, s.Discount, s.NetAmount, s.CustomerName, s.PaymentMethod, s.Profit
                            FROM Sales s";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sale = new Sales
                        {
                            SalesId = reader.GetInt32(0),
                            SalesDate = reader.GetDateTime(1),
                            TotalAmount = reader.GetDecimal(2),
                            Discount = reader.GetDecimal(3),
                            NetAmount = reader.GetDecimal(4),
                            CustomerName = reader.IsDBNull(5) ? null : reader.GetString(5),
                            PaymentMethod = reader.GetString(6),
                            Profit = reader.GetDecimal(7)
                        };

                        // Retrieve associated SalesItems for this Sale
                        sale.SalesItems = GetSalesItemsBySalesId(sale.SalesId);

                        salesList.Add(sale);
                    }
                }
            }

            return salesList;
        }
       
        public ObservableCollection<SalesItem> GetSalesItemsBySalesId(int salesId)
        {
            var salesItemsList = new ObservableCollection<SalesItem>();

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var query = @"SELECT si.SalesItemId, si.SalesId, si.ProductId, si.ProductName, si.Description, si.UnitPrice, 
                              si.Quantity, si.TotalPrice, si.BuyPrice, si.UOM, si.Discount, si.Colour, si.Barcode, si.ImagePath
                            FROM SalesItems si
                            WHERE si.SalesId = @SalesId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SalesId", salesId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var salesItem = new SalesItem
                            {
                                SalesItemId = reader.GetInt32(0),
                                SalesId = reader.GetInt32(1),
                                ProductId = reader.GetInt32(2),
                                ProductName = reader.GetString(3),
                                Description = reader.GetString(4),
                                UnitPrice = reader.GetDecimal(5),
                                Quantity = reader.GetInt32(6),
                                TotalPrice = reader.GetDecimal(7),
                                BuyPrice = reader.GetDecimal(8),
                                UOM = reader.GetString(9),
                                Discount = reader.GetDecimal(10),
                                Colour = reader.GetString(11),
                                Barcode = reader.GetString(12),
                                ImagePath = reader.GetString(13)
                            };

                            salesItemsList.Add(salesItem);
                        }
                    }
                }
            }

            return salesItemsList;
        }  // Method to retrieve SalesItems for a specific SalesId

        public int AddSales(Sales sale)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert Sale
                        var insertSalesQuery = @"INSERT INTO Sales (SalesDate, TotalAmount, Discount, NetAmount, CustomerName, PaymentMethod, Profit) 
                                        VALUES (@SalesDate, @TotalAmount, @Discount, @NetAmount, @CustomerName, @PaymentMethod, @Profit);
                                        SELECT last_insert_rowid();";

                        using (var command = new SQLiteCommand(insertSalesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SalesDate", sale.SalesDate);
                            command.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                            command.Parameters.AddWithValue("@Discount", sale.Discount);
                            command.Parameters.AddWithValue("@NetAmount", sale.NetAmount);
                            command.Parameters.AddWithValue("@CustomerName", (object)sale.CustomerName ?? DBNull.Value);
                            command.Parameters.AddWithValue("@PaymentMethod", sale.PaymentMethod);
                            command.Parameters.AddWithValue("@Profit", sale.Profit);

                            sale.SalesId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Insert Sales Items
                        var insertSalesItemQuery = @"INSERT INTO SalesItems (SalesId, ProductId, ProductName, Description, UnitPrice, Quantity, TotalPrice, BuyPrice, UOM, Discount, Colour, Barcode, ImagePath) 
                                           VALUES (@SalesId, @ProductId, @ProductName, @Description, @UnitPrice, @Quantity, @TotalPrice, @BuyPrice, @UOM, @Discount, @Colour, @Barcode, @ImagePath);";

                        foreach (var item in sale.SalesItems)
                        {
                            using (var command = new SQLiteCommand(insertSalesItemQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@SalesId", sale.SalesId);
                                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                                command.Parameters.AddWithValue("@ProductName", item.ProductName);
                                command.Parameters.AddWithValue("@Description", (object)item.Description ?? DBNull.Value);
                                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                                command.Parameters.AddWithValue("@BuyPrice", item.BuyPrice);
                                command.Parameters.AddWithValue("@UOM", (object)item.UOM ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Discount", item.Discount);
                                command.Parameters.AddWithValue("@Colour", (object)item.Colour ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Barcode", (object)item.Barcode ?? DBNull.Value);
                                command.Parameters.AddWithValue("@ImagePath", (object)item.ImagePath ?? DBNull.Value);

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return sale.SalesId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


    }
}
