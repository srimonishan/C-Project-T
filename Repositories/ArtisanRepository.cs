using System;
using System.Data;
using Microsoft.Data.SqlClient;
using LocalArtisanCraftMarket.Database;
using LocalArtisanCraftMarket.Models;

namespace LocalArtisanCraftMarket.Repositories
{
    internal class ArtisanRepository
    {
        public int Create(Artisan artisan)
        {
            string sql = @"INSERT INTO Artisans (FullName,Email,Phone,Address,Password) VALUES (@FullName,@Email,@Phone,@Address,@Password); SELECT SCOPE_IDENTITY();";
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", artisan.FullName);
                    cmd.Parameters.AddWithValue("@Email", artisan.Email);
                    cmd.Parameters.AddWithValue("@Phone", artisan.Phone);
                    cmd.Parameters.AddWithValue("@Address", artisan.Address);
                    cmd.Parameters.AddWithValue("@Password", artisan.Password);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool EmailExists(string email)
        {
            string sql = "SELECT COUNT(1) FROM Artisans WHERE Email = @Email";
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
