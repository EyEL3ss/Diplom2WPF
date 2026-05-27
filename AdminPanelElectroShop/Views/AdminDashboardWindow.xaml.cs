using AdminPanelElectroShop.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AdminPanelElectroShop.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminDashboardWindow.xaml
    /// </summary>
    public partial class AdminDashboardWindow : Window
    {
        private readonly AuthService _authService;

        // ✅ Конструктор принимает AuthService
        public AdminDashboardWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            DataContext = new { CurrentUser = _authService.CurrentUser };

            ContentFrame.Navigate(new StatisticsPage());
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new StatisticsPage());

        private void Users_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new UsersManagementPage());

        private void Moderation_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new ProductsModerationPage());

        private void Products_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new ProductCatalogManagementPage());

        private void PromoCodes_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new PromoCodesPage());

        private void Stock_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new StockManagementPage());

        private void Orders_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new OrdersManagementPage());

        private void DatabaseConsole_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new DatabaseConsolePage());

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void AddPage_Button(object sender, RoutedEventArgs e)
        
            => ContentFrame.Navigate(new AddProductPage());
        
    }
}
