using MySqlConnector;
using System.Windows.Forms;

namespace UserCurrencyConverter
{
    public static class DbHistoryWriter
    {
        public static void SaveConversionHistory(string fromCurrency, string toCurrency, decimal amount, decimal convertedAmount)
        {
            string server = SecretFileHandler.GetSecretData("server");
            string user = SecretFileHandler.GetSecretData("user");
            string password = SecretFileHandler.GetSecretData("dbPassword");
            string database = SecretFileHandler.GetSecretData("database");

            string connectionString = $"server={server};user={user};password={password};database={database};";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand("INSERT INTO conversion_history (fromCurrency, toCurrency, amount, resultingSum) VALUES (@fromCurrency, @toCurrency, @amount, @resultingSum)", connection))
                    {
                        command.Parameters.AddWithValue("@fromCurrency", fromCurrency);
                        command.Parameters.AddWithValue("@toCurrency", toCurrency);
                        command.Parameters.AddWithValue("@amount", amount);
                        command.Parameters.AddWithValue("@resultingSum", convertedAmount);

                        command.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка при сохранении истории конвертации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
