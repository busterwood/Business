using BusterWood.Collections;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;

namespace BusterWood.Business
{
    public class CsStateMachineGenerator : IGenerator
    {
        internal void Compile(Model model, string outputPath, IReadOnlyDictionary<string, object> options = null)
        {
            string @namespace = options?.GetValueOrDefault("namespace")?.ToString() ?? Path.GetFileNameWithoutExtension(outputPath);

            var codeProvider = new CSharpCodeProvider();
            var tables = new StringWriter();
            CsTablesGenerator.GenerateTables(model.Tables, tables, @namespace);

            var process = new StringWriter();
            GenerateProcesses(model.BusinessProcesses, process, @namespace);

            var @params = new CompilerParameters(new string[0], outputPath);
            var results = codeProvider.CompileAssemblyFromSource(@params, tables.GetStringBuilder().ToString(), process.GetStringBuilder().ToString());
            var errors = string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select(e => e.ToString()));
            if (errors.Length > 0)
                throw new Exception(errors);
        }

        public void Generate(Model model, string outputFolder, IReadOnlyDictionary<string, object> options = null)
        {
            string @namespace = options?.GetValueOrDefault("namespace")?.ToString();

            using (var output = new StreamWriter(Path.Combine(outputFolder, "tables.cs")))
                CsTablesGenerator.GenerateTables(model.Tables, output, @namespace);

            using (var output = new StreamWriter(Path.Combine(outputFolder, "processes.cs")))
                GenerateProcesses(model.BusinessProcesses, output, @namespace);
        }

        private void GenerateProcesses(UniqueList<BusinessProcess> businessProcesses, TextWriter output, string @namespace)
        {
            output.WriteLine("using System;");
            CsTablesGenerator.StartNamespace(output, @namespace);
            foreach (var p in businessProcesses)
            {
                output.WriteLine();
                GenerateProcess(p, output);
            }
            CsTablesGenerator.EndNamespace(output, @namespace);
        }

        private void GenerateProcess(BusinessProcess p, TextWriter output)
        {
            string className = p.ClrName();
            output.WriteLine($"public abstract class {className}Process");
            output.WriteLine("{");
            output.WriteLine("\tStep _step;");

            output.WriteLine();    
            output.WriteLine("\tpublic Exception Failure { get; private set; }");

            output.WriteLine();
            output.WriteLine("\tpublic bool Failed { get { return Failure != null; } }");

            GenerateGiven(p, output);
            GenerateExecute(p, output);
            GenerateStarting(p, output);
            GenerateSteps(p, output);
            GenerateFinished(output);

            output.WriteLine();
            output.WriteLine("\tprotected virtual void SetNextStep(Step s) { _step = s; }");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnStarting() {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnFinished() {}");

            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnStart(Step step) {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnEnd(Step step) {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnFailure(Step step, Exception e) {}");

            output.WriteLine();
            output.WriteLine("\tpublic enum Step");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tStarting,");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\t{s.ClrName()},");
            }
            output.WriteLine($"\t\tFinished,");
            output.WriteLine("\t}");

            output.WriteLine("}");
        }

        private static void GenerateGiven(BusinessProcess p, TextWriter output)
        {
            if (p.Given == null) return;
            var g = p.Given;
            output.WriteLine();
            output.WriteLine($"\t/// <summary>{g}</summary>");
            output.WriteLine($"\tpublic bool Given(I{g.What.ClrName()} item) {{ return {g.ClrName()}(item); }}");
            output.WriteLine();
            output.WriteLine($"\tprotected abstract bool {g.ClrName()}(I{g.What.ClrName()} item);");
        }

        private static void GenerateExecute(BusinessProcess p, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("\tpublic void Execute()");
            output.WriteLine("\t{");

            output.WriteLine("\t\tFailure = null;");
            output.WriteLine("\t\ttry");
            output.WriteLine("\t\t{");
            output.WriteLine("\t\t\tOnExecute();");
            output.WriteLine("\t\t}");
            output.WriteLine("\t\tcatch (Exception e)");
            output.WriteLine("\t\t{");
            output.WriteLine("\t\t\tFailure = e;");
            output.WriteLine("\t\t\tOnFailure(_step, e);");
            output.WriteLine("\t\t}");
            output.WriteLine("\t}");

            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnExecute()");
            output.WriteLine("\t{");

            output.WriteLine("\t\tStarting();");
            foreach (var s in p.Steps)
            {
                output.WriteLine($"\t\t{s.ClrName()}();");
            }
            output.WriteLine("\t\tFinished();");
            output.WriteLine("\t}");
        }

        private static void GenerateSteps(BusinessProcess p, TextWriter output)
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
                output.WriteLine($"\t\tOn{stepName}();");
                var next = e.Next?.ClrName() ?? "Finished";
                output.WriteLine($"\t\tSetNextStep(Step.{next});");
                output.WriteLine($"\t\tOnEnd(Step.{stepName});");
                output.WriteLine("\t}");

                output.WriteLine();
                output.WriteLine($"\tprotected abstract void On{s.ClrName()}();");
            }
        }

        private static void GenerateStarting(BusinessProcess p, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("\tprivate void Starting()");
            output.WriteLine("\t{");
            output.WriteLine("\t\tif (_step != Step.Starting) return;");
            output.WriteLine("\t\tOnStart(Step.Starting);");
            var next1 = p.Steps.FirstOrDefault()?.ClrName() ?? "_End";
            output.WriteLine($"\t\tSetNextStep(Step.{next1});");
            output.WriteLine("\t\tOnEnd(Step.Starting);");
            output.WriteLine("\t}");
        }

        private static void GenerateFinished(TextWriter output)
        {
            output.WriteLine();
            output.WriteLine($"\tprivate void Finished()");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tif (_step != Step.Finished) return;");
            output.WriteLine("\t\tOnStart(Step.Finished);");
            output.WriteLine($"\t\tOnFinished();");
            output.WriteLine("\t\tOnEnd(Step.Finished);");
            output.WriteLine("\t}");
        }
    }
}
