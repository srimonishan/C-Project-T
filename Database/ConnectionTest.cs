using Microsoft.Data.SqlClient;

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
            catch
            {
                return false;
            }
        }
    }
}