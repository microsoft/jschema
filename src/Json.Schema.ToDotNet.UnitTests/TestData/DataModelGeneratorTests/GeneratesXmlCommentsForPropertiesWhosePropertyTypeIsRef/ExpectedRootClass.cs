using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    /// <summary>
    /// Describes a console window.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class ConsoleWindow
    {
        public static IEqualityComparer<ConsoleWindow> ValueComparer => ConsoleWindowEqualityComparer.Instance;

        public bool ValueEquals(ConsoleWindow other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// The color of the text on the screen.
        /// </summary>
        [DataMember(Name = "foregroundColor", IsRequired = false, EmitDefaultValue = false)]
        public Color ForegroundColor { get; set; }

        /// <summary>
        /// The color of the screen background.
        /// </summary>
        [DataMember(Name = "backgroundColor", IsRequired = false, EmitDefaultValue = false)]
        public Color BackgroundColor { get; set; }
    }
}