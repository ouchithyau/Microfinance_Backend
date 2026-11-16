using System.Text.RegularExpressions;

namespace Microfinance.Application.Features.Customers.Helpers;

public static class SriLankanNicHelper
{
    public static (bool IsValid, string NormalizedNic, string LegacyOrNewEquivalent, DateTime DateOfBirth, string Gender, int Age, string Error) ParseAndValidate(string rawNic)
    {
        if (string.IsNullOrWhiteSpace(rawNic))
            return (false, string.Empty, string.Empty, default, string.Empty, 0, "NIC number is required.");

        var nic = rawNic.Trim().ToUpperInvariant();
        int year;
        int dayOfYear;
        string equivalent;

        if (Regex.IsMatch(nic, @"^\d{9}[VX]$"))
        {
            year = 1900 + int.Parse(nic.Substring(0, 2));
            dayOfYear = int.Parse(nic.Substring(2, 3));
            equivalent = $"{year}{nic.Substring(2, 3)}0{nic.Substring(5, 4)}";
        }
        else if (Regex.IsMatch(nic, @"^\d{12}$"))
        {
            year = int.Parse(nic.Substring(0, 4));
            dayOfYear = int.Parse(nic.Substring(4, 3));
            equivalent = year < 2000 && nic[7] == '0'
                ? $"{nic.Substring(2, 2)}{nic.Substring(4, 3)}{nic.Substring(8, 4)}V"
                : nic;
        }
        else
        {
            return (false, nic, string.Empty, default, string.Empty, 0, "Invalid Sri Lankan NIC format. Use 9 digits + V/X or 12 digits.");
        }

        string gender = "Male";
        if (dayOfYear > 500)
        {
            gender = "Female";
            dayOfYear -= 500;
        }

        if (dayOfYear < 1 || dayOfYear > 366)
            return (false, nic, equivalent, default, string.Empty, 0, "Invalid day code embedded in NIC.");

        var dob = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(dayOfYear - 1);
        var today = DateTime.UtcNow.Date;
        int age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;

        if (age < 18 || age > 65)
            return (false, nic, equivalent, dob, gender, age, $"Age Eligibility Policy: Customer age ({age}) must be between 18 and 65 years.");

        return (true, nic, equivalent, dob, gender, age, string.Empty);
    }
}