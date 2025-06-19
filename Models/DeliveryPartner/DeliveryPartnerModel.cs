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

        public int vehicle_id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [MaxLength(20)]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid mobile number")]
        public string ContactNumber { get; set; }

        [Required]
        public string Status { get; set; } = "Inactive";

        public DateTime CreatedAT { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public bool Isavailable { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        public bool isApprov { get; set; }

        public bool isOnline { get; set; }
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

        public string ResLat { get; set; }

        public string ResLag { get; set; }

        public string CustLat { get; set; }
        public string CustLag { get; set; }
        public string EstimatedDeliveryTime { get; set; }
    }

    // 🎯 Delivery Earnings
    [Table("tbl_deliveryEarnings", Schema = "deliverypartner")]
    public class tbl_deliveryEarnings
    {
        [Key]
        public int earning_id { get; set; }
       public int TotalOrders { get; set; }
        public int partner_id { get; set; }

        public int request_id { get; set; }
        public int mode_id { get; set; }

        [Required]
        public decimal Earnings { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentStatus { get; set; }
 
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
        public int partner_id { get; set; }
        public int order_id { get; set; }
        public string ReasonDescription { get; set; }
        public DateTime RejectedAt { get; set; }
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

    [Table("tbl_deliveryDocuments", Schema = "deliverypartner")]
    public class tbl_deliveryDocuments
    {
        [Key]
        public int document_id { get; set; }
        public int partner_id { get; set; }
        public string AadhaarCard { get; set; }
        public string PANCard { get; set; }
        public string DrivingLicense { get; set; }
        public string RCBook { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
