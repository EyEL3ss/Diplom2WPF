using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelElectroShop.Views
{
    public partial class OrdersManagementPage : Page
    {
        private readonly DbConnection _context;
        private List<Order> _orders = new();
        private List<OrderItem> _orderItems = new();
        private Order? _currentOrder;
        private OrderItem? _currentOrderItem;
        private int? _selectedOrderId;

        public OrdersManagementPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (_, _) => await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            _orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            OrdersGrid.ItemsSource = _orders;
        }

        private async Task LoadOrderItemsAsync(int orderId)
        {
            _orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .OrderBy(oi => oi.Id)
                .ToListAsync();
            OrderItemsGrid.ItemsSource = _orderItems;
            OrderItemsTitleText.Text = $"Позиции заказа: #{orderId}";
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(query))
            {
                OrdersGrid.ItemsSource = _orders;
                return;
            }

            OrdersGrid.ItemsSource = _orders
                .Where(o => o.OrderNumber.ToLowerInvariant().Contains(query) ||
                            o.Status!.ToLowerInvariant().Contains(query) ||
                            o.PaymentStatus!.ToLowerInvariant().Contains(query))
                .ToList();
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            _currentOrder = null;
            OrderNumberBox.Text = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
            UserIdBox.Text = string.Empty;
            TotalAmountBox.Text = string.Empty;
            StatusBox.SelectedIndex = 0;
            PaymentStatusBox.SelectedIndex = 0;
            EditPanel.Visibility = Visibility.Visible;
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Order order)
            {
                return;
            }

            _currentOrder = order;
            OrderNumberBox.Text = order.OrderNumber;
            UserIdBox.Text = order.UserId.ToString();
            TotalAmountBox.Text = order.TotalAmount.ToString();
            StatusBox.SelectedItem = StatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Content?.ToString() ?? "") == order.Status) ?? StatusBox.Items[0];
            PaymentStatusBox.SelectedItem = PaymentStatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Content?.ToString() ?? "") == order.PaymentStatus) ?? PaymentStatusBox.Items[0];
            EditPanel.Visibility = Visibility.Visible;
        }

        private async void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(UserIdBox.Text, out var userId) || !decimal.TryParse(TotalAmountBox.Text, out var total))
            {
                MessageBox.Show("Проверьте UserID и сумму заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "new";
            var paymentStatus = (PaymentStatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "pending";

            if (_currentOrder == null)
            {
                var order = new Order
                {
                    OrderNumber = OrderNumberBox.Text.Trim(),
                    UserId = userId,
                    TotalAmount = total,
                    Status = status,
                    PaymentStatus = paymentStatus,
                    DeliveryMethod = "pickup",
                    PaymentMethod = "card",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Orders.AddAsync(order);
            }
            else
            {
                _currentOrder.OrderNumber = OrderNumberBox.Text.Trim();
                _currentOrder.UserId = userId;
                _currentOrder.TotalAmount = total;
                _currentOrder.Status = status;
                _currentOrder.PaymentStatus = paymentStatus;
                _currentOrder.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await LoadOrdersAsync();
            EditPanel.Visibility = Visibility.Collapsed;
            _currentOrder = null;

            if (_selectedOrderId.HasValue)
            {
                await LoadOrderItemsAsync(_selectedOrderId.Value);
            }
        }

        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Order order)
            {
                return;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            await LoadOrdersAsync();
            _selectedOrderId = null;
            OrderItemsGrid.ItemsSource = null;
            OrderItemsTitleText.Text = "Позиции заказа: не выбран";
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            _currentOrder = null;
            EditPanel.Visibility = Visibility.Collapsed;
        }

        private async void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is not Order selectedOrder)
            {
                return;
            }

            _selectedOrderId = selectedOrder.Id;
            await LoadOrderItemsAsync(selectedOrder.Id);
        }

        private void AddOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedOrderId.HasValue)
            {
                MessageBox.Show("Сначала выберите заказ", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _currentOrderItem = null;
            ItemProductIdBox.Text = string.Empty;
            ItemProductNameBox.Text = string.Empty;
            ItemPriceBox.Text = string.Empty;
            ItemQuantityBox.Text = "1";
            OrderItemEditPanel.Visibility = Visibility.Visible;
        }

        private void EditOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not OrderItem item)
            {
                return;
            }

            _currentOrderItem = item;
            ItemProductIdBox.Text = item.ProductId?.ToString() ?? string.Empty;
            ItemProductNameBox.Text = item.ProductName;
            ItemPriceBox.Text = item.ProductPrice.ToString();
            ItemQuantityBox.Text = item.Quantity.ToString();
            OrderItemEditPanel.Visibility = Visibility.Visible;
        }

        private async void SaveOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedOrderId.HasValue)
            {
                return;
            }

            if (!int.TryParse(ItemQuantityBox.Text, out var quantity) || quantity <= 0 ||
                !decimal.TryParse(ItemPriceBox.Text, out var productPrice))
            {
                MessageBox.Show("Проверьте цену и количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? productId = null;
            if (int.TryParse(ItemProductIdBox.Text, out var parsedProductId))
            {
                productId = parsedProductId;
            }

            if (_currentOrderItem == null)
            {
                var orderItem = new OrderItem
                {
                    OrderId = _selectedOrderId.Value,
                    ProductId = productId,
                    ProductName = ItemProductNameBox.Text.Trim(),
                    ProductPrice = productPrice,
                    Quantity = quantity,
                    TotalPrice = productPrice * quantity
                };

                await _context.OrderItems.AddAsync(orderItem);
            }
            else
            {
                _currentOrderItem.ProductId = productId;
                _currentOrderItem.ProductName = ItemProductNameBox.Text.Trim();
                _currentOrderItem.ProductPrice = productPrice;
                _currentOrderItem.Quantity = quantity;
                _currentOrderItem.TotalPrice = productPrice * quantity;
            }

            await _context.SaveChangesAsync();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == _selectedOrderId.Value);
            if (order != null)
            {
                order.TotalAmount = await _context.OrderItems
                    .Where(oi => oi.OrderId == _selectedOrderId.Value)
                    .SumAsync(oi => oi.TotalPrice);
                await _context.SaveChangesAsync();
            }

            await LoadOrdersAsync();
            await LoadOrderItemsAsync(_selectedOrderId.Value);
            _currentOrderItem = null;
            OrderItemEditPanel.Visibility = Visibility.Collapsed;
        }

        private async void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not OrderItem item || !_selectedOrderId.HasValue)
            {
                return;
            }

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == _selectedOrderId.Value);
            if (order != null)
            {
                order.TotalAmount = await _context.OrderItems
                    .Where(oi => oi.OrderId == _selectedOrderId.Value)
                    .SumAsync(oi => oi.TotalPrice);
                await _context.SaveChangesAsync();
            }

            await LoadOrdersAsync();
            await LoadOrderItemsAsync(_selectedOrderId.Value);
        }

        private void CancelOrderItemEdit_Click(object sender, RoutedEventArgs e)
        {
            _currentOrderItem = null;
            OrderItemEditPanel.Visibility = Visibility.Collapsed;
        }
    }
}
