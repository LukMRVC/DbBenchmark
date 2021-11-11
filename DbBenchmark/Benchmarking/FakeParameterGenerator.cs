using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DbBenchmark.Benchmarking
{
    public class FakeParameterGenerator
    {
        
        public IOrderedDictionary GenerateParams(string[] paramsToGenerate)
        {
            var rnd = new Random();
            var parameters = new OrderedDictionary();
            var dtoNamespace = @"DbBenchmark.ORM.DTO";
            foreach (var paramType in paramsToGenerate)
            {
                var complex = Type.GetType(dtoNamespace + paramType);
                if (complex != null)
                {
                    // TODO: Generate new class instance
                }

                if (@"int".Equals(paramType, StringComparison.OrdinalIgnoreCase))
                {
                    parameters.Add(paramType, rnd.Next(Int32.MaxValue));
                }

                if (@"string".Equals(paramType, StringComparison.OrdinalIgnoreCase))
                {
                    parameters.Add(paramType, GeneratedString(rnd));
                }
            }

            return parameters;
        }

        public string GeneratedString(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, random.Next(32))
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}