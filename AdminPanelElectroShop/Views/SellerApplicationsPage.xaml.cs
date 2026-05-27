using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelElectroShop.Views
{
    public partial class SellerApplicationsPage : Page
    {
        private readonly DbConnection _context;
        private List<SellerApplication> _pendingApplications = new();

        public SellerApplicationsPage()
        {
            InitializeComponent();
            _context = App.ServiceProvider.GetRequiredService<DbConnection>();
            Loaded += async (_, _) => await LoadApplicationsAsync();
        }

        private async Task LoadApplicationsAsync()
        {
            _pendingApplications = await _context.SellerApplications
                .Where(a => a.Status == "pending")
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            ApplicationsGrid.ItemsSource = _pendingApplications;
            PendingCountText.Text = $"На проверке: {_pendingApplications.Count}";
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SellerApplication application)
            {
                return;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == application.UserId);
            if (user != null)
            {
                user.Role = "seller";
                user.UpdatedAt = DateTime.UtcNow;
            }

            application.Status = "approved";
            application.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LoadApplicationsAsync();
        }

        private async void Reject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SellerApplication application)
            {
                return;
            }

            application.Status = "rejected";
            application.ReviewedAt = DateTime.UtcNow;
            application.RejectionReason = "Отклонено администратором";

            await _context.SaveChangesAsync();
            await LoadApplicationsAsync();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SellerApplication application)
            {
                return;
            }

            MessageBox.Show(
                $"Магазин: {application.StoreName}\n" +
                $"Описание: {application.StoreDescription}\n" +
                $"Юр. лицо: {application.LegalName}\n" +
                $"Email: {application.Email}\n" +
                $"Документы: {application.Documents}",
                "Детали заявки",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
