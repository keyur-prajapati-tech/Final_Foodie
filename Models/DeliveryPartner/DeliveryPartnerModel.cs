using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.DeliveryPartner
{
    // 🎯 Delivery Partners Table
    [Table("tbl_deliveryPartners", Schema = "deliverypartner")]
    public class tbl_deliveryPartners
    {
        [Key]
        public int partner_id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(20)]
        public string ContactNumber { get; set; }

        [Required]
        public string Status { get; set; } = "Inactive";

        public DateTime CreatedAT { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public bool Isavailable { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    // 🎯 Delivery Partner Details
    [Table("tbl_deliveryPartnerDetails", Schema = "deliverypartner")]
    public class tbl_deliveryPartnerDetails
    {
        [Key]
        public int detail_id { get; set; }

        public int partner_id { get; set; }

        public string ProfilePicture { get; set; }

        [Required]
        public string AadhaarNumber { get; set; }

        [Required]
        public string PANNumber { get; set; }
    }

    // 🎯 Delivery Vehicle
    [Table("tbl_deliveryVehicle", Schema = "deliverypartner")]
    public class tbl_deliveryVehicle
    {
        [Key]
        public int vehicle_id { get; set; }

        public int partner_id { get; set; }

        public string RCNumber { get; set; }

        public string VehicleType { get; set; }

        public string LicensePlate { get; set; }

        public string MakeModel { get; set; }

        public string DrivingLicense { get; set; }

        public string InsuranceDetails { get; set; }

        public DateTime CreatedAT { get; set; } = DateTime.Now;
    }

    // 🎯 Delivery Request
    [Table("tbl_deliveryRequest", Schema = "deliverypartner")]
    public class tbl_deliveryRequest
    {
        [Key]
        public int request_id { get; set; }

        [Required]
        public string RequestStatus { get; set; }

        public DateTime CreatedAT { get; set; } = DateTime.Now;

        public string PickupLocation { get; set; }

        public string DropoffLocation { get; set; }

        public string EstimatedDeliveryTime { get; set; }
    }

    // 🎯 Delivery Earnings
    [Table("tbl_deliveryEarnings", Schema = "deliverypartner")]
    public class tbl_deliveryEarnings
    {
        [Key]
        public int earning_id { get; set; }

        public int partner_id { get; set; }

        public int request_id { get; set; }

        [Required]
        public decimal Earnings { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentStatus { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionID { get; set; }
    }

    // 🎯 Delivery Notifications
    [Table("tbl_deliveryNotification", Schema = "deliverypartner")]
    public class tbl_deliveryNotification
    {
        [Key]
        public int notification_id { get; set; }

        public int AssignmentID { get; set; }

        public string MessageType { get; set; }

        public string MessageContent { get; set; }

        public DateTime CreatedAT { get; set; } = DateTime.Now;

        public bool IsRead { get; set; }

        public string NotificationType { get; set; }
    }

    // 🎯 Delivery Payouts
    [Table("tbl_deliveryPayouts", Schema = "deliverypartner")]
    public class tbl_deliveryPayouts
    {
        [Key]
        public int payout_id { get; set; }

        public int partner_id { get; set; }

        public decimal TotalEarnings { get; set; }

        public decimal Deductions { get; set; }

        public decimal FinalAmount { get; set; }

        public string TransactionID { get; set; }

        public DateTime PaymentDate { get; set; }

        public int PaymentDetailID { get; set; }

        public string Status { get; set; }

        public DateTime RequestedOn { get; set; } = DateTime.Now;
    }

    // 🎯 Delivery Ratings
    [Table("tbl_deliveryRatings", Schema = "deliverypartner")]
    public class tbl_deliveryRatings
    {
        [Key]
        public int rating_id { get; set; }

        public int partner_id { get; set; }

        public int request_id { get; set; }

        public decimal CustomerRating { get; set; }

        public string ReviewComments { get; set; }

        public DateTime CreatedAT { get; set; } = DateTime.Now;
    }

    // 🎯 Delivery Slots
    [Table("tbl_deliverySlots", Schema = "deliverypartner")]
    public class tbl_deliverySlots
    {
        [Key]
        public int slot_id { get; set; }

        [Required]
        public string slot_name { get; set; }

        [Required]
        public DateTime start_time { get; set; }

        [Required]
        public DateTime end_time { get; set; }

        public decimal min_earning { get; set; }

        public decimal max_earning { get; set; }
    }

    // 🎯 Delivery Tracking
    [Table("tbl_deliveryTracking", Schema = "deliverypartner")]
    public class tbl_deliveryTracking
    {
        [Key]
        public int tracking_id { get; set; }

        public int AssignmentID { get; set; }

        public decimal CurrentLatitude { get; set; }

        public decimal CurrentLogitude { get; set; }

        public string Status { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public string DeliveryTime { get; set; }

        public decimal DistanceCovered { get; set; }

        public string EstimatedTime { get; set; }

        public string ActualTimeTaken { get; set; }
    }

    // 🎯 Partner Performance
    [Table("tbl_partnerPerformance", Schema = "deliverypartner")]
    public class tbl_partnerPerformance
    {
        [Key]
        public int performance_id { get; set; }

        public int partner_id { get; set; }

        public int TotalOrdersDelivered { get; set; }

        public int LateDeliveries { get; set; }

        public int CancellationCount { get; set; }

        public decimal CustomerRatingAvg { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // 🎯 Partner Schedule
    [Table("tbl_partnerSchedule", Schema = "deliverypartner")]
    public class tbl_partnerSchedule
    {
        [Key]
        public int schedule_id { get; set; }

        public int partner_id { get; set; }

        public int slot_id { get; set; }

        public DateTime selected_date { get; set; }

        public string status { get; set; }
    }

    // 🎯 Reject Reason
    [Table("tbl_rejectReason", Schema = "deliverypartner")]
    public class tbl_rejectReason
    {
        [Key]
        public int reason_id { get; set; }

        public string Reason { get; set; }
    }

    // 🎯 Wallet
    [Table("tbl_wallet", Schema = "deliverypartner")]
    public class tbl_wallet
    {
        [Key]
        public int WalletID { get; set; }

        public int DeliveryPartnerID { get; set; }

        public decimal Balance { get; set; }
    }

    // 🎯 Delivery Partner Payment Details
    [Table("tbl_deliveryPartnerPaymentDetails", Schema = "deliverypartner")]
    public class tbl_deliveryPartnerPaymentDetails
    {
        [Key]
        public int PaymentDetailID { get; set; }

        public int partner_id { get; set; }

        public string PaymentType { get; set; }

        public string UPI_ID { get; set; }

        public string BankName { get; set; }

        public string AccountNumber { get; set; }

        public string IFSC_Code { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
