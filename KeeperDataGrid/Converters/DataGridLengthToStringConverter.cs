using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
namespace KeeperDataGrid.Converters;

//public class DataGridLengthToStringConverter : IValueConverter
//{
//    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if(value is string input && !string.IsNullOrWhiteSpace(input))
//        {
//            var cvt = new DataGridLengthConverter(); 
//            return cvt.ConvertFrom(input) ?? new DataGridLength(1, DataGridLengthUnitType.Auto);
//        }
//        return new DataGridLength(1, DataGridLengthUnitType.Auto);
//    }
//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is DataGridLength length)
//        {
//            var cvt = new DataGridLengthConverter();
//            var result = cvt.ConvertToString(length);
//            return result ?? "Auto";
//        }
//        return "Auto";
//    }
//}

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

public class DataGridLengthToStringConverter : IValueConverter
{
    // Z Modelu (string) do UI (DataGridLength)
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        string? val = value as string;
        if (string.IsNullOrWhiteSpace(val))
            return new DataGridLength(1, DataGridLengthUnitType.Star);

        val = val.Trim().ToLowerInvariant();

        // 1. Obsługa wartości specjalnych
        if (val == "auto") return DataGridLength.Auto;
        if (val == "sizetocells") return DataGridLength.SizeToCells;
        if (val == "sizetoheader") return DataGridLength.SizeToHeader;

        // 2. Obsługa Gwiazdki (*)
        if (val.EndsWith("*"))
        {
            string numberPart = val.TrimEnd('*');
            if (string.IsNullOrEmpty(numberPart))
                return new DataGridLength(1, DataGridLengthUnitType.Star); // same "*" to "1*"

            if (double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double weight))
            {
                // OCHRONA PRZED NaN i nieskończonością!
                if (double.IsNaN(weight) || double.IsInfinity(weight) || weight <= 0)
                    return new DataGridLength(1, DataGridLengthUnitType.Star);

                return new DataGridLength(weight, DataGridLengthUnitType.Star);
            }
            return new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        // 3. Obsługa zwykłych pikseli (Absolute)
        if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double pixels))
        {
            if (double.IsNaN(pixels) || double.IsInfinity(pixels) || pixels < 0)
                return new DataGridLength(100, DataGridLengthUnitType.Pixel); // bezpieczne 100px

            return new DataGridLength(pixels, DataGridLengthUnitType.Pixel);
        }

        return new DataGridLength(1, DataGridLengthUnitType.Star);
    }

    // Z UI (DataGridLength) do Modelu (string)
    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataGridLength length)
        {
            if (length.IsAuto) return "Auto";
            if (length.IsSizeToCells) return "SizeToCells";
            if (length.IsSizeToHeader) return "SizeToHeader";

            // KLUCZOWE ZABEZPIECZENIE: Zapobiega rzucaniu NaNMeasure do modelu
            if (double.IsNaN(length.Value) || double.IsInfinity(length.Value))
                return "1*";

            if (length.IsStar)
            {
                // ROUNDING ZATRZYMUJE NIESKOŃCZONĄ PĘTLĘ ZAWIESZAJĄCĄ WIDOK!
                double roundedWeight = Math.Round(length.Value, 4);
                return roundedWeight.ToString(CultureInfo.InvariantCulture) + "*";
            }

            if (length.IsAbsolute)
            {
                double roundedPixels = Math.Round(length.Value, 0); // Piksele nie potrzebują ułamków
                return roundedPixels.ToString(CultureInfo.InvariantCulture);
            }
        }
        return "Auto";
    }
}