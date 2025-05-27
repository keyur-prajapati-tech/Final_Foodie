namespace Foodie.Models.Restaurant
{
    public class tbl_menu_items
    {
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public int cuisine_id { get; set; } 
        public byte[] menu_img { get; set; }
        public string menu_descripation { get; set; }
        public decimal amount { get; set; }
        public bool isAvailable { get; set; }
        public int Restaurant_id { get; set; }

        //// For displaying in view
        //public string MenuImageBase64 => Convert.ToBase64String(menu_img);
    }
}
