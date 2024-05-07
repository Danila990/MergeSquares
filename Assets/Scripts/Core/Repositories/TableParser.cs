using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LargeNumbers;
using ModestTree;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Core.Repositories
{
    public static class TableParser
    {
        public static T ParseTable<T>(TableParseContext<T> context, string column = "Id")
        {
            context.init(context);
            foreach (var cell in context.ss.columns[column])
            {
                if (cell.value == column)
                    continue;
                
                if (cell.value.StartsWith("//"))
                    continue;
                
                if (cell.value.Trim() == string.Empty)
                    continue;

                if (cell.value == "--")
                    break;

                context.cell = cell;
                context.row = context.ss.rows[cell.value];
                context.parseRow(context);
            }
            Debug.Log($"[TableParser][ParseTable] Finished for: {context.name}");
            return context.value;
        }

        public static FloatRange ParseRange(string data)
        {
            var minMax = data.Split('/');

            return new FloatRange(Convert.ToSingle(minMax[0]), Convert.ToSingle(minMax[1]));
        }

        public static List<string> ParseStringList(string data, char sep = '/')
        {
            var result = new List<string>();
            foreach (var element in data.Split(sep))
            {
                if (!element.Trim().IsEmpty())
                {
                    result.Add(element.Trim());
                }
            }

            return result;
        }
        
        public static List<int> ParseIntList(string data, char sep = '/')
        {
            var result = new List<int>();
            foreach (var element in data.Split(sep))
            {
                if (!element.Trim().IsEmpty())
                {
                    result.Add(int.Parse(element.Trim()));
                }
            }

            return result;
        }

        private static readonly Regex Whitespace = new Regex(@"\s+");

        public static string RemoveWhitespaces(string input)
        {
            return Whitespace.Replace(input, string.Empty);
        }

        public static AlphabeticNotation ExponentialParseToAlphabeticNotation(string data)
        {
            var values = data.Split(new[] {"E+"}, StringSplitOptions.RemoveEmptyEntries);
            var test0 = Convert.ToSingle(values[0]);
            var test = Convert.ToInt32(values[1]);
            ScientificNotation notation =
                new ScientificNotation(Convert.ToSingle(values[0]), Convert.ToInt32(values[1]));
            return (AlphabeticNotation) notation;
        }

        public static AlphabeticNotation ParseAlphabeticNotation(string data)
        {
            var values = data.Split('/');

            return new AlphabeticNotation(Convert.ToDouble(values[0]), Convert.ToInt32(values[1]));
        }
    }
}