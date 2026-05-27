using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdminPanelElectroShop.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductsModerationPage.xaml
    /// </summary>
    public partial class ProductsModerationPage : Page
    {
        private readonly DbConnection _context;
        private List<Product> _pendingProducts;

        public ProductsModerationPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (s, e) => await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            _pendingProducts = await _context.Products
                .Where(p => p.Status == "pending")
                .Include(p => p.Category)
                .Include(p => p.Images)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            ProductsGrid.ItemsSource = _pendingProducts;
            PendingCountText.Text = $"На модерации: {_pendingProducts.Count}";
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;
            if (product == null) return;

            product.Status = "approved";
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await LoadProductsAsync();

            MessageBox.Show($"Товар \"{product.Name}\" одобрен", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void Reject_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;
            if (product == null) return;

            product.Status = "rejected";
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await LoadProductsAsync();

            MessageBox.Show($"Товар \"{product.Name}\" отклонён", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;
            if (product == null) return;

            MessageBox.Show($"Название: {product.Name}\nЦена: {product.Price:N0} ₽\nОписание: {product.Description}",
                "Детали товара", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
