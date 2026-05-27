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
    /// Логика взаимодействия для SellerDashboardWindow.xaml
    /// </summary>
    public partial class SellerDashboardWindow : Window
    {
        private readonly AuthService _authService;

        // ✅ Конструктор принимает AuthService
        public SellerDashboardWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            DataContext = new { CurrentUser = _authService.CurrentUser };


        }



        private void AddProduct_Click(object sender, RoutedEventArgs e)
            => ContentFrame.Navigate(new AddProductPage());




        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
