namespace Foodie.ViewModels
{
    public class VendoreViewModel
    {
      
            public int RestaurantId { get; set; }
            public string RestaurantName { get; set; }
            public string RestaurantEmail { get; set; }
            public string RestaurantContact { get; set; }
            public string RestaurantStreet { get; set; }
            public string RestaurantPincode { get; set; }

            public string OwnerName { get; set; }
            public string OwnerContact { get; set; }
            public string OwnerEmail { get; set; }

            public string BankName { get; set; }
            public string IFSCCode { get; set; }
            public string ACCNo { get; set; }
            public string ACCType { get; set; }

            public string PanNumber { get; set; }
            public string PanHolderName { get; set; }
            public bool PanIsVerified { get; set; }
            public byte[] PanImage { get; set; }

            public string GstNumber { get; set; }
            public bool GstIsVerified { get; set; }

            public string FssaiCerti { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public bool FssaiIsVerified { get; set; }
            public byte[] FssaiImage { get; set; }

            public byte[] RestaurantImage { get; set; }
            public byte[] MenuImage { get; set; }
       

    }
}
