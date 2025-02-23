using System.ComponentModel.DataAnnotations;

namespace ContaBle.Attributes
{
    public class DniAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            var dni = value.ToString()!.ToUpper();
            if (dni.Length != 9)
            {
                return false;
            }

            var numbers = dni.Substring(0, 8);
            var letter = dni[8];

            if (!int.TryParse(numbers, out int dniNumber))
            {
                return false;
            }

            var validLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
            var calculatedLetter = validLetters[dniNumber % 23];

            return letter == calculatedLetter;
        }
    }
}
