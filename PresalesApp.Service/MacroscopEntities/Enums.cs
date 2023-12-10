using System.Reflection;

namespace PresalesApp.Service.MacroscopEntities;

public class Enums
{
    public enum Gender
    {
        [StringValue("Неопределён")]
        Unknown,
        [StringValue("Женский")]
        Woman,
        [StringValue("Мужской")]
        Man
    }

    public enum Emotion
    {
        [StringValue("Негативные")]
        Sadness,
        [StringValue("Нейтральные")]
        Neutral,
        [StringValue("Позитивные")]
        Happiness
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class StringValue(string value) : Attribute
    {
        public string Value { get; } = value;
    }

    public static class StringEnum
    {
        public static string? GetStringValue(Enum value)
        {
            string? output = null;
            var type = value.GetType();
            var fi = type.GetField(value.ToString());

            if(fi == null)
            {
                return null;
            }

            if(fi.GetCustomAttributes(typeof(StringValue), false) is not StringValue[] attrs)
            {
                return null;
            }

            if(attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }
    }
}
