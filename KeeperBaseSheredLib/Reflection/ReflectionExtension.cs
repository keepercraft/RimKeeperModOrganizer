using System.Reflection;
namespace KeeperBaseSheredLib.Reflection;

public static class ReflectionExtension
{

    public static T? GetPrivateProperty<T>(this object context, string property) => (T?)GetPrivateProperty(context, property);
    public static object? GetPrivateProperty(this object context, string property)
    {
        if (context == null) return null;
        var type = context.GetType();
        var prop = type.GetProperty(
            property,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        return prop?.GetValue(context);
    }

    public static T? GetField<T>(this object context, string property) => (T?)GetField(context, property);
    public static object? GetField(this object context, string field)
    {
        if (context == null) return null;
        var type = context.GetType();
        var f = type.GetField(
            field,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        return f?.GetValue(context);
    }

    public static T? GetMember<T>(this object context, string property) => (T?)GetMember(context, property);
    public static object? GetMember(this object context, string name)
    {
        if (context == null) return null;
        var type = context.GetType();
        var prop = type.GetProperty(name,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop != null)
            return prop.GetValue(context);
        var field = type.GetField(name,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return field?.GetValue(context);
    }

    public static void SetPrivateProperty(this object context, string property, object value)
    {
        if (context == null) return;
        var type = context.GetType();
        var prop = type.GetProperty(
            property,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        prop?.SetValue(context, value);
    }

    public static void SetField(this object context, string field, object value)
    {
        if (context == null) return;
        var type = context.GetType();
        var f = type.GetField(
            field,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        f?.SetValue(context, value);
    }
}
