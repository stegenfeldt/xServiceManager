using System;
using System.Text.RegularExpressions;

namespace xServiceManager.Module
{
    public class PropertyOperatorValue
    {
        public string Property;
        public string Operator;
        public string Value;
        public PropertyOperatorValue() {; }
        public PropertyOperatorValue(string filter)
        {
            RegexOptions ropt = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            Regex r = new Regex("(?<Property>.*)\\s+(?<Operator>-like|-notlike|=|==|<|>|!=|-eq|-ne|-gt|-ge|-le|-lt|-isnull|-isnotnull)\\s+(?<Value>.*)", ropt);
            // OK - we have a filter we can use
            Match m = r.Match(filter);
            if (!m.Success) { throw new InvalidOperationException("Filter '" + filter + "' is invalid"); }
            Property = m.Groups["Property"].Value.Trim();
            Operator = GetOperator(m.Groups["Operator"].Value.Trim().ToLower());
            // this now handles wildcard characters in a simple minded way
            Value = m.Groups["Value"].Value.Trim().Trim('"', '\'').Replace("*", "%").Replace("?", "_");
        }
        public static string GetOperator(string myOperator)
        {
            switch (myOperator.ToLowerInvariant())
            {
                case "-like": return "Like";
                case "-notlike": return "NotLike";
                case "-eq": case "=": case "==": return "Equal";
                case "-ne": case "!=": return "NotEqual";
                case "-gt": case ">": return "Greater";
                case "-ge": case ">=": return "GreaterEqual";
                case "-le": case "<=": return "LessEqual";
                case "-lt": case "<": return "Less";
                case "-isnull": return "Is Null";
                case "-isnotnull": return "Is Not Null";
                default: throw new InvalidOperationException("'" + myOperator + "' is not a valid operator");
            }
        }
    }
}
