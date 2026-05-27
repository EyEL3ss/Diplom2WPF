using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelElectroShop.Views
{
    public partial class ProductCatalogManagementPage : Page
    {
        private readonly DbConnection _context;
        private List<Product> _products = new();
        private Product? _currentProduct;

        public ProductCatalogManagementPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (_, _) => await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            _products = await _context.Products.OrderByDescending(p => p.Id).ToListAsync();
            ProductsGrid.ItemsSource = _products;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadProductsAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(query))
            {
                ProductsGrid.ItemsSource = _products;
                return;
            }

            ProductsGrid.ItemsSource = _products
                .Where(p => p.Name.ToLower().Contains(query) || (p.Brand != null && p.Brand.ToLower().Contains(query)))
                .ToList();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Product product)
            {
                return;
            }

            _currentProduct = product;
            NameBox.Text = product.Name;
            PriceBox.Text = product.Price.ToString();
            StockBox.Text = product.StockQuantity?.ToString() ?? "0";
            StatusCombo.SelectedIndex = product.Status switch
            {
                "approved" => 1,
                "rejected" => 2,
                _ => 0
            };
            EditPanel.Visibility = Visibility.Visible;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProduct == null)
            {
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out var price) || !int.TryParse(StockBox.Text, out var stock))
            {
                MessageBox.Show("Введите корректные цену и остаток", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentProduct.Name = NameBox.Text.Trim();
            _currentProduct.Price = price;
            _currentProduct.StockQuantity = stock;
            _currentProduct.InStock = stock > 0;
            _currentProduct.Status = (StatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "pending";
            _currentProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LoadProductsAsync();
            EditPanel.Visibility = Visibility.Collapsed;
            _currentProduct = null;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Product product)
            {
                return;
            }

            var result = MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            await LoadProductsAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _currentProduct = null;
            EditPanel.Visibility = Visibility.Collapsed;
        }
    }
}
