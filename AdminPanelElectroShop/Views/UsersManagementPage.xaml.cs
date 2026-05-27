using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
using System.Security.Cryptography;

namespace AdminPanelElectroShop.Views
{
    /// <summary>
    /// Логика взаимодействия для UsersManagementPage.xaml
    /// </summary>
    public partial class UsersManagementPage : Page
    {
        private readonly Database.DbConnection _context;
        private List<User> _allUsers = new();
        private User? _currentUser;

        public UsersManagementPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<Database.DbConnection>();
            Loaded += async (s, e) => await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            _allUsers = await _context.Users.ToListAsync();
            UsersGrid.ItemsSource = _allUsers;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UsersGrid.ItemsSource = _allUsers;
            }
            else
            {
                var filtered = _allUsers.Where(u =>
                    u.Email.ToLower().Contains(searchText) ||
                    u.FirstName.ToLower().Contains(searchText) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchText)) ||
                    u.Phone.Contains(searchText)).ToList();
                UsersGrid.ItemsSource = filtered;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox_TextChanged(sender, null);
        }

        private async void ToggleBlock_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;
            if (user == null) return;

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            await LoadUsersAsync();
        }

        // НОВОЕ: назначить продавцом
        private async void MakeSeller_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;
            if (user == null) return;

            if (user.Role == "seller")
            {
                MessageBox.Show("Пользователь уже является продавцом", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Назначить пользователя {user.Email} продавцом?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            user.Role = "seller";
            await _context.SaveChangesAsync();
            await LoadUsersAsync();

            MessageBox.Show("Пользователь назначен продавцом", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // НОВОЕ: снять роль продавца
        private async void RemoveSeller_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;
            if (user == null) return;

            if (user.Role != "seller")
            {
                MessageBox.Show("Пользователь не является продавцом", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Снять роль продавца с пользователя {user.Email}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            user.Role = "customer";
            await _context.SaveChangesAsync();
            await LoadUsersAsync();

            MessageBox.Show("Роль продавца снята", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Удалить пользователя? Все его данные будут потеряны.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var button = sender as Button;
            var user = button?.DataContext as User;
            if (user == null) return;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await LoadUsersAsync();
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as User;
            if (user == null) return;

            _currentUser = user;
            FirstNameBox.Text = user.FirstName;
            LastNameBox.Text = user.LastName;
            EmailBox.Text = user.Email;
            PhoneBox.Text = user.Phone;
            PasswordBox.Text = string.Empty;
            RoleBox.SelectedIndex = user.Role switch
            {
                "seller" => 1,
                "admin" => 2,
                _ => 0
            };
            EditPanel.Visibility = Visibility.Visible;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            FirstNameBox.Text = string.Empty;
            LastNameBox.Text = string.Empty;
            EmailBox.Text = string.Empty;
            PhoneBox.Text = string.Empty;
            RoleBox.SelectedIndex = 0;
            PasswordBox.Text = string.Empty;
            EditPanel.Visibility = Visibility.Visible;
        }

        private async void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            var firstName = FirstNameBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            var phone = PhoneBox.Text.Trim();
            var role = (RoleBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "customer";

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Имя, email и телефон обязательны", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentUser == null)
            {
                var rawPassword = PasswordBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(rawPassword))
                {
                    MessageBox.Show("Для нового пользователя нужен пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = new User
                {
                    FirstName = firstName,
                    LastName = string.IsNullOrWhiteSpace(LastNameBox.Text) ? null : LastNameBox.Text.Trim(),
                    Email = email,
                    Phone = phone,
                    Role = role,
                    IsActive = true,
                    PasswordHash = HashPassword(rawPassword),
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Users.AddAsync(user);
            }
            else
            {
                _currentUser.FirstName = firstName;
                _currentUser.LastName = string.IsNullOrWhiteSpace(LastNameBox.Text) ? null : LastNameBox.Text.Trim();
                _currentUser.Email = email;
                _currentUser.Phone = phone;
                _currentUser.Role = role;
                _currentUser.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await LoadUsersAsync();
            PasswordBox.Text = string.Empty;
            EditPanel.Visibility = Visibility.Collapsed;
            _currentUser = null;
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            EditPanel.Visibility = Visibility.Collapsed;
        }

        private static string HashPassword(string password)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }
}
