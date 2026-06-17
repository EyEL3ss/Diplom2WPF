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
        private List<Product> _allProducts = new();
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
                await LoadProductsAsync();
                await LoadOrdersAsync();
            };
        }

        private async Task LoadSellersAsync()
        {
            _sellers = await _context.Users
                .Where(u => u.Role == "seller" && u.IsActive == true)
                .OrderBy(u => u.FirstName)
                .ToListAsync();
            ResponsibleSellerCombo.ItemsSource = _sellers;
        }

        private async Task LoadProductsAsync()
        {
            _allProducts = await _context.Products
                .Where(p => p.Status == "approved" && (p.InStock == true || p.StockQuantity > 0))
                .Include(p => p.Discounts)
                .OrderBy(p => p.Name)
                .ToListAsync();

            foreach (var product in _allProducts)
            {
                product.DisplayName = $"{product.Name} ({(product.InStock == true ? $"в наличии {product.StockQuantity}" : "нет в наличии")})";
            }

            ItemProductCombo.ItemsSource = _allProducts;
            ItemProductCombo.DisplayMemberPath = "DisplayName";
            ItemProductCombo.SelectedValuePath = "Id";
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
                .Where(oi => oi.OrderId == orderId)
                .OrderBy(oi => oi.Id)
                .ToListAsync();
            OrderItemsGrid.ItemsSource = _orderItems;
            OrderItemsTitleText.Text = $"Позиции заказа: {_orderItems.Count} шт.";
        }

        // ✅ ПОИСК
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
                            || o.CustomerName.ToLowerInvariant().Contains(query))
                .ToList();
        }

        // ✅ ОБНОВЛЕНИЕ
        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
            if (_selectedOrderId.HasValue)
            {
                await LoadOrderItemsAsync(_selectedOrderId.Value);
            }
        }

        // ✅ ДОБАВЛЕНИЕ ЗАКАЗА
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            _currentOrder = null;
            OrderNumberBox.Text = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
            UserIdBox.Text = string.Empty;
            TotalAmountBox.Text = "0";
            ResponsibleSellerCombo.SelectedIndex = -1;
            DeliveryMethodBox.SelectedIndex = 0;
            DeliveryAddressBox.Text = string.Empty;
            DeliveryDatePicker.SelectedDate = null;
            DeliveryTimeSlotBox.Text = string.Empty;
            PaymentMethodBox.SelectedIndex = 0;
            TrackingNumberBox.Text = string.Empty;
            StatusBox.SelectedIndex = 0;
            PaymentStatusBox.SelectedIndex = 0;
            EditPanel.Visibility = Visibility.Visible;
        }

        // ✅ РЕДАКТИРОВАНИЕ ЗАКАЗА
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
            DeliveryMethodBox.SelectedItem = DeliveryMethodBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Tag?.ToString() ?? string.Empty) == order.DeliveryMethod) ?? DeliveryMethodBox.Items[0];
            DeliveryAddressBox.Text = order.DeliveryAddress ?? string.Empty;
            DeliveryDatePicker.SelectedDate = order.DeliveryDate;
            DeliveryTimeSlotBox.Text = order.DeliveryTimeSlot ?? string.Empty;
            PaymentMethodBox.SelectedItem = PaymentMethodBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Tag?.ToString() ?? string.Empty) == order.PaymentMethod) ?? PaymentMethodBox.Items[0];
            TrackingNumberBox.Text = order.TrackingNumber ?? string.Empty;
            StatusBox.SelectedItem = StatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Content?.ToString() ?? string.Empty) == order.Status) ?? StatusBox.Items[0];
            PaymentStatusBox.SelectedItem = PaymentStatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (i.Content?.ToString() ?? string.Empty) == order.PaymentStatus) ?? PaymentStatusBox.Items[0];
            EditPanel.Visibility = Visibility.Visible;
        }

        // ✅ СОХРАНЕНИЕ ЗАКАЗА
        private async void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(UserIdBox.Text, out var userId))
            {
                MessageBox.Show("Проверьте UserID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var total = 0m;
            if (!string.IsNullOrWhiteSpace(TotalAmountBox.Text) && !decimal.TryParse(TotalAmountBox.Text, out total))
            {
                MessageBox.Show("Проверьте сумму заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var deliveryMethod = (DeliveryMethodBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "pickup";
            var deliveryAddress = string.IsNullOrWhiteSpace(DeliveryAddressBox.Text) ? null : DeliveryAddressBox.Text.Trim();
            if (deliveryMethod == "courier" && string.IsNullOrWhiteSpace(deliveryAddress))
            {
                MessageBox.Show("Для доставки на дом укажите адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "new";
            var paymentStatus = (PaymentStatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "pending";
            var responsibleSellerId = ResponsibleSellerCombo.SelectedValue is int sellerId ? sellerId : (int?)null;
            var paymentMethod = (PaymentMethodBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "card";
            var deliveryTimeSlot = string.IsNullOrWhiteSpace(DeliveryTimeSlotBox.Text) ? null : DeliveryTimeSlotBox.Text.Trim();
            var trackingNumber = string.IsNullOrWhiteSpace(TrackingNumberBox.Text) ? null : TrackingNumberBox.Text.Trim();

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
                    DeliveryMethod = deliveryMethod,
                    DeliveryAddress = deliveryAddress,
                    DeliveryDate = DeliveryDatePicker.SelectedDate,
                    DeliveryTimeSlot = deliveryTimeSlot,
                    PaymentMethod = paymentMethod,
                    TrackingNumber = trackingNumber,
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
                _currentOrder.DeliveryMethod = deliveryMethod;
                _currentOrder.DeliveryAddress = deliveryAddress;
                _currentOrder.DeliveryDate = DeliveryDatePicker.SelectedDate;
                _currentOrder.DeliveryTimeSlot = deliveryTimeSlot;
                _currentOrder.PaymentMethod = paymentMethod;
                _currentOrder.TrackingNumber = trackingNumber;
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

        // ✅ ОТМЕНА ЗАКАЗА
        private async void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Order order)
            {
                return;
            }

            if (order.Status == "cancelled")
            {
                MessageBox.Show("Заказ уже отменен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Отменить заказ {order.OrderNumber}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await LoadOrdersAsync();

            if (_selectedOrderId == order.Id)
            {
                await LoadOrderItemsAsync(order.Id);
            }
        }

        // ✅ УДАЛЕНИЕ ЗАКАЗА
        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Order order)
            {
                return;
            }

            var result = MessageBox.Show($"Удалить заказ {order.OrderNumber}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
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

        // ✅ ОТМЕНА РЕДАКТИРОВАНИЯ
        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            _currentOrder = null;
            EditPanel.Visibility = Visibility.Collapsed;
        }

        // ✅ ВЫБОР ЗАКАЗА В ТАБЛИЦЕ
        private async void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is not Order selectedOrder)
            {
                return;
            }

            _selectedOrderId = selectedOrder.Id;
            await LoadOrderItemsAsync(selectedOrder.Id);
        }

        // ✅ ДОБАВЛЕНИЕ ПОЗИЦИИ В ЗАКАЗ
        private void AddOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedOrderId.HasValue)
            {
                MessageBox.Show("Сначала выберите заказ", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _currentOrderItem = null;
            ItemProductCombo.SelectedIndex = -1;
            ItemProductNameBox.Text = string.Empty;
            ItemPriceBox.Text = string.Empty;
            ItemQuantityBox.Text = "1";
            OrderItemEditPanel.Visibility = Visibility.Visible;
        }

        // ✅ РЕДАКТИРОВАНИЕ ПОЗИЦИИ
        private void EditOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not OrderItem item)
            {
                return;
            }

            _currentOrderItem = item;
            if (item.ProductId.HasValue)
            {
                ItemProductCombo.SelectedValue = item.ProductId.Value;
            }
            else
            {
                ItemProductCombo.SelectedIndex = -1;
            }
            ItemProductNameBox.Text = item.ProductName;
            ItemPriceBox.Text = item.ProductPrice.ToString("F2");
            ItemQuantityBox.Text = item.Quantity.ToString();
            OrderItemEditPanel.Visibility = Visibility.Visible;
        }

        // ✅ ВЫБОР ТОВАРА В КОМБОБОКСЕ
        private void ItemProductCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemProductCombo.SelectedItem is Product selectedProduct)
            {
                ItemProductNameBox.Text = selectedProduct.Name;

                var activeDiscount = selectedProduct.Discounts?.FirstOrDefault(d =>
                    d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);

                if (activeDiscount != null)
                {
                    ItemPriceBox.Text = activeDiscount.DiscountType == "percentage"
                        ? (selectedProduct.Price - (selectedProduct.Price * activeDiscount.DiscountValue / 100)).ToString("F2")
                        : (selectedProduct.Price - activeDiscount.DiscountValue).ToString("F2");
                }
                else
                {
                    ItemPriceBox.Text = selectedProduct.Price.ToString("F2");
                }

                if (string.IsNullOrWhiteSpace(ItemQuantityBox.Text))
                {
                    ItemQuantityBox.Text = "1";
                }
            }
        }

        // ✅ СОХРАНЕНИЕ ПОЗИЦИИ
        private async void SaveOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedOrderId.HasValue) return;

            if (!int.TryParse(ItemQuantityBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Проверьте количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(ItemPriceBox.Text, out var productPrice))
            {
                MessageBox.Show("Проверьте цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? productId = null;
            string productName = ItemProductNameBox.Text.Trim();

            if (ItemProductCombo.SelectedItem is Product selectedProduct)
            {
                productId = selectedProduct.Id;
                productName = selectedProduct.Name;

                var activeDiscount = selectedProduct.Discounts?.FirstOrDefault(d =>
                    d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);

                if (activeDiscount != null)
                {
                    productPrice = activeDiscount.DiscountType == "percentage"
                        ? selectedProduct.Price - (selectedProduct.Price * activeDiscount.DiscountValue / 100)
                        : selectedProduct.Price - activeDiscount.DiscountValue;
                }
                else
                {
                    productPrice = selectedProduct.Price;
                }
            }

            if (string.IsNullOrWhiteSpace(productName) || productPrice <= 0)
            {
                MessageBox.Show("Укажите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        // ✅ УДАЛЕНИЕ ПОЗИЦИИ
        private async void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not OrderItem item || !_selectedOrderId.HasValue) return;

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            await RecalculateOrderTotalAsync(_selectedOrderId.Value);
            await LoadOrdersAsync();
            await LoadOrderItemsAsync(_selectedOrderId.Value);
        }

        // ✅ ОТМЕНА РЕДАКТИРОВАНИЯ ПОЗИЦИИ
        private void CancelOrderItemEdit_Click(object sender, RoutedEventArgs e)
        {
            _currentOrderItem = null;
            OrderItemEditPanel.Visibility = Visibility.Collapsed;
        }

        // ✅ ПЕРЕСЧЁТ ИТОГОВОЙ СУММЫ ЗАКАЗА
        private async Task RecalculateOrderTotalAsync(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            order.TotalAmount = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => oi.TotalPrice);
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
