using System.Collections.Generic;
using System.Reflection;

namespace ReflectionToIL.Models
{
    public sealed class ClosureField
    {
        public FieldInfo Info { get; }

        public IReadOnlyList<FieldInfo> Parents { get; }

        public ClosureField(FieldInfo info, IReadOnlyList<FieldInfo> parents)
        {
            Info = info;
            Parents = parents;
        }
    }
}
