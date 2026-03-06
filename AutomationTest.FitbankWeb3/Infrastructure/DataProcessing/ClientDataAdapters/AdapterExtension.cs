using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

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

        // Si ya es del tipo correcto (ej. DB devuelve double/decimal)
        if (targetType.IsInstanceOfType(val))
            return (T)val!;

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

        // Si el valor viene como string y queremos parsear
        if (val is string s)
        {
            s = s.Trim();

            // Normalizar y limpiar la cadena: quitar símbolos no numéricos excepto . , - y paréntesis
            bool isNegative = false;
            if (s.StartsWith("(") && s.EndsWith(")"))
            {
                isNegative = true;
                s = s.Substring(1, s.Length - 2);
            }

            s = s.Trim();
            // eliminar espacios y símbolos de moneda comunes
            s = Regex.Replace(s, @"\s+", "");
            s = Regex.Replace(s, @"[^\d\.,\-]", ""); // queda sólo dígitos, punto, coma y guión

            // lógica para decidir separador decimal:
            int lastDot = s.LastIndexOf('.');
            int lastComma = s.LastIndexOf(',');

            if (lastDot >= 0 && lastComma >= 0)
            {
                // si ambos existen, el que aparezca más a la derecha suele ser el decimal
                if (lastDot > lastComma)
                {
                    // punto = decimal, quitar todas las comas (separadores de miles)
                    s = s.Replace(",", "");
                }
                else
                {
                    // coma = decimal, quitar puntos (miles) y convertir coma->punto
                    s = s.Replace(".", "").Replace(",", ".");
                }
            }
            else if (lastComma >= 0) // solo coma presente
            {
                int decimals = s.Length - lastComma - 1;
                // si hay 3 dígitos después de la coma, puede ser separador de miles (ej "1,999"),
                // si hay 2 o distinto a 3, usualmente es decimal ("999,99").
                if (decimals == 3)
                    s = s.Replace(",", ""); // 1,999 -> 1999
                else
                    s = s.Replace(",", "."); // 999,99 -> 999.99
            }
            else if (lastDot >= 0) // solo punto presente
            {
                int decimals = s.Length - lastDot - 1;
                // si hay 3 dígitos después del punto, puede ser separador de miles ("1.999")
                if (decimals == 3)
                    s = s.Replace(".", ""); // 1.999 -> 1999
                // si no, dejamos el punto como decimal (ej 999.99)
            }

            if (isNegative && !s.StartsWith("-"))
                s = "-" + s;

            var numberStyles = NumberStyles.Any;

            // double
            if (targetType == typeof(double))
            {
                if (double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out var d))
                    return (T?)(object)d;
                return default;
            }

            // decimal
            if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out var dec))
                    return (T?)(object)dec;
                return default;
            }

            // float
            if (targetType == typeof(float))
            {
                if (float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out var f))
                    return (T?)(object)f;
                return default;
            }

            // integer types (int, long, short)
            if (targetType == typeof(int))
            {
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    return (T?)(object)i;
                return default;
            }
            if (targetType == typeof(long))
            {
                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                    return (T?)(object)l;
                return default;
            }
            if (targetType == typeof(short))
            {
                if (short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sh))
                    return (T?)(object)sh;
                return default;
            }

            // DateTime
            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return (T?)(object)dt;
                if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                    return (T?)(object)dt;
                return default;
            }

            // Guid
            if (targetType == typeof(Guid))
            {
                if (Guid.TryParse(s, out var g)) return (T?)(object)g;
                return default;
            }

            // string
            if (targetType == typeof(string))
                return (T?)(object)s;
        }

        // Fallback general con CultureInfo.InvariantCulture, y luego CurrentCulture
        try
        {
            object? converted = Convert.ChangeType(val, targetType, CultureInfo.InvariantCulture);
            return (T?)converted;
        }
        catch
        {
            try
            {
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