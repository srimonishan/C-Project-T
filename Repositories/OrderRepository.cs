using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using LocalArtisanCraftMarket.Database;
using LocalArtisanCraftMarket.Models;

namespace LocalArtisanCraftMarket.Repositories
{
    internal class OrderRepository
    {
        public DataTable GetAvailableCraftItems()
        {
            string sql = @"
                SELECT ci.CraftItemID, ci.ItemName, ci.SellingPrice, ci.StockQuantity,
                       a.FullName AS ArtisanName, c.CategoryName
                FROM CraftItems ci
                JOIN Artisans a   ON ci.ArtisanID  = a.ArtisanID
                JOIN Categories c ON ci.CategoryID = c.CategoryID
                WHERE ci.Status = 'Available' AND ci.StockQuantity > 0";

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Places the full order — saves to Orders table AND OrderItems table
        // Returns the new OrderID, or -1 if it failed
        public int PlaceOrder(Order order, List<OrderItem> items)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                // Use a transaction — if any step fails, NOTHING gets saved
                // (prevents half-saved orders in the database)
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Step A: Insert into Orders, get the new OrderID back
                    string orderSql = @"
                        INSERT INTO Orders (CustomerID, TotalAmount, OrderStatus, DeliveryAddress, ContactNumber)
                        VALUES (@CustomerID, @TotalAmount, 'Placed', @DeliveryAddress, @ContactNumber);
                        SELECT SCOPE_IDENTITY();";  // SCOPE_IDENTITY() returns the new OrderID

                    int newOrderID;
                    using (SqlCommand cmd = new SqlCommand(orderSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID);
                        cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                        cmd.Parameters.AddWithValue("@DeliveryAddress", order.DeliveryAddress);
                        cmd.Parameters.AddWithValue("@ContactNumber", order.ContactNumber);
                        newOrderID = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Step B: Insert each item into OrderItems
                    string itemSql = @"
                        INSERT INTO OrderItems (OrderID, CraftItemID, Quantity, UnitPrice)
                        VALUES (@OrderID, @CraftItemID, @Quantity, @UnitPrice)";

                    foreach (var item in items)
                    {
                        using (SqlCommand cmd = new SqlCommand(itemSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", newOrderID);
                            cmd.Parameters.AddWithValue("@CraftItemID", item.CraftItemID);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            cmd.ExecuteNonQuery();
                        }

                        // Step C: Reduce stock quantity for this item
                        string stockSql = @"
                            UPDATE CraftItems 
                            SET StockQuantity = StockQuantity - @Qty
                            WHERE CraftItemID = @CraftItemID";

                        using (SqlCommand cmd = new SqlCommand(stockSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Qty", item.Quantity);
                            cmd.Parameters.AddWithValue("@CraftItemID", item.CraftItemID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();   // All good — save everything
                    return newOrderID;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Something went wrong — undo everything
                    throw ex;
                }
            }
        }

        // ─────────────────────────────────────────────
        // MODULE 6: Order Status
        // ─────────────────────────────────────────────

        // Gets all orders (for admin/status view)
        public DataTable GetAllOrders()
        {
            string sql = @"
                SELECT o.OrderID, c.FullName AS CustomerName, o.OrderDate,
                       o.TotalAmount, o.OrderStatus, o.DeliveryAddress, o.ContactNumber
                FROM Orders o
                JOIN Customers c ON o.CustomerID = c.CustomerID
                ORDER BY o.OrderDate DESC";

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Gets the items inside a specific order
        public DataTable GetOrderItems(int orderID)
        {
            string sql = @"
                SELECT ci.ItemName, oi.Quantity, oi.UnitPrice, oi.SubTotal
                FROM OrderItems oi
                JOIN CraftItems ci ON oi.CraftItemID = ci.CraftItemID
                WHERE oi.OrderID = @OrderID";

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@OrderID", orderID);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Updates the status of an order (Placed → Packed → Shipped → etc.)
        public bool UpdateOrderStatus(int orderID, string newStatus)
        {
            string sql = "UPDATE Orders SET OrderStatus = @Status WHERE OrderID = @OrderID";

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;  // true = success
                }
            }
        }


    }
}
