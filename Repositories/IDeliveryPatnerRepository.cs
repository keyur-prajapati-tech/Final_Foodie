using Foodie.Models.DeliveryPartner;

namespace Foodie.Repositories
{
    public interface IDeliveryPatnerRepository
    {
        List<AssignedOrderViewModel> GetAssignedOrdersAsync(int partnerId);
        bool UpdateOrderStatus(int orderId, string status);
        bool MarkOrderAsDelivered(int orderId);
        bool CancelOrder(int orderId);
        bool AcceptOrder(int partnerId, int restaurantId);
        List<tbl_deliveryNotification> GetNotifications(int partnerId);
        MemoryStream ExportOrdersToExcel(int partnerId);
        MemoryStream ExportOrdersToPdf(int partnerId);
    }
}
