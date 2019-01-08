using System.CodeDom.Compiler;

namespace N
{
    /// <summary>
    /// An interface for all types generated from the S schema.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public interface ISNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISNode" />.
        /// </summary>
        SNodeKind SNodeKind { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISNode DeepClone();
    }
}