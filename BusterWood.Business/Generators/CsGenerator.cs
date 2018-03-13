using BusterWood.Collections;
using System;
using System.IO;
using System.Linq;

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
            output.WriteLine($"public abstract class {className}");
            output.WriteLine("{");
            output.WriteLine("\tStep _step;");
            output.WriteLine();

            GenerateGiven(p, output);
            GenerateExecute(p, output);
            GenerateStarting(p, output);
            GenerateSteps(p, output);
            GenerateFinished(p, output);

            output.WriteLine();
            output.WriteLine("\tprotected virtual void SetNextStep(Step s) => _step = s;");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void StartingCore() {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void FinishedCore() {}");

            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tprotected abstract void {s.ClrName()}Core();");
            }

            foreach (var s in p.Steps)
            {
                output.WriteLine();
                output.WriteLine($"\tprotected virtual void Before{s.ClrName()}() {{}}");
                output.WriteLine();
                output.WriteLine($"\tprotected virtual void After{s.ClrName()}() {{}}");
            }

            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnStart(Step step) {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnEnd(Step step) {}");

            output.WriteLine();
            output.WriteLine("\tprotected enum Step");
            output.WriteLine("\t{");
            output.WriteLine($"\t\t_Start,");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\t{s.ClrName()},");
            }
            output.WriteLine($"\t\t_Finish,");
            output.WriteLine("\t}");

            output.WriteLine("}");
        }

        private static void GenerateGiven(BusinessProcess p, StreamWriter output)
        {
            if (p.Given == null) return;
            var g = p.Given;
            output.WriteLine($"\t/// <summary>{g}</summary>");
            output.WriteLine($"\tpublic abstract bool Given(I{g.What.ClrName()} item);");
            output.WriteLine();
        }

        private static void GenerateExecute(BusinessProcess p, StreamWriter output)
        {
            output.WriteLine("\tpublic void Execute()");
            output.WriteLine("\t{");

            output.WriteLine("\t\tStarting();");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\t{s.ClrName()}();");
            }
            output.WriteLine("\t\tFinished();");
            output.WriteLine("\t}");
        }

        private static void GenerateSteps(BusinessProcess p, StreamWriter output)
        {
            var e = new LookAheadEnumerator<Step>(p.Steps.GetEnumerator());
            while (e.MoveNext())
            {
                var s = e.Current;
                string stepName = s.ClrName();
                output.WriteLine();
                output.WriteLine($"\tprivate void {stepName}()");
                output.WriteLine("\t{");
                output.WriteLine($"\t\tif (_step != Step.{stepName}) return;");
                output.WriteLine($"\t\tOnStart(Step.{stepName});");
                output.WriteLine($"\t\tBefore{stepName}();");
                output.WriteLine($"\t\t{stepName}Core();");
                output.WriteLine($"\t\tAfter{stepName}();");
                output.WriteLine($"\t\tOnEnd(Step.{stepName});");
                var next = e.Next?.ClrName() ?? "_Finish";
                output.WriteLine($"\t\tSetNextStep(Step.{next});");
                output.WriteLine("\t}");
            }
        }

        private static void GenerateStarting(BusinessProcess p, StreamWriter output)
        {
            output.WriteLine();
            output.WriteLine($"\tprivate void Starting()");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tif (_step != Step._Start) return;");
            output.WriteLine($"\t\tStartingCore();");
            var next1 = p.Steps.FirstOrDefault()?.ClrName() ?? "_End";
            output.WriteLine($"\t\tSetNextStep(Step.{next1});");
            output.WriteLine("\t}");
        }

        private static void GenerateFinished(BusinessProcess p, StreamWriter output)
        {
            output.WriteLine();
            output.WriteLine($"\tprivate void Finished()");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tif (_step != Step._Finish) return;");
            output.WriteLine($"\t\tFinishedCore();");
            output.WriteLine("\t}");
        }
    }


}
