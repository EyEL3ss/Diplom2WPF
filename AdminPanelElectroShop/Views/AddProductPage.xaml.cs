using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using AdminPanelElectroShop.Services;
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
    /// Логика взаимодействия для AddProductPage.xaml
    /// </summary>
    public partial class AddProductPage : Page
    {
        private readonly DbConnection _context;
        private readonly AuthService _authService;

        // Событие, которое вызывается после успешного добавления товара
        public event EventHandler? ProductAdded;

        public AddProductPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            _authService = App.ServiceProvider.GetRequiredService<AuthService>();
            ReceivedDatePicker.SelectedDate = DateTime.Today;

            // Загружаем категории при загрузке страницы
            Loaded += async (s, e) => await LoadCategoriesAsync();
        }

        /// <summary>
        /// Загрузка списка категорий из базы данных
        /// </summary>
        private async Task LoadCategoriesAsync()
        {
            try
            {
                await _context.EnsureAdminPanelSchemaAsync();

                var categories = await _context.Categories
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                CategoryCombo.ItemsSource = categories;
                CategoryCombo.DisplayMemberPath = "Name";
                CategoryCombo.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки категорий: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        /// <summary>
        /// Валидация введённых данных
        /// </summary>
        private bool ValidateInputs()
        {
            // Проверка названия товара
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                StatusText.Text = "Введите название товара";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }

            // Проверка цены
            if (!decimal.TryParse(PriceBox.Text, out var price))
            {
                StatusText.Text = "Введите корректную цену";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }

            if (price <= 0)
            {
                StatusText.Text = "Цена должна быть больше 0";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }

            // Проверка количества на складе
            if (!int.TryParse(StockBox.Text, out var stock))
            {
                StatusText.Text = "Введите корректное количество товара";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }

            if (stock < 0)
            {
                StatusText.Text = "Количество не может быть отрицательным";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }

            // Проверка выбора категории
            if (CategoryCombo.SelectedItem == null)
            {
                StatusText.Text = "Выберите категорию товара";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Парсинг характеристик из строки формата "ключ1: значение1, ключ2: значение2"
        /// </summary>
        private List<ProductSpecification> ParseSpecifications(string specsText, int productId)
        {
            var specifications = new List<ProductSpecification>();

            if (string.IsNullOrWhiteSpace(specsText))
                return specifications;

            var specPairs = specsText.Split(',');

            foreach (var pair in specPairs)
            {
                var parts = pair.Split(':');
                if (parts.Length == 2)
                {
                    specifications.Add(new ProductSpecification
                    {
                        ProductId = productId,
                        SpecKey = parts[0].Trim(),
                        SpecValue = parts[1].Trim()
                    });
                }
            }

            return specifications;
        }

        /// <summary>
        /// Сохранение товара в базу данных
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация
            if (!ValidateInputs())
                return;

            // Блокируем кнопку на время сохранения
            SaveButton.IsEnabled = false;
            StatusText.Text = "Сохранение...";
            StatusText.Foreground = System.Windows.Media.Brushes.Blue;

            try
            {
                // 2. Создание объекта товара
                var product = new Product
                {
                    Name = NameBox.Text.Trim(),
                    Brand = string.IsNullOrWhiteSpace(BrandBox.Text) ? null : BrandBox.Text.Trim(),
                    CategoryId = (int)CategoryCombo.SelectedValue,
                    Price = decimal.Parse(PriceBox.Text),
                    StockQuantity = int.Parse(StockBox.Text),
                    ShortDescription = string.IsNullOrWhiteSpace(ShortDescBox.Text) ? null : ShortDescBox.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(FullDescBox.Text) ? null : FullDescBox.Text.Trim(),
                    MainImageUrl = string.IsNullOrWhiteSpace(ImageUrlBox.Text) ? null : ImageUrlBox.Text.Trim(),
                    SellerId = _authService.IsSeller ? _authService.CurrentUser.Id : null,
                    ReceivedAt = ReceivedDatePicker.SelectedDate ?? DateTime.Today,
                    Nomenclature = string.IsNullOrWhiteSpace(NomenclatureBox.Text) ? null : NomenclatureBox.Text.Trim(),
                    Status = "pending",                           // На модерацию
                    InStock = int.Parse(StockBox.Text) > 0,      // В наличии, если остаток > 0
                    CreatedAt = DateTime.UtcNow,
                    IsNew = true,                                 // Помечаем как новый товар
                    IsHit = false,                                // По умолчанию не хит
                    Rating = 0,
                    ReviewsCount = 0,
                    ViewsCount = 0,
                    SalesCount = 0
                };

                // 3. Сохранение товара в БД
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                // 4. Добавление характеристик (если есть)
                var specifications = ParseSpecifications(SpecsBox.Text, product.Id);
                if (specifications.Any())
                {
                    await _context.ProductSpecifications.AddRangeAsync(specifications);
                    await _context.SaveChangesAsync();
                }

                // 5. Успешное завершение
                StatusText.Text = "✅ Товар успешно добавлен и отправлен на модерацию!";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                // 6. Очистка формы (опционально)
                ClearForm();

                // 7. Уведомляем родительскую страницу об обновлении
                ProductAdded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка при сохранении: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Очистка формы после успешного добавления
        /// </summary>
        private void ClearForm()
        {
            NameBox.Text = "";
            BrandBox.Text = "";
            PriceBox.Text = "";
            StockBox.Text = "";
            ShortDescBox.Text = "";
            FullDescBox.Text = "";
            ImageUrlBox.Text = "";
            SpecsBox.Text = "";
            NomenclatureBox.Text = "";
            ReceivedDatePicker.SelectedDate = DateTime.Today;
            CategoryCombo.SelectedIndex = -1;
        }

        /// <summary>
        /// Отмена и возврат на предыдущую страницу
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
