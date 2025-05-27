namespace Foodie.Models.customers
{
    public class weeklypayoutsViewModel
    {
        List<tbl_orders> orders { get; set; }
        List<payments> allavailablepayment { get; set; }
        List<tbl_order_items> orderitems { get; set; }
    }
}
