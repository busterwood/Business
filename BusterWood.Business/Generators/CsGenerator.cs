using BusterWood.Collections;
using System.IO;

namespace BusterWood.Business
{
    public class CsGenerator : IGenerator
    {
        public void Generate(Model model, string outputFolder)
        {
            GenerateTables(model.Tables, outputFolder);
            GenerateProcesses(model.BusinessProcesses, outputFolder);

        }

        private void GenerateTables(UniqueList<Table> tables, string outputFolder)
        {
            using (var output = new StreamWriter(Path.Combine(outputFolder, "tables.cs")))
            {
                output.WriteLine("using System;");
                output.WriteLine("using System.Threading.Tasks;");
                output.WriteLine();
                foreach (var t in tables)
                {
                    GenerateTable(t, output);
                }
            }
        }

        private void GenerateTable(Table t, StreamWriter output)
        {
            output.WriteLine($"public interface I{t.ClrName()}");
            output.WriteLine("{");
            foreach (var f in t.Fields)
            {
                output.WriteLine($"\tstring {f.ClrName()} {{ get; }}");
            }
            output.WriteLine("}");
        }

        private void GenerateProcesses(UniqueList<BusinessProcess> businessProcesses, string outputFolder)
        {
            using (var output = new StreamWriter(Path.Combine(outputFolder, "processes.cs")))
            {
                output.WriteLine("using System;");
                output.WriteLine("using System.Threading.Tasks;");
                output.WriteLine("using BusterWood.Logging;");
                output.WriteLine();
                foreach (var p in businessProcesses)
                {
                    GenerateProcess(p, output);
                }
            }
        }

        private void GenerateProcess(BusinessProcess p, StreamWriter output)
        {
            string className = p.ClrName();
            output.WriteLine($"public class {className}");
            output.WriteLine("{");

            output.WriteLine("\tpublic async Task Execute()");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tLog.Info($\"Before {{nameof({className})}}\");");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\tawait {s.ClrName()}();");
            }
            output.WriteLine($"\t\tLog.Info($\"After {{nameof({className})}}\");");
            output.WriteLine("\t}");
            
            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tprivate async Task {s.ClrName()}()");
                output.WriteLine("\t{");
                output.WriteLine($"\t\tLog.Info($\"Before {{nameof({s.ClrName()})}}\");");
                output.WriteLine($"\t\tawait Before{s.ClrName()}?.Invoke(this);");
                output.WriteLine($"\t\tawait {s.ClrName()}Core();");
                output.WriteLine($"\t\tawait After{s.ClrName()}?.Invoke(this);");
                output.WriteLine($"\t\tLog.Info($\"After {{nameof({s.ClrName()})}}\");");
                output.WriteLine("\t}");
            }

            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tprotected virtual Task {s.ClrName()}Core() => Task.CompletedTask;");
            }

            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tpublic event Func<{className}, Task> Before{s.ClrName()};");
                output.WriteLine();
                output.WriteLine($"\tpublic event Func<{className}, Task> After{s.ClrName()};");
            }
            output.WriteLine("}");
        }

    }


}
