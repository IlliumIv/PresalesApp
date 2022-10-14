using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Entities.Helpers
{
    [AttributeUsage(AttributeTargets.All)]
    public class JsonIgnoreSerializationAttribute : Attribute { }
    public class JsonPropertiesResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return objectType.GetProperties()
                             .Where(pi => !Attribute.IsDefined(pi, typeof(JsonIgnoreSerializationAttribute)))
                             .ToList<MemberInfo>();
        }
    }
}
