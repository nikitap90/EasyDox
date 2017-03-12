﻿using System.Collections.Generic;
using System.Linq;

namespace EasyDox
{
    public class Engine
    {
        private readonly Dictionary <string, IFuncN> functions;

        /// <summary>
        /// Creates the templating engine with a specified set of user-defined functions.
        /// </summary>
        public Engine (params IFunctionPack [] functionPacks)
        {
            this.functions = functionPacks
                .SelectMany (p => p.Functions)
                .ToDictionary (kvp => kvp.Key, kvp => kvp.Value);
        }

        public Engine (Dictionary <string, IFuncN> functions)
        {
            this.functions = functions;
        }

        internal string Eval (string expression, Properties properties)
        {
            IExpression exp = Parse (expression);

            return exp == null ? null : properties.Eval (exp);
        }

        internal IExpression Parse(string expression)
        {
            var parts = expression.Split ('(').Select (p => p.Trim()).ToList();

            IExpression exp = ParsePropertyOrLiteral (parts.First ());

            foreach (var part in parts.Skip (1))
            {
                if (! part.EndsWith (")")) return null;

                string funcArgs = part.Substring (0, part.Length - 1);

                int i=0;

                for (; i < funcArgs.Length; ++i)
                {
                    if (char.IsUpper (funcArgs [i]) || funcArgs [i] == '\"') break;
                }

                string func = funcArgs.Substring (0, i).Trim ();

                var args = new List <IExpression> {exp};

                string arg2 = funcArgs.Substring (i).Trim ();

                if (!string.IsNullOrEmpty (arg2))
                {
                    args.Add (ParsePropertyOrLiteral (arg2));
                }

                IFuncN def;
                if (!functions.TryGetValue (func, out def)) return null;

                if (def.ArgCount != args.Count) return null;

                exp = new Function (def, args);
            }

            return exp;
        }

        private static IExpression ParsePropertyOrLiteral (string s)
        {
            return (s [0] == '\"') ? (IExpression) new Literal (s.Substring (1, s.Length-2)) : new Field (s);
        }
    }
}