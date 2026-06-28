using Microsoft.Data.SqlClient;
using System.Configuration;

namespace LocalArtisanCraftMarket.Database
{
    public class DatabaseConnection
    {
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["LocalArtisanCraftMarketDB"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}