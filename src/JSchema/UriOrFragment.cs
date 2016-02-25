// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.JSchema
{
    /// <summary>
    /// Represents a value that is either a URI reference that is valid according to RFC 2396,
    /// or a bare fragment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is necessary because JSON Schema relies on URI references that consist
    /// of only a fragment (a string of the form #fragment, which we will refer to as a
    /// "bare fragment"). A bare fragment is a valid URI reference according to RFC 3986,
    /// but not according to RFC 2396. For backwards compatibility with versions of .NET
    /// going back to 1.0, the <see cref="System.Uri"/> class conforms to RFC 2396 and has
    /// not been updated to fully support RFC 3986. Therefore it cannot represent a bare
    /// fragment. If you try to construct a System.Uri from a bare fragment, the constructor
    /// throws <see cref="UriFormatException"/>.)
    /// </para>
    /// <para>
    /// This class does not fully represent an RFC 3986 URI reference, which is why it is
    /// not named (for example) Rfc3986Uri. It simply accommodates JSON Schema's requirement
    /// to handle bare fragments.
    /// </para>
    /// </remarks>
    [TypeConverter(typeof(UriOrFragmentTypeConverter))]
    public class UriOrFragment: IEquatable<UriOrFragment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriOrFragment"/> class.
        /// </summary>
        /// <param name="uriString">
        /// A string containing either a URI reference that is valid according to RFC 2396,
        /// or a bare fragment.
        /// </param>
        public UriOrFragment(string uriString)
        {
            // TODO: Make sure we handle URI-escaped # properly. Should it match or not?
            if (uriString.StartsWith("#"))
            {
                Fragment = uriString;
            }
            else
            {
                Uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object represents a URI reference that is
        /// valid according to RFC 2396.
        /// </summary>
        public bool IsUri => Uri != null;

        /// <summary>
        /// Gets a value indicating whether this object represents a bare fragment.
        /// </summary>
        public bool IsFragment => Fragment != null;

        /// <summary>
        /// Gets the URI reference represented by this object, or null if this object does
        /// not represent a URI reference that is valid according to RFC 2396.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets the bare fragment represented by this object, or null if this object does
        /// not represent a bare fragment.
        /// </summary>
        public string Fragment { get; }

        #region Object

        /// <summary>
        /// Compares two <see cref="UriOrFragment"/> instances for equality.
        /// </summary>
        /// <param name="comparand">
        /// The UriOrFragment instance to compare with this instance.
        /// </param>
        /// <returns>
        /// True if this instance and <paramref name="comparand"/> are equal; otherwise false.
        /// </returns>
        public override bool Equals(object comparand)
        {
            return Equals(comparand as UriOrFragment);
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>
        /// The hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return IsFragment ? Fragment.GetHashCode() : Uri.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>
        /// A string representing either a URI reference, if this object represents a URI
        /// reference that is valid according to RFC 2396, or a bare fragment.
        /// </returns>
        public override string ToString()
        {
            return IsFragment ? Fragment : Uri.OriginalString;
        }

        #endregion Object

        #region IComparable<T>

        public bool Equals(UriOrFragment other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (IsFragment != other.IsFragment || IsUri != other.IsUri)
            {
                return false;
            }

            // Uri.Equals does not compare fragments on absolute URIs (although it does
            // compare them on relative URIs). We always want to compare the fragments.
            return IsFragment
                ? Fragment.Equals(other.Fragment)
                : Uri.EqualsWithFragments(other.Uri);
        }

        #endregion IComparable<T>

        public static bool operator ==(UriOrFragment left, UriOrFragment right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((object)left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(UriOrFragment left, UriOrFragment right)
        {
            return !(left == right);
        }
    }
}