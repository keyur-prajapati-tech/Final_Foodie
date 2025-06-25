using Foodie.Models.DeliveryPartner;

namespace Foodie.Repositories
{
    public interface IDeliveryPatnerRepository
    {
        List<AssignedOrderViewModel> GetAssignedOrdersAsync(int partnerId);
        bool CancelOrder(int orderId);
        bool AcceptOrder(int partnerId, int restaurantId);
        List<tbl_deliveryNotification> GetNotifications(int partnerId);
        MemoryStream ExportOrdersToExcel(int partnerId);
        MemoryStream ExportOrdersToPdf(int partnerId);

        decimal GetTodayEarningsAsync(int partnerId);
        int GetTodayDeliveriesAsync(int partnerId);
        double GetAverageDeliveryTimeAsync(int partnerId);
        double GetAverageRatingAsync(int partnerId);
        LatestOrderInfo GetLatestOrderAsync(int partnerId);
        bool UpdateOnlineStatus(int partnerId, bool isOnline);
        bool MarkOrderAsDelivered(int orderId);
        bool UpdateOrderStatus(int orderId, string status);
    }
}
