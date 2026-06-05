using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для StockManagementPage.xaml
    /// </summary>
    public partial class StockManagementPage : Page
    {
        private readonly DbConnection _context;
        private List<Product> _allProducts = new();
        private readonly SemaphoreSlim _dbLock = new(1, 1);
        private bool _isLoadingProducts;

        public StockManagementPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (s, e) => await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            await _dbLock.WaitAsync();
            try
            {
                _isLoadingProducts = true;
                await _context.EnsureAdminPanelSchemaAsync();
                _allProducts = await _context.Products
                    .Include(p => p.Discounts)
                    .Include(p => p.Seller)
                    .OrderBy(p => p.StockQuantity)
                    .ToListAsync();
                ProductsGrid.ItemsSource = _allProducts;
            }
            finally
            {
                _isLoadingProducts = false;
                _dbLock.Release();
            }
        }
        private async void HitCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoadingProducts) return;

            var checkBox = sender as CheckBox;
            var product = checkBox?.DataContext as Product;
            if (product == null) return;

            await _dbLock.WaitAsync();
            try
            {
                product.IsHit = true;
                product.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
            }
            finally
            {
                _dbLock.Release();
            }

            MessageBox.Show($"Товар \"{product.Name}\" добавлен в хиты продаж", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void HitCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isLoadingProducts) return;

            var checkBox = sender as CheckBox;
            var product = checkBox?.DataContext as Product;
            if (product == null) return;

            await _dbLock.WaitAsync();
            try
            {
                product.IsHit = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            finally
            {
                _dbLock.Release();
            }
        }
        private void EditStock_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;
            if (product == null) return;

            QuantityBox.Text = product.StockQuantity.ToString();
            ReceivedDatePicker.SelectedDate = product.ReceivedAt ?? DateTime.Today;
            NomenclatureBox.Text = product.Nomenclature ?? string.Empty;
            CurrentProduct = product;
            EditPanel.Visibility = Visibility.Visible;
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            EditPanel.Visibility = Visibility.Collapsed;
            CurrentProduct = null;
        }

        private async void SaveStock_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentProduct == null) return;

            if (!int.TryParse(QuantityBox.Text, out int newQuantity))
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _dbLock.WaitAsync();
            try
            {
                CurrentProduct.StockQuantity = newQuantity;
                CurrentProduct.InStock = newQuantity > 0;
                CurrentProduct.ReceivedAt = ReceivedDatePicker.SelectedDate;
                CurrentProduct.Nomenclature = string.IsNullOrWhiteSpace(NomenclatureBox.Text) ? null : NomenclatureBox.Text.Trim();
                CurrentProduct.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            finally
            {
                _dbLock.Release();
            }

            await LoadProductsAsync();

            EditPanel.Visibility = Visibility.Collapsed;
            CurrentProduct = null;

            MessageBox.Show("Количество обновлено", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ProductsGrid.ItemsSource = _allProducts;
            }
            else
            {
                var filtered = _allProducts
                    .Where(p => p.Name.ToLowerInvariant().Contains(searchText) ||
                               (p.Brand != null && p.Brand.ToLowerInvariant().Contains(searchText)))
                    .ToList();
                ProductsGrid.ItemsSource = filtered;
            }
        }

        private Product? CurrentProduct { get; set; }
    }
}
