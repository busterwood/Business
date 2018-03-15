using BusterWood.Collections;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;

namespace BusterWood.Business
{
    public class CsGenerator : IGenerator
    {
        internal void Compile(Model model, string outputPath, IReadOnlyDictionary<string, object> options = null)
        {
            string @namespace = options?.GetValueOrDefault("namespace")?.ToString() ?? Path.GetFileNameWithoutExtension(outputPath);

            var codeProvider = new CSharpCodeProvider();
            var tables = new StringWriter();
            GenerateTables(model.Tables, tables, @namespace);

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
                GenerateTables(model.Tables, output, @namespace);

            using (var output = new StreamWriter(Path.Combine(outputFolder, "processes.cs")))
                GenerateProcesses(model.BusinessProcesses, output, @namespace);
        }

        private void GenerateTables(UniqueList<Table> tables, TextWriter output, string @namespace)
        {
            StartNamespace(output, @namespace);
            foreach (var t in tables)
            {
                output.WriteLine();
                GenerateTable(t, output);
            }
            EndNamespace(output, @namespace);
        }

        private static void EndNamespace(TextWriter output, string @namespace)
        {
            if (!string.IsNullOrWhiteSpace(@namespace))
                output.WriteLine("}");
        }

        private static void StartNamespace(TextWriter output, string @namespace)
        {
            if (!string.IsNullOrWhiteSpace(@namespace))
            {
                output.WriteLine("namespace " + @namespace);
                output.WriteLine("{");
            }
        }

        private void GenerateTable(Table t, TextWriter output)
        {
            output.WriteLine($"public interface I{t.ClrName()}");
            output.WriteLine("{");
            foreach (var f in t.Fields)
            {
                string type = string.Equals(f.Type, "string", StringComparison.OrdinalIgnoreCase) ? f.Type : "I" + f.Type.ClrName();
                output.WriteLine($"\t{type} {f.Name.ClrName()} {{ get; }}");
            }
            output.WriteLine("}");
        }

        private void GenerateProcesses(UniqueList<BusinessProcess> businessProcesses, TextWriter output, string @namespace)
        {
            output.WriteLine("using System;");
            StartNamespace(output, @namespace);
            foreach (var p in businessProcesses)
            {
                output.WriteLine();
                GenerateProcess(p, output);
            }
            EndNamespace(output, @namespace);
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
            GenerateFinished(p, output);

            output.WriteLine();
            output.WriteLine("\tprotected virtual void SetNextStep(Step s) { _step = s; }");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void StartingCore() {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void FinishedCore() {}");

            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnStart(Step step) {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnEnd(Step step) {}");
            output.WriteLine();
            output.WriteLine("\tprotected virtual void OnFailure(Step step, Exception e) {}");

            output.WriteLine();
            output.WriteLine("\tpublic enum Step");
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

        private static void GenerateGiven(BusinessProcess p, TextWriter output)
        {
            if (p.Given == null) return;
            var g = p.Given;
            output.WriteLine();
            output.WriteLine($"\t/// <summary>{g}</summary>");
            output.WriteLine($"\tpublic bool Given(I{g.What.ClrName()} item) {{ return {g.ClrName()}(item); }}");
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
                output.WriteLine($"\t\tBefore{stepName}();");
                output.WriteLine($"\t\tOn{stepName}();");
                output.WriteLine($"\t\tAfter{stepName}();");
                var next = e.Next?.ClrName() ?? "_Finish";
                output.WriteLine($"\t\tSetNextStep(Step.{next});");
                output.WriteLine($"\t\tOnEnd(Step.{stepName});");
                output.WriteLine("\t}");

                output.WriteLine();
                output.WriteLine($"\tprotected virtual void Before{s.ClrName()}() {{}}");
                output.WriteLine();
                output.WriteLine($"\tprotected abstract void On{s.ClrName()}();");
                output.WriteLine();
                output.WriteLine($"\tprotected virtual void After{s.ClrName()}() {{}}");
            }
        }

        private static void GenerateStarting(BusinessProcess p, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("\tprivate void Starting()");
            output.WriteLine("\t{");
            output.WriteLine("\t\tif (_step != Step._Start) return;");
            output.WriteLine("\t\tOnStart(Step._Start);");
            output.WriteLine("\t\tStartingCore();");
            var next1 = p.Steps.FirstOrDefault()?.ClrName() ?? "_End";
            output.WriteLine($"\t\tSetNextStep(Step.{next1});");
            output.WriteLine("\t\tOnEnd(Step._Start);");
            output.WriteLine("\t}");
        }

        private static void GenerateFinished(BusinessProcess p, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine($"\tprivate void Finished()");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tif (_step != Step._Finish) return;");
            output.WriteLine("\t\tOnStart(Step._Finish);");
            output.WriteLine($"\t\tFinishedCore();");
            output.WriteLine("\t\tOnEnd(Step._Finish);");
            output.WriteLine("\t}");
        }
    }


}
