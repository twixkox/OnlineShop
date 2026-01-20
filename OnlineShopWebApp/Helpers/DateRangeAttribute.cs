using System.ComponentModel.DataAnnotations;

namespace OnlineShopWebApp.Helpers
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly DateOnly minDate;
        private readonly DateOnly maxDate;

        public DateRangeAttribute()
        {
            minDate = DateOnly.FromDateTime(DateTime.Now);
            maxDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(3));

            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = $"Дата должна быть от {minDate.ToShortDateString()} до {maxDate.ToShortDateString()}";
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(ErrorMessage);

            DateOnly date = (DateOnly)value;

            if (date < minDate || date > maxDate)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}