using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelElectroShop.Views
{
    public partial class PromoCodesPage : Page
    {
        private readonly DbConnection _context;

        public PromoCodesPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (_, _) =>
            {
                await _context.EnsureAdminPanelSchemaAsync();
                await EnsureProductDiscountsTableAsync();
                await LoadPromoCodesAsync();
                await LoadProductsAsync();
                await LoadProductDiscountsAsync();
            };
        }

        private async Task LoadPromoCodesAsync()
        {
            var promoCodes = await _context.PromoCodes
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            PromoCodesGrid.ItemsSource = promoCodes;
        }

        private async Task LoadProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Discounts)
                .OrderBy(p => p.Name)
                .ToListAsync();

            DiscountProductCombo.ItemsSource = products;
            if (DiscountProductCombo.Items.Count > 0)
            {
                DiscountProductCombo.SelectedIndex = 0;
            }

            ProductDiscountTypeCombo.SelectedIndex = 0;
            ProductDiscountStartDatePicker.SelectedDate = DateTime.Today;
            ProductDiscountEndDatePicker.SelectedDate = DateTime.Today.AddMonths(1);
        }

        private async Task LoadProductDiscountsAsync()
        {
            var discounts = await _context.ProductDiscounts
                .Include(d => d.Product)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            ProductDiscountsGrid.ItemsSource = discounts;
        }

        private void AddPromo_Click(object sender, RoutedEventArgs e)
        {
            CodeBox.Text = string.Empty;
            DescriptionBox.Text = string.Empty;
            DiscountTypeCombo.SelectedIndex = 0;
            DiscountValueBox.Text = string.Empty;
            MinOrderBox.Text = string.Empty;
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddMonths(1);
            IsActiveCheck.IsChecked = true;

            AddEditPanel.Visibility = Visibility.Visible;
            AddButton.Visibility = Visibility.Collapsed;
        }

        private void CancelAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEditPanel.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;
        }

        private async void SavePromo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeBox.Text))
            {
                MessageBox.Show("Введите код промокода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(DiscountValueBox.Text, out var discountValue))
            {
                MessageBox.Show("Введите корректное значение скидки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var promoCode = new PromoCode
            {
                Code = CodeBox.Text.ToUpperInvariant(),
                Description = DescriptionBox.Text,
                DiscountType = ((ComboBoxItem)DiscountTypeCombo.SelectedItem)?.Content?.ToString() == "Процент" ? "percentage" : "fixed",
                DiscountValue = discountValue,
                MinOrderAmount = string.IsNullOrWhiteSpace(MinOrderBox.Text) ? 0 : decimal.Parse(MinOrderBox.Text),
                StartDate = StartDatePicker.SelectedDate ?? DateTime.Today,
                EndDate = EndDatePicker.SelectedDate ?? DateTime.Today.AddMonths(1),
                IsActive = IsActiveCheck.IsChecked ?? true
            };

            await _context.PromoCodes.AddAsync(promoCode);
            await _context.SaveChangesAsync();

            AddEditPanel.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;
            await LoadPromoCodesAsync();
        }

        private async void DeletePromo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not PromoCode promo)
            {
                return;
            }

            _context.PromoCodes.Remove(promo);
            await _context.SaveChangesAsync();
            await LoadPromoCodesAsync();
        }

        private async void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not PromoCode promo)
            {
                return;
            }

            promo.IsActive = !promo.IsActive;
            await _context.SaveChangesAsync();
            await LoadPromoCodesAsync();
        }

        private async void SaveProductDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (DiscountProductCombo.SelectedValue == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(ProductDiscountValueBox.Text, out var discountValue) || discountValue <= 0)
            {
                MessageBox.Show("Введите корректное значение скидки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var discountType = ((ComboBoxItem)ProductDiscountTypeCombo.SelectedItem)?.Content?.ToString() == "Процент" ? "percentage" : "fixed";
            if (discountType == "percentage" && discountValue > 100)
            {
                MessageBox.Show("Процентная скидка не может быть больше 100%", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = ProductDiscountStartDatePicker.SelectedDate ?? DateTime.Today;
            var endDate = ProductDiscountEndDatePicker.SelectedDate ?? DateTime.Today.AddMonths(1);
            if (endDate < startDate)
            {
                MessageBox.Show("Дата окончания скидки не может быть раньше даты начала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var discount = new ProductDiscount
            {
                ProductId = (int)DiscountProductCombo.SelectedValue,
                DiscountType = discountType,
                DiscountValue = discountValue,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true
            };

            await _context.ProductDiscounts.AddAsync(discount);
            await _context.SaveChangesAsync();
            await LoadProductsAsync();
            await LoadProductDiscountsAsync();

            ProductDiscountValueBox.Text = string.Empty;
        }

        private async void ToggleProductDiscount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not ProductDiscount discount)
            {
                return;
            }

            discount.IsActive = !discount.IsActive;
            discount.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await LoadProductsAsync();
            await LoadProductDiscountsAsync();
        }

        private async void DeleteProductDiscount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not ProductDiscount discount)
            {
                return;
            }

            _context.ProductDiscounts.Remove(discount);
            await _context.SaveChangesAsync();
            await LoadProductsAsync();
            await LoadProductDiscountsAsync();
        }

        private async Task EnsureProductDiscountsTableAsync()
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS ProductDiscounts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    discount_type ENUM('percentage','fixed') NOT NULL DEFAULT 'percentage',
    discount_value DECIMAL(10,2) NOT NULL,
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    INDEX idx_product_discount_product (product_id),
    INDEX idx_product_discount_active (is_active),
    CONSTRAINT fk_product_discounts_product FOREIGN KEY (product_id) REFERENCES Products(id) ON DELETE CASCADE
);";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
