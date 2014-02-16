/*
 * [The "BSD license"]
 *  Copyright (c) 2013 Terence Parr
 *  Copyright (c) 2013 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// A tree structure used to record the semantic context in which
    /// an ATN configuration is valid.
    /// </summary>
    /// <remarks>
    /// A tree structure used to record the semantic context in which
    /// an ATN configuration is valid.  It's either a single predicate,
    /// a conjunction
    /// <code>p1&&p2</code>
    /// , or a sum of products
    /// <code>p1||p2</code>
    /// .
    /// <p>I have scoped the
    /// <see cref="AND">AND</see>
    /// ,
    /// <see cref="OR">OR</see>
    /// , and
    /// <see cref="Predicate">Predicate</see>
    /// subclasses of
    /// <see cref="SemanticContext">SemanticContext</see>
    /// within the scope of this outer class.</p>
    /// </remarks>
    public abstract class SemanticContext
    {
        /// <summary>
        /// The default
        /// <see cref="SemanticContext">SemanticContext</see>
        /// , which is semantically equivalent to
        /// a predicate of the form
        /// <code></code>
        /// 
        /// true}?}.
        /// </summary>
        public static readonly SemanticContext None = new SemanticContext.Predicate();

        /// <summary>
        /// For context independent predicates, we evaluate them without a local
        /// context (i.e., null context).
        /// </summary>
        /// <remarks>
        /// For context independent predicates, we evaluate them without a local
        /// context (i.e., null context). That way, we can evaluate them without
        /// having to create proper rule-specific context during prediction (as
        /// opposed to the parser, which creates them naturally). In a practical
        /// sense, this avoids a cast exception from RuleContext to myruleContext.
        /// <p>For context dependent predicates, we must pass in a local context so that
        /// references such as $arg evaluate properly as _localctx.arg. We only
        /// capture context dependent predicates in the context in which we begin
        /// prediction, so we passed in the outer context here in case of context
        /// dependent predicate evaluation.</p>
        /// </remarks>
        public abstract bool Eval<_T0>(Recognizer<_T0> parser, RuleContext outerContext);

        /// <summary>Evaluate the precedence predicates for the context and reduce the result.</summary>
        /// <remarks>Evaluate the precedence predicates for the context and reduce the result.</remarks>
        /// <param name="parser">The parser instance.</param>
        /// <param name="outerContext">The current parser context object.</param>
        /// <returns>
        /// The simplified semantic context after precedence predicates are
        /// evaluated, which will be one of the following values.
        /// <ul>
        /// <li>
        /// <see cref="None">None</see>
        /// : if the predicate simplifies to
        /// <code>true</code>
        /// after
        /// precedence predicates are evaluated.</li>
        /// <li>
        /// <code>null</code>
        /// : if the predicate simplifies to
        /// <code>false</code>
        /// after
        /// precedence predicates are evaluated.</li>
        /// <li>
        /// <code>this</code>
        /// : if the semantic context is not changed as a result of
        /// precedence predicate evaluation.</li>
        /// <li>A non-
        /// <code>null</code>
        /// 
        /// <see cref="SemanticContext">SemanticContext</see>
        /// : the new simplified
        /// semantic context after precedence predicates are evaluated.</li>
        /// </ul>
        /// </returns>
        public virtual SemanticContext EvalPrecedence<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
        {
            return this;
        }

        public class Predicate : SemanticContext
        {
            public readonly int ruleIndex;

            public readonly int predIndex;

            public readonly bool isCtxDependent;

            protected internal Predicate()
            {
                // e.g., $i ref in pred
                this.ruleIndex = -1;
                this.predIndex = -1;
                this.isCtxDependent = false;
            }

            public Predicate(int ruleIndex, int predIndex, bool isCtxDependent)
            {
                this.ruleIndex = ruleIndex;
                this.predIndex = predIndex;
                this.isCtxDependent = isCtxDependent;
            }

            public override bool Eval<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                RuleContext localctx = isCtxDependent ? outerContext : null;
                return parser.Sempred(localctx, ruleIndex, predIndex);
            }

            public override int GetHashCode()
            {
                int hashCode = MurmurHash.Initialize();
                hashCode = MurmurHash.Update(hashCode, ruleIndex);
                hashCode = MurmurHash.Update(hashCode, predIndex);
                hashCode = MurmurHash.Update(hashCode, isCtxDependent ? 1 : 0);
                hashCode = MurmurHash.Finish(hashCode, 3);
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is SemanticContext.Predicate))
                {
                    return false;
                }
                if (this == obj)
                {
                    return true;
                }
                SemanticContext.Predicate p = (SemanticContext.Predicate)obj;
                return this.ruleIndex == p.ruleIndex && this.predIndex == p.predIndex && this.isCtxDependent == p.isCtxDependent;
            }

            public override string ToString()
            {
                return "{" + ruleIndex + ":" + predIndex + "}?";
            }
        }

        public class PrecedencePredicate : SemanticContext, IComparable<SemanticContext.PrecedencePredicate>
        {
            public readonly int precedence;

            protected internal PrecedencePredicate()
            {
                this.precedence = 0;
            }

            public PrecedencePredicate(int precedence)
            {
                this.precedence = precedence;
            }

            public override bool Eval<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                return parser.Precpred(outerContext, precedence);
            }

            public override SemanticContext EvalPrecedence<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                if (parser.Precpred(outerContext, precedence))
                {
                    return SemanticContext.None;
                }
                else
                {
                    return null;
                }
            }

            public virtual int CompareTo(SemanticContext.PrecedencePredicate o)
            {
                return precedence - o.precedence;
            }

            public override int GetHashCode()
            {
                int hashCode = 1;
                hashCode = 31 * hashCode + precedence;
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is SemanticContext.PrecedencePredicate))
                {
                    return false;
                }
                if (this == obj)
                {
                    return true;
                }
                SemanticContext.PrecedencePredicate other = (SemanticContext.PrecedencePredicate)obj;
                return this.precedence == other.precedence;
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// A semantic context which is true whenever none of the contained contexts
        /// is false.
        /// </summary>
        /// <remarks>
        /// A semantic context which is true whenever none of the contained contexts
        /// is false.
        /// </remarks>
        public class AND : SemanticContext
        {
            [NotNull]
            public readonly SemanticContext[] opnds;

            public AND(SemanticContext a, SemanticContext b)
            {
                HashSet<SemanticContext> operands = new HashSet<SemanticContext>();
                if (a is SemanticContext.AND)
                {
                    Sharpen.Collections.AddAll(operands, Arrays.AsList(((SemanticContext.AND)a).opnds));
                }
                else
                {
                    operands.AddItem(a);
                }
                if (b is SemanticContext.AND)
                {
                    Sharpen.Collections.AddAll(operands, Arrays.AsList(((SemanticContext.AND)b).opnds));
                }
                else
                {
                    operands.AddItem(b);
                }
                IList<SemanticContext.PrecedencePredicate> precedencePredicates = FilterPrecedencePredicates(operands);
                if (!precedencePredicates.IsEmpty())
                {
                    // interested in the transition with the lowest precedence
                    SemanticContext.PrecedencePredicate reduced = Sharpen.Collections.Min(precedencePredicates);
                    operands.AddItem(reduced);
                }
                opnds = Sharpen.Collections.ToArray(operands, new SemanticContext[operands.Count]);
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (!(obj is SemanticContext.AND))
                {
                    return false;
                }
                SemanticContext.AND other = (SemanticContext.AND)obj;
                return Arrays.Equals(this.opnds, other.opnds);
            }

            public override int GetHashCode()
            {
                return MurmurHash.HashCode(opnds, typeof(SemanticContext.AND).GetHashCode());
            }

            /// <summary>
            /// <inheritDoc></inheritDoc>
            /// <p>
            /// The evaluation of predicates by this context is short-circuiting, but
            /// unordered.</p>
            /// </summary>
            public override bool Eval<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                foreach (SemanticContext opnd in opnds)
                {
                    if (!opnd.Eval(parser, outerContext))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override SemanticContext EvalPrecedence<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                bool differs = false;
                IList<SemanticContext> operands = new List<SemanticContext>();
                foreach (SemanticContext context in opnds)
                {
                    SemanticContext evaluated = context.EvalPrecedence(parser, outerContext);
                    differs |= (evaluated != context);
                    if (evaluated == null)
                    {
                        // The AND context is false if any element is false
                        return null;
                    }
                    else
                    {
                        if (evaluated != None)
                        {
                            // Reduce the result by skipping true elements
                            operands.AddItem(evaluated);
                        }
                    }
                }
                if (!differs)
                {
                    return this;
                }
                if (operands.IsEmpty())
                {
                    // all elements were true, so the AND context is true
                    return None;
                }
                SemanticContext result = operands[0];
                for (int i = 1; i < operands.Count; i++)
                {
                    result = SemanticContext.And(result, operands[i]);
                }
                return result;
            }

            public override string ToString()
            {
                return Utils.Join(opnds, "&&");
            }
        }

        /// <summary>
        /// A semantic context which is true whenever at least one of the contained
        /// contexts is true.
        /// </summary>
        /// <remarks>
        /// A semantic context which is true whenever at least one of the contained
        /// contexts is true.
        /// </remarks>
        public class OR : SemanticContext
        {
            [NotNull]
            public readonly SemanticContext[] opnds;

            public OR(SemanticContext a, SemanticContext b)
            {
                HashSet<SemanticContext> operands = new HashSet<SemanticContext>();
                if (a is SemanticContext.OR)
                {
                    Sharpen.Collections.AddAll(operands, Arrays.AsList(((SemanticContext.OR)a).opnds));
                }
                else
                {
                    operands.AddItem(a);
                }
                if (b is SemanticContext.OR)
                {
                    Sharpen.Collections.AddAll(operands, Arrays.AsList(((SemanticContext.OR)b).opnds));
                }
                else
                {
                    operands.AddItem(b);
                }
                IList<SemanticContext.PrecedencePredicate> precedencePredicates = FilterPrecedencePredicates(operands);
                if (!precedencePredicates.IsEmpty())
                {
                    // interested in the transition with the highest precedence
                    SemanticContext.PrecedencePredicate reduced = Sharpen.Collections.Max(precedencePredicates);
                    operands.AddItem(reduced);
                }
                this.opnds = Sharpen.Collections.ToArray(operands, new SemanticContext[operands.Count]);
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (!(obj is SemanticContext.OR))
                {
                    return false;
                }
                SemanticContext.OR other = (SemanticContext.OR)obj;
                return Arrays.Equals(this.opnds, other.opnds);
            }

            public override int GetHashCode()
            {
                return MurmurHash.HashCode(opnds, typeof(SemanticContext.OR).GetHashCode());
            }

            /// <summary>
            /// <inheritDoc></inheritDoc>
            /// <p>
            /// The evaluation of predicates by this context is short-circuiting, but
            /// unordered.</p>
            /// </summary>
            public override bool Eval<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                foreach (SemanticContext opnd in opnds)
                {
                    if (opnd.Eval(parser, outerContext))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override SemanticContext EvalPrecedence<_T0>(Recognizer<_T0> parser, RuleContext outerContext)
            {
                bool differs = false;
                IList<SemanticContext> operands = new List<SemanticContext>();
                foreach (SemanticContext context in opnds)
                {
                    SemanticContext evaluated = context.EvalPrecedence(parser, outerContext);
                    differs |= (evaluated != context);
                    if (evaluated == None)
                    {
                        // The OR context is true if any element is true
                        return None;
                    }
                    else
                    {
                        if (evaluated != null)
                        {
                            // Reduce the result by skipping false elements
                            operands.AddItem(evaluated);
                        }
                    }
                }
                if (!differs)
                {
                    return this;
                }
                if (operands.IsEmpty())
                {
                    // all elements were false, so the OR context is false
                    return null;
                }
                SemanticContext result = operands[0];
                for (int i = 1; i < operands.Count; i++)
                {
                    result = SemanticContext.Or(result, operands[i]);
                }
                return result;
            }

            public override string ToString()
            {
                return Utils.Join(opnds, "||");
            }
        }

        public static SemanticContext And(SemanticContext a, SemanticContext b)
        {
            if (a == null || a == None)
            {
                return b;
            }
            if (b == null || b == None)
            {
                return a;
            }
            SemanticContext.AND result = new SemanticContext.AND(a, b);
            if (result.opnds.Length == 1)
            {
                return result.opnds[0];
            }
            return result;
        }

        /// <seealso cref="ParserATNSimulator.GetPredsForAmbigAlts(Sharpen.BitSet, ATNConfigSet, int)">ParserATNSimulator.GetPredsForAmbigAlts(Sharpen.BitSet, ATNConfigSet, int)</seealso>
        public static SemanticContext Or(SemanticContext a, SemanticContext b)
        {
            if (a == null)
            {
                return b;
            }
            if (b == null)
            {
                return a;
            }
            if (a == None || b == None)
            {
                return None;
            }
            SemanticContext.OR result = new SemanticContext.OR(a, b);
            if (result.opnds.Length == 1)
            {
                return result.opnds[0];
            }
            return result;
        }

        private static IList<SemanticContext.PrecedencePredicate> FilterPrecedencePredicates<_T0>(ICollection<_T0> collection)
            where _T0 : SemanticContext
        {
            List<SemanticContext.PrecedencePredicate> result = null;
            for (IEnumerator<SemanticContext> iterator = collection.GetEnumerator(); iterator.HasNext(); )
            {
                SemanticContext context = iterator.Next();
                if (context is SemanticContext.PrecedencePredicate)
                {
                    if (result == null)
                    {
                        result = new List<SemanticContext.PrecedencePredicate>();
                    }
                    result.AddItem((SemanticContext.PrecedencePredicate)context);
                    iterator.Remove();
                }
            }
            if (result == null)
            {
                return Sharpen.Collections.EmptyList();
            }
            return result;
        }
    }
}
