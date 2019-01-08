using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// A set of values for all the types that implement <see cref="ISNode" />.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public enum SNodeKind
    {
        /// <summary>
        /// An uninitialized kind.
        /// </summary>
        None,
        /// <summary>
        /// A value indicating that the <see cref="ISNode" /> object is of type <see cref="C" />.
        /// </summary>
        C,
        /// <summary>
        /// A value indicating that the <see cref="ISNode" /> object is of type <see cref="D" />.
        /// </summary>
        D
    }
}