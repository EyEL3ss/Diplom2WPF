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
        private List<User> _sellers = new();
        private Product? _currentProduct;

        public ProductCatalogManagementPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (_, _) =>
            {
                await _context.EnsureAdminPanelSchemaAsync();
                await LoadSellersAsync();
                await LoadProductsAsync();
            };
        }

        private async Task LoadSellersAsync()
        {
            _sellers = await _context.Users
                .Where(u => u.Role == "seller" && u.IsActive == true)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            SellerCombo.ItemsSource = _sellers;
        }

        private async Task LoadProductsAsync()
        {
            _products = await _context.Products
                .Include(p => p.Discounts)
                .Include(p => p.Seller)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

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
                .Where(p => p.Name.ToLowerInvariant().Contains(query)
                            || (p.Brand != null && p.Brand.ToLowerInvariant().Contains(query))
                            || p.SellerName.ToLowerInvariant().Contains(query))
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
            SellerCombo.SelectedValue = product.SellerId;
            ReceivedDatePicker.SelectedDate = product.ReceivedAt ?? DateTime.Today;
            NomenclatureBox.Text = product.Nomenclature ?? string.Empty;
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
            _currentProduct.SellerId = SellerCombo.SelectedValue is int sellerId ? sellerId : null;
            _currentProduct.ReceivedAt = ReceivedDatePicker.SelectedDate;
            _currentProduct.Nomenclature = string.IsNullOrWhiteSpace(NomenclatureBox.Text) ? null : NomenclatureBox.Text.Trim();
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

            var orderItemsCount = await _context.OrderItems.CountAsync(item => item.ProductId == product.Id);
            if (orderItemsCount > 0)
            {
                MessageBox.Show(
                    $"Товар \"{product.Name}\" нельзя удалить, потому что он находится в заказах. Количество позиций в заказах: {orderItemsCount}.",
                    "Удаление запрещено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
