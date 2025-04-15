namespace Foodie.Models.Restaurant
{
    public class tbl_vendor_img
    {
        public int vendore_img_id { get; set; }
        public int Restaurant_id { get; set; }
        public byte[] Restaurant_img { get; set; }
        public byte[] Restaurant_menu_img { get; set; }
    }
}
