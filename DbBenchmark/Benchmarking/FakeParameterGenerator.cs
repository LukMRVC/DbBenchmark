using System;
using System.Collections.Generic;
using System.Linq;

namespace DbBenchmark.Benchmarking
{
    public class FakeParameterGenerator
    {
        private Random rnd;
        
        public FakeParameterGenerator()
        {
            rnd = new Random();
        }

        public object[] GenerateParams(string[] paramsToGenerate)
        {
            return GenerateParams(paramsToGenerate, out _);
        }
        
        public object[] GenerateParams(string[] paramsToGenerate, out int[] indexes, int depth = 0)
        {
            var changedIndexes = new List<int>();
            var parameters = new List<object>();
            var dtoNamespace = @"DbBenchmark.ORM.DTO.";
            for (int i = 0; i < paramsToGenerate.Length; ++i)
            {
                var paramType = paramsToGenerate[i];
                var complex = Type.GetType(dtoNamespace + paramType);
                if (complex != null && depth < 1)
                {
                    changedIndexes.Add(i);
                    parameters.Add(GenerateInstanceOf(complex, depth));
                }
                else
                {
                    var val = GenerateSimpleRandomOf(paramType);
                    if (val != null)
                    {
                        parameters.Add(val);
                        changedIndexes.Add(i);
                    }
                }
            }

            indexes = changedIndexes.ToArray();
            return parameters.ToArray();
        }

        public string GeneratedString(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, random.Next(32))
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public object GenerateInstanceOf(Type complex, int depth)
        {
            var fields = complex.GetProperties();
            var changed = new int[] {};
            var result = GenerateParams(fields.Select(f => f.PropertyType.Name).ToArray(), out changed, depth + 1);
            var instance = Activator.CreateInstance(complex);
            int skipped = 0;
            for (int i = 0; i < fields.Length; ++i)
            {
                skipped++;
                if (changed.Contains(i))
                {
                    skipped--;
                    var val = result[i - skipped];
                    if (fields[i].CanWrite)
                        fields[i].SetValue(instance, val);
                }
            }

            return instance;
        }

        public object? GenerateSimpleRandomOf(string paramTypeName)
        {
            if (@"int".Equals(paramTypeName, StringComparison.OrdinalIgnoreCase)
                || @"int32".Equals(paramTypeName, StringComparison.OrdinalIgnoreCase))
            {
                return rnd.Next(UInt16.MaxValue);
            }

            if (@"string".Equals(paramTypeName, StringComparison.OrdinalIgnoreCase))
            {
                if (paramTypeName.EndsWith("Num", StringComparison.OrdinalIgnoreCase))
                {
                    return GenerateParsableNumber(rnd);
                }
                return GeneratedString(rnd);
            }

            if (@"datetime".Equals(paramTypeName, StringComparison.OrdinalIgnoreCase))
            {
                var start = new DateTime(2020, 1, 1);
                int range = (DateTime.Today - start).Days;
                return start.AddDays(rnd.Next(range));
            }
                
            if (@"boolean".Equals(paramTypeName, StringComparison.OrdinalIgnoreCase))
            {
                if (rnd.Next(100) < 50)
                {
                    return true;
                }
                return false;
            }

            return null;
        }

        private object GenerateParsableNumber(Random random)
        {
            var prefix = random.Next(500).ToString().PadLeft(3, '0');
            var num = random.Next(500_000_000, 599_999_999);
            return $"+{prefix}{num}";
        }
    }
}