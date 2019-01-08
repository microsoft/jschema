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
            }

            return node;
        }
    }
}