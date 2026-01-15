using System.Data;
using System.Globalization;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters
{
    public static class AdapterExtension
    {
        public static T? SafeField<T>(this DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return default;

            var val = row[columnName];
            if (val is DBNull || val is null)
                return default;

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // Enums
            if (targetType.IsEnum)
            {
                var str = val.ToString();
                if (Enum.TryParse(targetType, str, ignoreCase: true, out var enumValObj))
                {
                    if (!Enum.IsDefined(targetType, enumValObj))
                        return default;
                    return (T?)enumValObj;
                }
                return default;
            }

            // Si ya es del tipo correcto (ej. DB devuelve double/decimal)
            if (targetType.IsInstanceOfType(val))
                return (T)val!;

            // Si el valor proviene como string y queremos un número, parsear con InvariantCulture primero
            if (val is string s)
            {
                var numberStyles = System.Globalization.NumberStyles.Any;
                // double
                if (targetType == typeof(double))
                {
                    if (double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out var d)) return (T?)(object)d;
                    if (double.TryParse(s, numberStyles, CultureInfo.CurrentCulture, out d)) return (T?)(object)d;
                    return default;
                }
                // decimal
                if (targetType == typeof(decimal))
                {
                    if (decimal.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out var dec)) return (T?)(object)dec;
                    if (decimal.TryParse(s, numberStyles, CultureInfo.CurrentCulture, out dec)) return (T?)(object)dec;
                    return default;
                }
                // float
                if (targetType == typeof(float))
                {
                    if (float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out var f)) return (T?)(object)f;
                    if (float.TryParse(s, numberStyles, CultureInfo.CurrentCulture, out f)) return (T?)(object)f;
                    return default;
                }
                // integer types
                if (targetType == typeof(int))
                {
                    if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return (T?)(object)i;
                    if (int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out i)) return (T?)(object)i;
                    return default;
                }
                if (targetType == typeof(long))
                {
                    if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) return (T?)(object)l;
                    if (long.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out l)) return (T?)(object)l;
                    return default;
                }
                if (targetType == typeof(short))
                {
                    if (short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sh)) return (T?)(object)sh;
                    if (short.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out sh)) return (T?)(object)sh;
                    return default;
                }

                // DateTime
                if (targetType == typeof(DateTime))
                {
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return (T?)(object)dt;
                    if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt)) return (T?)(object)dt;
                    return default;
                }

                // Guid
                if (targetType == typeof(Guid))
                {
                    if (Guid.TryParse(s, out var g)) return (T?)(object)g;
                    return default;
                }

                // Si es string -> devuelve directamente
                if (targetType == typeof(string))
                    return (T?)(object)s;
            }

            // Fallback general con CultureInfo.InvariantCulture
            try
            {
                object? converted = Convert.ChangeType(val, targetType, CultureInfo.InvariantCulture);
                return (T?)converted;
            }
            catch
            {
                try
                {
                    // Intento final usando la cultura actual
                    object? converted = Convert.ChangeType(val, targetType, CultureInfo.CurrentCulture);
                    return (T?)converted;
                }
                catch
                {
                    return default;
                }
            }
        }
    }
}
