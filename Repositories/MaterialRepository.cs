using LocalArtisanCraftMarket.Database;
using LocalArtisanCraftMarket.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalArtisanCraftMarket.Repositories
{
    internal class MaterialRepository
    {

        /// Returns all materials ordered by name.
        public List<Material> GetAll()
        {
            var list = new List<Material>();

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT MaterialID, MaterialName, Unit, UnitCost,
                                      AvailableQuantity, Supplier, LastUpdated
                               FROM   Materials
                               ORDER  BY MaterialName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        list.Add(MapMaterial(dr));
                }
            }
            return list;
        }

        /// Returns a single material by ID, or null if not found.
        public Material GetById(int materialId)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT MaterialID, MaterialName, Unit, UnitCost,
                                      AvailableQuantity, Supplier, LastUpdated
                               FROM   Materials
                               WHERE  MaterialID = @MaterialID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaterialID", materialId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        return dr.Read() ? MapMaterial(dr) : null;
                    }
                }
            }
        }

        /// Inserts a new material.
        /// Returns true on success; throws on duplicate name or constraint
        /// violation so the caller can show a meaningful message.
        public bool Add(Material m)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Materials
                                   (MaterialName, Unit, UnitCost, AvailableQuantity,
                                    Supplier, LastUpdated)
                               VALUES
                                   (@Name, @Unit, @Cost, @Qty, @Supplier, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    AddMaterialParams(cmd, m);
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        /// Updates an existing material. Returns true on success.
        public bool Update(Material m)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Materials
                               SET    MaterialName      = @Name,
                                      Unit              = @Unit,
                                      UnitCost          = @Cost,
                                      AvailableQuantity = @Qty,
                                      Supplier          = @Supplier,
                                      LastUpdated       = GETDATE()
                               WHERE  MaterialID = @MaterialID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    AddMaterialParams(cmd, m);
                    cmd.Parameters.AddWithValue("@MaterialID", m.MaterialID);
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        /// Deletes a material only if it is not referenced by any craft item.
        /// Returns true on success, false if rows are still linked.
        public bool Delete(int materialId)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                // Safety check — do not delete if it is assigned to any craft item
                string checkSql = @"SELECT COUNT(*) FROM CraftItemMaterials
                                    WHERE MaterialID = @MaterialID";
                using (SqlCommand check = new SqlCommand(checkSql, conn))
                {
                    check.Parameters.AddWithValue("@MaterialID", materialId);
                    int linked = (int)check.ExecuteScalar();
                    if (linked > 0)
                        return false; // caller should warn the user
                }

                string deleteSql = "DELETE FROM Materials WHERE MaterialID = @MaterialID";
                using (SqlCommand del = new SqlCommand(deleteSql, conn))
                {
                    del.Parameters.AddWithValue("@MaterialID", materialId);
                    return del.ExecuteNonQuery() == 1;
                }
            }
        }

        /// Returns true if a material name already exists (for validation).
        public bool NameExists(string name, int excludeId = 0)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM Materials
                               WHERE MaterialName = @Name AND MaterialID <> @ExcludeID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name.Trim());
                    cmd.Parameters.AddWithValue("@ExcludeID", excludeId);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        /// Links a material to a craft item.
        /// Returns true on success, false if the pair is already assigned.
        public bool AssignToCraftItem(int craftItemId, int materialId,
                                      decimal quantityUsed, decimal cost)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                // Unique constraint UQ_CIM_Pair handles duplicates at DB level,
                // but we check first so we can return false instead of throwing.
                string checkSql = @"SELECT COUNT(*) FROM CraftItemMaterials
                                    WHERE CraftItemID = @CraftItemID
                                      AND MaterialID  = @MaterialID";
                using (SqlCommand check = new SqlCommand(checkSql, conn))
                {
                    check.Parameters.AddWithValue("@CraftItemID", craftItemId);
                    check.Parameters.AddWithValue("@MaterialID", materialId);
                    if ((int)check.ExecuteScalar() > 0)
                        return false; // already assigned
                }

                string insertSql = @"INSERT INTO CraftItemMaterials
                                         (CraftItemID, MaterialID, QuantityUsed, Cost)
                                     VALUES
                                         (@CraftItemID, @MaterialID, @Qty, @Cost)";
                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@CraftItemID", craftItemId);
                    cmd.Parameters.AddWithValue("@MaterialID", materialId);
                    cmd.Parameters.AddWithValue("@Qty", quantityUsed);
                    cmd.Parameters.AddWithValue("@Cost", cost);
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        /// Updates the quantity and cost for an existing craft-item / material link.
        public bool UpdateAssignment(int craftMaterialId, decimal quantityUsed, decimal cost)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE CraftItemMaterials
                               SET    QuantityUsed = @Qty, Cost = @Cost
                               WHERE  CraftMaterialID = @ID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Qty", quantityUsed);
                    cmd.Parameters.AddWithValue("@Cost", cost);
                    cmd.Parameters.AddWithValue("@ID", craftMaterialId);
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        /// Removes one material assignment from a craft item.
        public bool RemoveAssignment(int craftMaterialId)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM CraftItemMaterials WHERE CraftMaterialID = @ID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", craftMaterialId);
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        /// Returns all materials assigned to a specific craft item,
        /// joined with material name and unit for display.
        public List<CraftMaterial> GetMaterialsByItem(int craftItemId)
        {
            var list = new List<CraftMaterial>();

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT cim.CraftMaterialID,
                                      cim.CraftItemID,
                                      ci.ItemName,
                                      cim.MaterialID,
                                      m.MaterialName,
                                      m.Unit,
                                      cim.QuantityUsed,
                                      cim.Cost
                               FROM   CraftItemMaterials cim
                               JOIN   Materials   m  ON m.MaterialID   = cim.MaterialID
                               JOIN   CraftItems  ci ON ci.CraftItemID = cim.CraftItemID
                               WHERE  cim.CraftItemID = @CraftItemID
                               ORDER BY m.MaterialName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CraftItemID", craftItemId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new CraftMaterial
                            {
                                CraftMaterialID = (int)dr["CraftMaterialID"],
                                CraftItemID = (int)dr["CraftItemID"],
                                ItemName = dr["ItemName"].ToString(),
                                MaterialID = (int)dr["MaterialID"],
                                MaterialName = dr["MaterialName"].ToString(),
                                Unit = dr["Unit"].ToString(),
                                QuantityUsed = (decimal)dr["QuantityUsed"],
                                Cost = (decimal)dr["Cost"]
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// Sums all material costs for a craft item.
        /// Called by Module 7 (Price Suggestion) to calculate MaterialCost.
        public decimal GetTotalMaterialCost(int craftItemId)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT ISNULL(SUM(Cost), 0)
                               FROM   CraftItemMaterials
                               WHERE  CraftItemID = @CraftItemID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CraftItemID", craftItemId);
                    return (decimal)cmd.ExecuteScalar();
                }
            }
        }


        // HELPERS FOR FORMS

        /// Returns a DataTable of all craft items (ID + Name).
        /// Used to populate the craft-item ComboBox in AssignMaterialForm.
        public DataTable GetCraftItemsForDropdown()
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT CraftItemID, ItemName
                               FROM   CraftItems
                               WHERE  Status = 'Available'
                               ORDER  BY ItemName";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ERROR LOGGING (integrates with Module 9)
        // ─────────────────────────────────────────────────────────────────────

        ///Logs an error to the ErrorLogs table (Module 9 integration).
        public void LogError(Exception ex)
        {
            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO ErrorLogs (ErrorMessage, ModuleName, LogDate)
                                   VALUES (@Message, @Module, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Message", ex.Message);
                        cmd.Parameters.AddWithValue("@Module", "MaterialTracking");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Do not let logging failure crash the app
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private Material MapMaterial(SqlDataReader dr)
        {
            return new Material
            {
                MaterialID = (int)dr["MaterialID"],
                MaterialName = dr["MaterialName"].ToString(),
                Unit = dr["Unit"].ToString(),
                UnitCost = (decimal)dr["UnitCost"],
                AvailableQuantity = (decimal)dr["AvailableQuantity"],
                Supplier = dr["Supplier"] == DBNull.Value ? null : dr["Supplier"].ToString(),
                LastUpdated = (DateTime)dr["LastUpdated"]
            };
        }

        private void AddMaterialParams(SqlCommand cmd, Material m)
        {
            cmd.Parameters.AddWithValue("@Name", m.MaterialName.Trim());
            cmd.Parameters.AddWithValue("@Unit", m.Unit.Trim());
            cmd.Parameters.AddWithValue("@Cost", m.UnitCost);
            cmd.Parameters.AddWithValue("@Qty", m.AvailableQuantity);
            cmd.Parameters.AddWithValue("@Supplier",
                string.IsNullOrWhiteSpace(m.Supplier)
                    ? (object)DBNull.Value
                    : m.Supplier.Trim());
        }

    }
}
