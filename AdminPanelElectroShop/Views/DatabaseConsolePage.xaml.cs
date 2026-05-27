using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelElectroShop.Views
{
    public partial class DatabaseConsolePage : Page
    {
        public DatabaseConsolePage()
        {
            InitializeComponent();
        }

        private async void RunQuery_Click(object sender, RoutedEventArgs e)
        {
            var sql = SqlTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            try
            {
                if (sql.StartsWith("select", StringComparison.OrdinalIgnoreCase))
                {
                    using var connection = new MySqlConnection(Database.DbConnection.ConnectionString);
                    await connection.OpenAsync();
                    using var command = new MySqlCommand(sql, connection);
                    using var adapter = new MySqlDataAdapter(command);
                    var table = new DataTable();
                    adapter.Fill(table);
                    ResultGrid.ItemsSource = table.DefaultView;
                    ResultInfoText.Text = $"Строк: {table.Rows.Count}";
                }
                else
                {
                    var context = App.ServiceProvider.GetRequiredService<Database.DbConnection>();
                    var affectedRows = await context.Database.ExecuteSqlRawAsync(sql);
                    ResultGrid.ItemsSource = null;
                    ResultInfoText.Text = $"Команда выполнена. Изменено строк: {affectedRows}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
