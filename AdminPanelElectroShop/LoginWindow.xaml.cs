using AdminPanelElectroShop.Services;
using AdminPanelElectroShop.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdminPanelElectroShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = App.ServiceProvider.GetRequiredService<AuthService>();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            LoginButton.IsEnabled = false;
            LoadingText.Visibility = Visibility.Visible;

            try
            {
                var success = await _authService.LoginAsync(login, password);

                // ✅ Отладка
                System.Diagnostics.Debug.WriteLine($"Login success: {success}");
                System.Diagnostics.Debug.WriteLine($"IsAdmin: {_authService.IsAdmin}");
                System.Diagnostics.Debug.WriteLine($"IsSeller: {_authService.IsSeller}");

                if (success)
                {
                    if (_authService.IsAdmin)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Открываем панель администратора");
                        var adminWindow = new AdminDashboardWindow(_authService);
                        adminWindow.Show();
                        this.Close();
                    }
                    else if (_authService.IsSeller)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Открываем панель продавца");
                        var sellerWindow = new SellerDashboardWindow(_authService);
                        sellerWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Роль не распознана: " + _authService.CurrentUser?.Role);
                        ShowError("У вас нет доступа к панели управления");
                    }
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка: {ex.Message}");
                ShowError($"Ошибка подключения: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoadingText.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenMainWindow()
        {
            if (_authService.IsAdmin)
            {
                // ✅ ПЕРЕДАЁМ AuthService в конструктор
                var adminWindow = new AdminDashboardWindow(_authService);
                adminWindow.Show();
            }
            else if (_authService.IsSeller)
            {
                // ✅ ПЕРЕДАЁМ AuthService в конструктор
                var sellerWindow = new SellerDashboardWindow(_authService);
                sellerWindow.Show();
            }
            else
            {
                ShowError("У вас нет доступа к панели управления");
                return;
            }

            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}