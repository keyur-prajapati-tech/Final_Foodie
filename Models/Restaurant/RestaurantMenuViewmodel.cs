namespace Foodie.Models.Restaurant
{
    public class RestaurantMenuViewmodel
    {
        public tbl_vendores_img tbl_Vendores_Img { get; set; } = new tbl_vendores_img();

        public tbl_vendor_availability tbl_Vendor_Availability { get; set; } = new tbl_vendor_availability();
    }
}
