using System.Reflection;
using System.Text.Json;

namespace TestSyncProg.Helpers;

public static class JsonDeserializeHelper
{
    public static T Deserialize<T>(JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Null:
                return default(T); // will be null for ref types
            case JsonValueKind.Number:
                if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(sbyte?))
                    return (T)(object)jsonElement.GetSByte();
                if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
                    return (T)(object)jsonElement.GetInt16();
                if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                    return (T)(object)jsonElement.GetInt32();
                if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                    return (T)(object)jsonElement.GetInt64();
                if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
                    return (T)(object)jsonElement.GetByte();
                if (typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?))
                    return (T)(object)jsonElement.GetUInt16();
                if (typeof(T) == typeof(uint) || typeof(T) == typeof(uint?))
                    return (T)(object)jsonElement.GetUInt32();
                if (typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?))
                    return (T)(object)jsonElement.GetUInt64();
                else if (typeof(T) == typeof(float) || typeof(T) == typeof(float?) ||
                    typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                    return (T)(object)jsonElement.GetDouble();
                else if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                    return (T)(object)jsonElement.GetDecimal();
                throw new ArgumentException();
            case JsonValueKind.True:
                if (typeof(T) != typeof(bool) && typeof(T) != typeof(bool?))
                {
                    throw new ArgumentException();
                }
                return (T)(object)true;
            case JsonValueKind.False:
                if (typeof(T) != typeof(bool) && typeof(T) != typeof(bool?))
                {
                    throw new ArgumentException();
                }
                return (T)(object)false;
            case JsonValueKind.Array:
                // Use ParseArrayOrNull instead!
                throw new ArgumentException();
        }

        if (jsonElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException();
        }

        string name = jsonElement.GetProperty("class").GetString();

        Type currType = typeof(T);
        Assembly assembly = Assembly.GetAssembly(currType);

        Type deserializedType = assembly.GetType(name);
        Type requestedType = typeof(T);

        if (!requestedType.IsAssignableFrom(deserializedType))
        {
            throw new ArgumentException();
        }

        if (deserializedType.IsClass)
        {
            ConstructorInfo constructor = deserializedType.GetConstructor(new[] { typeof(JsonElement) });
            MethodInfo factory = deserializedType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);

            if (constructor == null && factory == null && !deserializedType.IsAbstract)
            {
                // Only abstract classes are allowed to not have deserializers at all
                throw new InvalidOperationException();
            }
            else if (constructor != null && factory == null && !deserializedType.IsAbstract)
            {
                // Only non-abstract classes use constructor deserializers, obviously
                return (T)constructor.Invoke(new object[] { jsonElement });
            }
            else if (constructor == null && factory != null)
            {
                // Abstract classes are allowed to have a static deserializer factory to do backwards compat
                // where that class was formerly non-abstract and has now been broken up into child classes
                // (see BrickLikeWallMaterial.Deserialize TZ release backwards compat as an example).
                return (T)factory.Invoke(null, new object[] { jsonElement });
            }
        }

        throw new InvalidOperationException();
    }

    public static T Deserialize<T>(JsonDocument jsonDocument)
    {
        return Deserialize<T>(jsonDocument.RootElement);
    }

    public static T Deserialize<T>(string json)
    {
        return Deserialize<T>(JsonDocument.Parse(json));
    }

    public static int GetInt32FieldOrDefault(this JsonElement jsonElement, string fieldName, int defVal = 0)
    {
        if (!jsonElement.TryGetProperty(fieldName, out JsonElement val))
        {
            return defVal;
        }

        if (!val.TryGetInt32(out int res))
        {
            return defVal;
        }

        return res;
    }

    public static long GetInt64FieldOrDefault(this JsonElement jsonElement, string fieldName, long defVal = 0)
    {
        if (!jsonElement.TryGetProperty(fieldName, out JsonElement val))
        {
            return defVal;
        }

        if (!val.TryGetInt64(out long res))
        {
            return defVal;
        }

        return res;
    }

    public static string GetStringFieldOrDefault(this JsonElement jsonElement, string fieldName, string defVal = null)
    {
        if (!jsonElement.TryGetProperty(fieldName, out JsonElement val))
        {
            return defVal;
        }

        return val.GetString();
    }
}
