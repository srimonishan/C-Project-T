using Microsoft.Data.SqlClient;
using System;

namespace LocalArtisanCraftMarket.Database
{
    public class ConnectionTest
    {
        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Database Connection Error: " + ex.Message);
                return false;
            }
        }
    }
}