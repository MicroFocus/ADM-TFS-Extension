using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PSModule.ParallelRunner.SDK.Util
{
    public class FieldsContractResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = base.GetSerializableMembers(objectType);
            return members.Where(m => m.MemberType == MemberTypes.Field).ToList();
        }
    }
}
