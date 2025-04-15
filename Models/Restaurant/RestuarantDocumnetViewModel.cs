namespace Foodie.Models.Restaurant
{
    public class RestuarantDocumnetViewModel
    {
        public tbl_pan_details PanDetails { get; set; } = new tbl_pan_details();
        public tbl_gst_details GstDetails { get; set; } = new tbl_gst_details();    
        public tbl_fssai_Details FssaiDetails { get; set; } = new tbl_fssai_Details();
        public tbl_bank_details BankDetails { get; set; } = new tbl_bank_details();
    }
}
