using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelElectroShop.Views
{
    public partial class OrdersManagementPage : Page
    {
        private readonly DbConnection _context;
        private List<Order> _orders = new();
        private List<OrderItem> _orderItems = new();
        private List<User> _sellers = new();
        private Order? _currentOrder;
        private OrderItem? _currentOrderItem;
        private int? _selectedOrderId;

        public OrdersManagementPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (_, _) =>
            {
                await _context.EnsureAdminPanelSchemaAsync();
                await LoadSellersAsync();
                await LoadOrdersAsync();
            };
        }

        private async Task LoadSellersAsync()
        {
            _sellers = await _context.Users
                .Where(u => u.Role == "seller" && u.IsActive == true)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            ResponsibleSellerCombo.ItemsSource = _sellers;
        }

        private async Task LoadOrdersAsync()
        {
            _orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ResponsibleSeller)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            OrdersGrid.ItemsSource = _orders;
        }

        private async Task LoadOrderItemsAsync(int orderId)
        {
            _orderItems = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p!.Seller)
                .Where(oi => oi.OrderId == orderId)
                .OrderBy(oi => oi.Id)
                .ToListAsync();

            OrderItemsGrid.ItemsSource = _orderItems;

            var selectedOrder = _orders.FirstOrDefault(o => o.Id == orderId);
            if (selectedOrder == null)
            {
                OrderItemsTitleText.Text = $"Позиции заказа: #{orderId}";
                return;
            }

            OrderItemsTitleText.Text =
                $"Заказ {selectedOrder.OrderNumber}: {selectedOrder.ProductsCount} шт., " +
                $"{selectedOrder.PositionsCount} позиций, клиент: {selectedOrder.CustomerName}, " +
                $"ответственный: {selectedOrder.ResponsibleSellerName}";
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
            if (_selectedOrderId.HasValue)
            {
                await LoadOrderItemsAsync(_selectedOrderId.Value);
            }
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
                .Where(o => o.OrderNumber.ToLowerInvariant().Contains(query)
                            || (o.Status ?? string.Empty).ToLowerInvariant().Contains(query)
                            || (o.PaymentStatus ?? string.Empty).ToLowerInvariant().Contains(query)
                            || o.CustomerName.ToLowerInvariant().Contains(query)
                            || o.ResponsibleSellerName.ToLowerInvariant().Contains(query))
                .ToList();
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            _currentOrder = null;
            OrderNumberBox.Text = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
            UserIdBox.Text = string.Empty;
            TotalAmountBox.Text = string.Empty;
            ResponsibleSellerCombo.SelectedIndex = -1;
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
            ResponsibleSellerCombo.SelectedValue = order.ResponsibleSellerId;
            StatusBox.SelectedItem = StatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Content?.ToString() ?? string.Empty) == order.Status) ?? StatusBox.Items[0];
            PaymentStatusBox.SelectedItem = PaymentStatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Content?.ToString() ?? string.Empty) == order.PaymentStatus) ?? PaymentStatusBox.Items[0];
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
            var responsibleSellerId = ResponsibleSellerCombo.SelectedValue is int sellerId ? sellerId : (int?)null;

            if (_currentOrder == null)
            {
                var order = new Order
                {
                    OrderNumber = OrderNumberBox.Text.Trim(),
                    UserId = userId,
                    TotalAmount = total,
                    ResponsibleSellerId = responsibleSellerId,
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
                _currentOrder.ResponsibleSellerId = responsibleSellerId;
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

            if (!int.TryParse(ItemQuantityBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Проверьте количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? productId = null;
            Product? product = null;
            if (int.TryParse(ItemProductIdBox.Text, out var parsedProductId))
            {
                productId = parsedProductId;
                product = await _context.Products
                    .Include(p => p.Discounts)
                    .FirstOrDefaultAsync(p => p.Id == parsedProductId);
            }

            var productName = string.IsNullOrWhiteSpace(ItemProductNameBox.Text)
                ? product?.Name ?? string.Empty
                : ItemProductNameBox.Text.Trim();

            var productPrice = product?.FinalPrice ?? 0;
            if (product == null && !decimal.TryParse(ItemPriceBox.Text, out productPrice))
            {
                MessageBox.Show("Проверьте цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (productPrice <= 0 || string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("Укажите товар или заполните название и цену вручную", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentOrderItem == null)
            {
                var orderItem = new OrderItem
                {
                    OrderId = _selectedOrderId.Value,
                    ProductId = productId,
                    ProductName = productName,
                    ProductPrice = productPrice,
                    Quantity = quantity,
                    TotalPrice = productPrice * quantity
                };

                await _context.OrderItems.AddAsync(orderItem);
            }
            else
            {
                _currentOrderItem.ProductId = productId;
                _currentOrderItem.ProductName = productName;
                _currentOrderItem.ProductPrice = productPrice;
                _currentOrderItem.Quantity = quantity;
                _currentOrderItem.TotalPrice = productPrice * quantity;
            }

            await _context.SaveChangesAsync();
            await RecalculateOrderTotalAsync(_selectedOrderId.Value);
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
            await RecalculateOrderTotalAsync(_selectedOrderId.Value);
            await LoadOrdersAsync();
            await LoadOrderItemsAsync(_selectedOrderId.Value);
        }

        private async Task RecalculateOrderTotalAsync(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return;
            }

            order.TotalAmount = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => oi.TotalPrice);
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private void CancelOrderItemEdit_Click(object sender, RoutedEventArgs e)
        {
            _currentOrderItem = null;
            OrderItemEditPanel.Visibility = Visibility.Collapsed;
        }
    }
}
