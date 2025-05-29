using System.Numerics;

namespace Foodie.Models.DeliveryPartner
{
    public class DeliveryPartnerProfileViewModel
    {
        // Personal Details
        public int PartnerId { get; set; }
        public int DetailId { get; set; }
        public string FullName { get; set; }
        public BigInteger ContactNumber { get; set; }
        public string Email { get; set; }
        public string AadhaarNumber { get; set; }
        public string PANNumber { get; set; }
        public string ProfilePicture { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public bool IsActive { get; set; }

        // Vehicle
        public string VehicleType { get; set; }
        public string VehicleSubType { get; set; }
        public string LicensePlate { get; set; }
        public string RegistrationYear { get; set; }
        public bool InsuranceValid { get; set; }
        public DateTime? InsuranceExpiry { get; set; }

        // Bank
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSC_Code { get; set; }
        public string AccountHolderName { get; set; }
        public bool BankVerified { get; set; }

        // Performance
        public int? CompletedDeliveries { get; set; }
        public decimal? Rating { get; set; }

        // Documents
        public string AadhaarDoc { get; set; }
        public string LicenseDoc { get; set; }
        public string PANDoc { get; set; }
        public string VehicleDoc { get; set; }
    }
}