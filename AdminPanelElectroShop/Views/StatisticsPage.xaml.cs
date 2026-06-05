using AdminPanelElectroShop.Database;
using AdminPanelElectroShop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
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
    /// Логика взаимодействия для StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        private readonly DbConnection _context;

        public StatisticsPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();

            Loaded += async (s, e) => await LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            // Общая статистика
            TotalUsersText.Text = (await _context.Users.CountAsync(u => u.Role == "customer")).ToString();
            TotalProductsText.Text = (await _context.Products.CountAsync(p => p.Status == "approved")).ToString();
            TotalOrdersText.Text = (await _context.Orders.CountAsync()).ToString();

            var totalRevenue = await _context.Orders
                .Where(o => o.PaymentStatus == "paid")
                .SumAsync(o => o.TotalAmount);
            TotalRevenueText.Text = $"{totalRevenue:N0} ₽";

            // Топ товаров
            var topProducts = await _context.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new TopProduct
                {
                    Rank = 0,
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(10)
                .ToListAsync();

            int rank = 1;
            foreach (var product in topProducts)
            {
                product.Rank = rank++;
            }

            TopProductsGrid.ItemsSource = topProducts;
        }
    }
    public class TopProduct
    {
        public int Rank { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
