using System.Data;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters
{
    public static class AdapterExtension
    {
        public static T? SafeField<T>(this DataRow row, string columnName)
        {
            // 1) Si no existe la columna, devolvemos default
            if (!row.Table.Columns.Contains(columnName))
                return default;

            // 2) Leemos el valor bruto
            var val = row[columnName];
            if (val is DBNull || val is null)
                return default;

            // 3) Determinamos el tipo de destino, desenvolviendo nullables
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // 4) Si es enum, intentamos parsear desde la cadena
            if (targetType.IsEnum)
            {
                var str = val.ToString();
                if (Enum.TryParse(targetType, str, ignoreCase: true, out var enumValObj))
                {
                    // Comprueba que enumValObj realmente corresponde a un nombre existente:
                    if (!Enum.IsDefined(targetType, enumValObj))
                        return default;    // “5000” no está definido → devolvemos default

                    return (T?)enumValObj;
                }
                return default;
            }

            // 5) Si ya es del tipo correcto, casteamos directo
            if (targetType.IsInstanceOfType(val))
                return (T)val!;

            // 6) Si es convertible a través de Convert.ChangeType, lo intentamos
            try
            {
                object? converted = Convert.ChangeType(val, targetType);
                return (T?)converted;
            }
            catch
            {
                // Si falla la conversión (p.ej. "abc" a int), devolvemos default
                return default;
            }
        }
    }
}
