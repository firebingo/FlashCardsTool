using System;
using System.ComponentModel.DataAnnotations;

namespace FlashCards.Client.Components
{
	public class NotCompareAttribute : ValidationAttribute
	{
		public string OtherProperty { get; }
		public override bool RequiresValidationContext => true;

		public NotCompareAttribute(string propertyName)
		{
			OtherProperty = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		}

		public override string FormatErrorMessage(string name)
		{
			var errorMessage = $"{name} must not match {OtherProperty}";
			return ErrorMessage ?? errorMessage;
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			ArgumentNullException.ThrowIfNull(validationContext);
			var otherProperty = validationContext.ObjectType.GetProperty(OtherProperty)
				?? throw new NotSupportedException($"Can't find {OtherProperty} on searched type: {validationContext.ObjectType.Name}");
			var compareActualValue = otherProperty.GetValue(validationContext.ObjectInstance);

			if (compareActualValue is string cs && value is string v &&
				string.Equals(cs, v))
			{
				string[]? memberNames = validationContext.MemberName != null
				   ? [validationContext.MemberName]
				   : null;
				return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
			}
			else if (compareActualValue == value)
			{
				string[]? memberNames = validationContext.MemberName != null
				   ? [validationContext.MemberName]
				   : null;
				return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
			}

			return ValidationResult.Success;
		}
	}
}
