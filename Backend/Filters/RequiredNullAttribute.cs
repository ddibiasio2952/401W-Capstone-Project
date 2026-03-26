using System.ComponentModel.DataAnnotations;

namespace FalveyInsuranceGroup.Backend.Filters
{
    /// <summary>
    /// Field-level attribute ensures a value is null
    /// </summary>
    public class RequiredNullAttribute : ValidationAttribute
    {
        public RequiredNullAttribute()
        {
            ErrorMessage = "ERROR: Field must not be given";
        }

        /// <summary>
        /// Checks to see if the value is null
        /// </summary>
        /// <param name="value">The member field being checked</param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // return the error message if a value is not null
            if (value != null)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success; // Success if null
        }


    }
}