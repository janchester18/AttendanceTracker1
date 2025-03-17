using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class CashAdvanceReview
    {

        [Required]
        public CashAdvanceRequestStatus Status { get; set; } = CashAdvanceRequestStatus.Pending; // pending, approved, rejected

        [RequiredWhen("Status", CashAdvanceRequestStatus.Rejected, ErrorMessage = "Rejection reason is required when status is rejected.")]
        public string? RejectionReason { get; set; }

        [Required]
        public List<DateTime> PaymentDates { get; set; }
    }

    public class RequiredWhenAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _targetValue;

        public RequiredWhenAttribute(string dependentProperty, object targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dependentPropertyValue = validationContext.ObjectType.GetProperty(_dependentProperty)?.GetValue(validationContext.ObjectInstance);

            if (dependentPropertyValue?.Equals(_targetValue) == true && (value == null || string.IsNullOrWhiteSpace(value?.ToString())))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
