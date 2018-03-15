using BusterWood.Collections;
using System;
using System.IO;

namespace BusterWood.Business
{
    static class CsTablesGenerator
    {
        public static void GenerateTables(UniqueList<Table> tables, TextWriter output, string @namespace)
        {
            StartNamespace(output, @namespace);
            output.WriteLine("using System.Collections.Generic;");
            foreach (var t in tables)
            {
                output.WriteLine();
                GenerateTable(t, output);
            }
            EndNamespace(output, @namespace);
        }

        public static void EndNamespace(TextWriter output, string @namespace)
        {
            if (!string.IsNullOrWhiteSpace(@namespace))
                output.WriteLine("}");
        }

        public static void StartNamespace(TextWriter output, string @namespace)
        {
            if (!string.IsNullOrWhiteSpace(@namespace))
            {
                output.WriteLine("namespace " + @namespace);
                output.WriteLine("{");
            }
        }

        public static void GenerateTable(Table t, TextWriter output)
        {
            output.WriteLine($"public interface I{t.ClrName()}");
            output.WriteLine("{");
            foreach (var f in t.Fields)
            {
                string type = string.Equals(f.Type, "string", StringComparison.OrdinalIgnoreCase) ? f.Type : "I" + f.Type.ClrName();
                if (f is Relationship r && (r.Many & Multiplicity.Many) == Multiplicity.Many)
                {
                    output.WriteLine($"\tIEnumerable<I{r.SingularWhat().ClrName()}> {r.What.ClrName()} {{ get; }}");

                }
                else
                {
                    output.WriteLine($"\t{type} {f.Name.ClrName()} {{ get; }}");
                }
            }
            output.WriteLine("}");
        }
    }
}
