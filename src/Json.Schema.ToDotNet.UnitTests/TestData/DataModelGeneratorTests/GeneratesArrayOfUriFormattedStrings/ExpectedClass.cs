using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace N
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public partial class C : ISNode
    {
        public static IEqualityComparer<C> ValueComparer => CEqualityComparer.Instance;

        public bool ValueEquals(C other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISNode" />.
        /// </summary>
        public SNodeKind SNodeKind
        {
            get
            {
                return SNodeKind.C;
            }
        }

        [DataMember(Name = "uriFormattedStrings", IsRequired = false, EmitDefaultValue = false)]
        public IList<Uri> UriFormattedStrings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="C" /> class.
        /// </summary>
        public C()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C" /> class from the supplied values.
        /// </summary>
        /// <param name="uriFormattedStrings">
        /// An initialization value for the <see cref="P:UriFormattedStrings" /> property.
        /// </param>
        public C(IEnumerable<Uri> uriFormattedStrings)
        {
            Init(uriFormattedStrings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public C(C other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.UriFormattedStrings);
        }

        ISNode ISNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public C DeepClone()
        {
            return (C)DeepCloneCore();
        }

        private ISNode DeepCloneCore()
        {
            return new C(this);
        }

        private void Init(IEnumerable<Uri> uriFormattedStrings)
        {
            if (uriFormattedStrings != null)
            {
                var destination_0 = new List<Uri>();
                foreach (var value_0 in uriFormattedStrings)
                {
                    destination_0.Add(value_0);
                }

                UriFormattedStrings = destination_0;
            }
        }
    }
}