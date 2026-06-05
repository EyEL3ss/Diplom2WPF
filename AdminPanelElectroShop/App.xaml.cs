using AdminPanelElectroShop.Database;
using AdminPanelElectroShop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Navigation;

namespace AdminPanelElectroShop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // ✅ Теперь всё работает — у DbConnection есть нужный конструктор
            services.AddDbContext<DbConnection>(options =>
                options.UseMySql(DbConnection.ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

            // Регистрация сервисов
            services.AddSingleton<AuthService>();

            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);
        }
    }
}

