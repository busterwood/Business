using BusterWood.Collections;
using System;
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
                foreach (var t in tables)
                {
                    output.WriteLine();
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
                output.WriteLine("using BusterWood.Logging;");
                foreach (var p in businessProcesses)
                {
                    output.WriteLine();
                    GenerateProcess(p, output);
                }
            }
        }

        private void GenerateProcess(BusinessProcess p, StreamWriter output)
        {
            string className = p.ClrName();
            output.WriteLine($"public partial class {className}");
            output.WriteLine("{");
            output.WriteLine("\tStep _step;");

            output.WriteLine("\tpublic void Execute()");
            output.WriteLine("\t{");

            output.WriteLine("\t\tfor(;;)");
            output.WriteLine("\t\t{");
            output.WriteLine("\t\t\tswitch (_step)");
            output.WriteLine("\t\t\t{");
            output.WriteLine("\t\t\t\tcase Step._Start:");
            output.WriteLine("\t\t\t\t\tBeforeExecute();");
            output.WriteLine("\t\t\t\t\tbreak;");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\t\t\tcase Step.{s.ClrName()}:");
                output.WriteLine($"\t\t\t\t\t{s.ClrName()}();");
                output.WriteLine("\t\t\t\t\tbreak;");
            }
            output.WriteLine("\t\t\t\tcase Step._End:");
            output.WriteLine("\t\t\t\t\tAfterExecute();");
            output.WriteLine("\t\t\t\t\tbreak;");
            output.WriteLine("\t\t\t\tdefault:");
            output.WriteLine("\t\t\t\t\tthrow new InvalidOperationException(_step.ToString());");
            output.WriteLine("\t\t\t}");
            output.WriteLine("\t\t}");
            output.WriteLine("\t}");

            var e = new LookAheadEnumerator<Step>(p.Steps.GetEnumerator());
            while (e.MoveNext())
            {
                var s = e.Current;
                output.WriteLine();
                output.WriteLine($"\tprivate void {s.ClrName()}()");
                output.WriteLine("\t{");
                output.WriteLine($"\t\tStarting(Step.{s.ClrName()});");
                output.WriteLine($"\t\tBefore{s.ClrName()}();");
                output.WriteLine($"\t\t{s.ClrName()}Core();");
                output.WriteLine($"\t\tAfter{s.ClrName()}();");
                output.WriteLine($"\t\tFinished(Step.{s.ClrName()});");
                var next = e.Next?.ClrName() ?? "_End";
                output.WriteLine($"\t\t_step = Step.{next};");
                output.WriteLine("\t}");
            }

            output.WriteLine();
            output.WriteLine("\tpartial void BeforeExecute();");
            output.WriteLine();
            output.WriteLine("\tpartial void AfterExecute();");

            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tpartial void {s.ClrName()}Core();");
            }

            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tpartial void Before{s.ClrName()}();");
                output.WriteLine();
                output.WriteLine($"\tpartial void After{s.ClrName()}();");
            }

            output.WriteLine();
            output.WriteLine("\tpartial void Starting(Step step);");
            output.WriteLine();
            output.WriteLine("\tpartial void Finished(Step step);");

            output.WriteLine();
            output.WriteLine("\tenum Step");
            output.WriteLine("\t{");
            output.WriteLine($"\t\t_Start,");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\t{s.ClrName()},");
            }
            output.WriteLine($"\t\t_End,");
            output.WriteLine("\t}");

            output.WriteLine("}");
        }

    }


}
