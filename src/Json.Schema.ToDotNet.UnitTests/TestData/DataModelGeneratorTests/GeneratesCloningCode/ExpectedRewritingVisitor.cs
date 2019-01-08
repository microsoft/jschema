using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    /// <summary>
    /// Rewriting visitor for the S object model.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "$JSchemaFileVersion$")]
    public abstract class SRewritingVisitor
    {
        /// <summary>
        /// Starts a rewriting visit of a node in the S object model.
        /// </summary>
        /// <param name="node">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object Visit(ISNode node)
        {
            return this.VisitActual(node);
        }

        /// <summary>
        /// Visits and rewrites a node in the S object model.
        /// </summary>
        /// <param name="node">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object VisitActual(ISNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            switch (node.SNodeKind)
            {
                case SNodeKind.C:
                    return VisitC((C)node);
                case SNodeKind.D:
                    return VisitD((D)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node) where T : class, ISNode
        {
            if (node == null)
            {
                return null;
            }

            return (T)Visit(node);
        }

        public virtual C VisitC(C node)
        {
            if (node != null)
            {
                node.ReferencedTypeProp = VisitNullChecked(node.ReferencedTypeProp);
                if (node.ArrayOfRefProp != null)
                {
                    for (int index_0 = 0; index_0 < node.ArrayOfRefProp.Count; ++index_0)
                    {
                        node.ArrayOfRefProp[index_0] = VisitNullChecked(node.ArrayOfRefProp[index_0]);
                    }
                }

                if (node.ArrayOfArrayProp != null)
                {
                    for (int index_0 = 0; index_0 < node.ArrayOfArrayProp.Count; ++index_0)
                    {
                        var value_0 = node.ArrayOfArrayProp[index_0];
                        if (value_0 != null)
                        {
                            for (int index_1 = 0; index_1 < value_0.Count; ++index_1)
                            {
                                value_0[index_1] = VisitNullChecked(value_0[index_1]);
                            }
                        }
                    }
                }

                if (node.DictionaryWithObjectSchemaProp != null)
                {
                    var keys = node.DictionaryWithObjectSchemaProp.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.DictionaryWithObjectSchemaProp[key];
                        if (value != null)
                        {
                            node.DictionaryWithObjectSchemaProp[key] = VisitNullChecked(value);
                        }
                    }
                }

                if (node.DictionaryWithObjectArraySchemaProp != null)
                {
                    var keys = node.DictionaryWithObjectArraySchemaProp.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.DictionaryWithObjectArraySchemaProp[key];
                        if (value != null)
                        {
                            for (int index_0 = 0; index_0 < node.DictionaryWithObjectArraySchemaProp[key].Count; ++index_0)
                            {
                                node.DictionaryWithObjectArraySchemaProp[key][index_0] = VisitNullChecked(node.DictionaryWithObjectArraySchemaProp[key][index_0]);
                            }
                        }
                    }
                }

                if (node.DictionaryWithUriKeyProp != null)
                {
                    var keys = node.DictionaryWithUriKeyProp.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.DictionaryWithUriKeyProp[key];
                        if (value != null)
                        {
                            node.DictionaryWithUriKeyProp[key] = VisitNullChecked(value);
                        }
                    }
                }
            }

            return node;
        }

        public virtual D VisitD(D node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}