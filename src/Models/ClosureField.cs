using System.Collections.Generic;
using System.Reflection;

namespace ReflectionToIL.Models
{
    /// <summary>
    /// A simple model that wraps a <see cref="FieldInfo"/> instance and a list of its parents from a given root
    /// </summary>
    public sealed class ClosureField
    {
        /// <summary>
        /// The wrapped <see cref="FieldInfo"/> instance
        /// </summary>
        public FieldInfo Info { get; }

        /// <summary>
        /// The list of <see cref="FieldInfo"/> values to traverse the closure class hierarchy and reach the target field
        /// </summary>
        public IReadOnlyList<FieldInfo> Parents { get; }

        public ClosureField(FieldInfo info, IReadOnlyList<FieldInfo> parents)
        {
            Info = info;
            Parents = parents;
        }
    }
}
