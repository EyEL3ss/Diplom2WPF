using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AdminPanelElectroShop.Services
{
    public class AuthService
    {
        private readonly DbConnection _context;
        private User _currentUser;

        public AuthService(DbConnection context)
        {
            _context = context;
        }

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        // ✅ Нормализация роли (убираем пробелы, приводим к нижнему регистру)
        public bool IsAdmin => _currentUser?.Role?.Trim().ToLower() == "admin";
        public bool IsSeller => _currentUser?.Role?.Trim().ToLower() == "seller";

        public async Task<bool> LoginAsync(string login, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    (u.Email == login || u.Phone == login) &&
                    u.IsActive == true);

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Пользователь не найден");
                return false;
            }

            // Отладка
            System.Diagnostics.Debug.WriteLine($"✅ Пользователь: {user.Email}, Роль: '{user.Role}', Активен: {user.IsActive}");

            if (!VerifyPassword(password, user.PasswordHash))
            {
                System.Diagnostics.Debug.WriteLine("❌ Неверный пароль");
                return false;
            }

            _currentUser = user;

            System.Diagnostics.Debug.WriteLine($"✅ Вход выполнен. IsAdmin: {IsAdmin}, IsSeller: {IsSeller}");

            return true;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
