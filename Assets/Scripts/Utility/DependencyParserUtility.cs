using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class DependencyParserUtility
{
    private static string ExpressionReplacer(string value, string valueToBeReplaced)
    {
        int start = value.IndexOf("*");
        int end = value.IndexOf("*", start + 1) + 1;
        string result = value.Substring(start, end - start);
        value = value.Replace(result, valueToBeReplaced);
        return value;
    }

    public static string DependencyCheck(string expression, List<string> videoNameList)
    {
        //Check the videos which are dependent based on Logical expression
        if (expression.Contains("<") || expression.Contains(">") || expression.Contains("="))
        {
            if (expression.Contains("AND") || expression.Contains("OR") || expression.Contains("NOT"))
            {
                string result = expression.Split(new string[] { "*" }, 3, StringSplitOptions.None)[1];

                string isExpressionTrue = ComparisonCheck(result);

                string updatedExpression = ExpressionReplacer(expression, isExpressionTrue);

                return DependencyCheck(updatedExpression, videoNameList);
            }
            else
                return ComparisonCheck(expression);
        }
        else
        {
            //Check the videos which are dependent based on boolean expression
            List<string> members = new();

            foreach (string item in videoNameList)
                members.Add(item);

            expression = expression.Replace(" ", "");
            expression = expression.Replace("OR", " || ");
            expression = expression.Replace("AND", " && ");
            expression = expression.Replace("NOT", " ! ");

            Regex RE = new Regex(@"([\(\)\! ])");
            string[] tokens = RE.Split(expression);
            string eqOutput = String.Empty;
            string[] operators = new string[] { "&&", "||", "!", ")", "(" };

            foreach (string tok in tokens)
            {
                if (tok == "1")
                {
                    eqOutput += "1";
                    continue;
                }

                if (tok == "0")
                {
                    eqOutput += "0";
                    continue;
                }


                if (tok.Trim() == String.Empty)
                    continue;
                if (operators.Contains(tok))
                {
                    eqOutput += tok;
                }
                else if (members.Contains(tok))
                {
                    eqOutput += "1";
                }
                else
                {
                    eqOutput += "0";
                }
            }

            while (eqOutput.Length > 1)
            {
                if (eqOutput.Contains("!1"))
                    eqOutput = eqOutput.Replace("!1", "0");
                else if (eqOutput.Contains("!0"))
                    eqOutput = eqOutput.Replace("!0", "1");
                else if (eqOutput.Contains("1&&1"))
                    eqOutput = eqOutput.Replace("1&&1", "1");
                else if (eqOutput.Contains("1&&0"))
                    eqOutput = eqOutput.Replace("1&&0", "0");
                else if (eqOutput.Contains("0&&1"))
                    eqOutput = eqOutput.Replace("0&&1", "0");
                else if (eqOutput.Contains("0&&0"))
                    eqOutput = eqOutput.Replace("0&&0", "0");
                else if (eqOutput.Contains("1||1"))
                    eqOutput = eqOutput.Replace("1||1", "1");
                else if (eqOutput.Contains("1||0"))
                    eqOutput = eqOutput.Replace("1||0", "1");
                else if (eqOutput.Contains("0||1"))
                    eqOutput = eqOutput.Replace("0||1", "1");
                else if (eqOutput.Contains("0||0"))
                    eqOutput = eqOutput.Replace("0||0", "0");
                else if (eqOutput.Contains("(1)"))
                    eqOutput = eqOutput.Replace("(1)", "1");
                else if (eqOutput.Contains("(0)"))
                    eqOutput = eqOutput.Replace("(0)", "0");
            }

            return eqOutput;
        }
    }

    private static string ComparisonCheck(string expression)
    {
        string[] temp = expression.Split(' ');

        int value;
        int valueToCompare = 0;

        if (temp[1].Equals("<"))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare < value) ? "1" : "0";
        }
        else if (temp[1].Equals(">"))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare > value) ? "1" : "0";
        }
        else if (temp[1].Equals("<="))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare <= value) ? "1" : "0";
        }
        else if (temp[1].Equals(">="))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare >= value) ? "1" : "0";
        }
        else if (temp[1].Equals("="))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare == value) ? "1" : "0";
        }

        return "0";
    }

}
