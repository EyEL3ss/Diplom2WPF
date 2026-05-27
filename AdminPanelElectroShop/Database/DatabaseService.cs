using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Database
{
     public class DatabaseService : IDisposable
    {
        private DbConnection _context;
        private static DatabaseService _instance;
        private bool _isInitialized = false;
        public static DatabaseService Instance => _instance ??= new DatabaseService();
        public DatabaseService()
        {
            // Конструктор приватный для синглтона
        }

        // Инициализация базы данных
        public async Task InitializeDatabase()
        {
            try
            {
                _context = new DbConnection();

                // Проверка подключения
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    throw new Exception("Не удалось подключиться к базе данных");
                }

                // Создание таблиц, если их нет (для SQLite)
                await _context.Database.EnsureCreatedAsync();

                Console.WriteLine("База данных инициализирована успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации базы данных: {ex.Message}");
                throw;
            }
        }

        // Получение пользователя по логину (с проверкой пароля)
        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
