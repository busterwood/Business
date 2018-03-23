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
        public bool Async { get; set; }
        public bool Transactions { get; set; }

        string VoidMethodReturns => Async ? "Task" : "void";
        string FunctionReturns(string type) => Async ? $"Task<{type}>" : type;
        string MethodModifier => Async ? "async " : "";
        string CallModifier => Async ? "await " : "";
        string TransactionScopeOption => Async ? "TransactionScopeAsyncFlowOption.Enabled" : "";

        //internal void Compile(Model model, string outputPath, IReadOnlyDictionary<string, object> options = null)
        //{
        //    string @namespace = options?.GetValueOrDefault("namespace")?.ToString() ?? Path.GetFileNameWithoutExtension(outputPath);

        //    var codeProvider = new CSharpCodeProvider();

        //    var common = new StringWriter();
        //    CsCommonGenerator.Generate(common, @namespace);

        //    var tables = new StringWriter();
        //    CsTablesGenerator.GenerateTables(model.Tables, tables, @namespace);

        //    var process = new StringWriter();
        //    GenerateProcesses(model.BusinessProcesses, process, @namespace);

        //    var refs = Transactions ? new string[] { "System.Transactions.dll" } : new string[0];
        //    var @params = new CompilerParameters(refs, outputPath);
        //    var results = codeProvider.CompileAssemblyFromSource(@params, 
        //        common.GetStringBuilder().ToString(), 
        //        tables.GetStringBuilder().ToString(), 
        //        process.GetStringBuilder().ToString()
        //    );
        //    var errors = string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select(e => e.ToString()));
        //    if (errors.Length > 0)
        //        throw new Exception(errors);
        //}

        public void Generate(Model model, string outputFolder, IReadOnlyDictionary<string, object> options = null)
        {
            string @namespace = options?.GetValueOrDefault("namespace")?.ToString();

            using (var output = new StreamWriter(Path.Combine(outputFolder, "common.cs")))
                CsCommonGenerator.Generate(output, @namespace);

            using (var output = new StreamWriter(Path.Combine(outputFolder, "tables.cs")))
                CsTablesGenerator.GenerateTables(model.Tables, output, @namespace);

            using (var output = new StreamWriter(Path.Combine(outputFolder, "processes.cs")))
                GenerateProcesses(model.BusinessProcesses, output, @namespace);
        }

        private void GenerateProcesses(UniqueList<BusinessProcess> businessProcesses, TextWriter output, string @namespace)
        {
            output.WriteLine("using System;");
            if (Transactions)
                output.WriteLine("using System.Transactions;");
            if (Async)
                output.WriteLine("using System.Threading.Tasks;");

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
            string className = p.ClrName() + "Process";
            output.WriteLine($"public abstract class {className}: IProcess<{className}.Step, {className}.Context>");
            output.WriteLine("{");
            output.WriteLine("\tprotected Step _step;");
            output.WriteLine("\tprotected Context _context;");
            output.WriteLine();
            output.WriteLine("\tStep IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }");
            output.WriteLine("\tContext IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }");

            var startingStepsEnding = new List<Step> { new Step("Starting", 0), new Step("Ending", 0) };
            startingStepsEnding.InsertRange(1, p.Steps);

            GenerateStepsEnum(startingStepsEnding, output);
            GenerateContextClass(output);

            GenerateGiven(p, output);
            GenerateExecute(startingStepsEnding, output);
            GenerateSteps(startingStepsEnding, output);

            output.WriteLine();
            output.WriteLine($"\tprotected virtual {MethodModifier}{VoidMethodReturns} OnStart(Step step) {{}}");
            output.WriteLine();
            output.WriteLine($"\tprotected virtual {MethodModifier}{VoidMethodReturns} OnEnd(Step step) {{}}");
            output.WriteLine();
            output.WriteLine($"\tprotected virtual {MethodModifier}{VoidMethodReturns} OnFailure(Step step, Exception e) {{}}");


            output.WriteLine("}");
        }

        private void GenerateContextClass(TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("\tpublic partial class Context");
            output.WriteLine("\t{");
            output.WriteLine("\t\tpublic Exception Failure { get; set; }");
            output.WriteLine("\t\tpublic bool Failed { get { return Failure != null; } }");
            output.WriteLine("\t}");
        }

        private void GenerateStepsEnum(IEnumerable<Step> steps, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("\tpublic enum Step");
            output.WriteLine("\t{");
            foreach (var s in steps)
            {
                output.WriteLine($"\t\t{s.ClrName()},");
            }
            output.WriteLine($"\t\tFinished,");
            output.WriteLine("\t}");
        }

        private void GenerateGiven(BusinessProcess p, TextWriter output)
        {
            if (p.Given == null) return;
            var g = p.Given;
            output.WriteLine();
            output.WriteLine($"\t/// <summary>{g}</summary>");
            output.WriteLine($"\tpublic bool Given(I{g.What.ClrName()} item) {{ return {g.ClrName()}(item); }}");
            output.WriteLine();
            output.WriteLine($"\tprotected abstract bool {g.ClrName()}(I{g.What.ClrName()} item);");
        }

        private void GenerateExecute(IEnumerable<Step> steps, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine($"\tpublic {MethodModifier}{VoidMethodReturns} Execute()");
            output.WriteLine("\t{");

            output.WriteLine("\t\t_context.Failure = null;");
            output.WriteLine("\t\ttry");
            output.WriteLine("\t\t{");
            output.WriteLine($"\t\t\t{CallModifier}OnExecute();");
            output.WriteLine($"\t\t\treturn;");
            output.WriteLine("\t\t}");
            output.WriteLine("\t\tcatch (Exception e)");
            output.WriteLine("\t\t{");
            output.WriteLine("\t\t\t_context.Failure = e;");
            output.WriteLine("\t\t}");
            output.WriteLine($"\t\t{CallModifier}OnFailure(_step, _context.Failure);");
            output.WriteLine("\t}");

            output.WriteLine();
            output.WriteLine($"\tprivate {MethodModifier}{VoidMethodReturns} OnExecute()");
            output.WriteLine("\t{");

            foreach (var s in steps)
            {
                output.WriteLine($"\t\t{s.ClrName()}();");
            }
            output.WriteLine("\t}");
        }

        private void GenerateSteps(IEnumerable<Step> steps, TextWriter output)
        {
            var e = new LookAheadEnumerator<Step>(steps.GetEnumerator());
            while (e.MoveNext())
            {
                var s = e.Current;
                string stepName = s.ClrName();
                GenerateStep(output, stepName, e.Next);
                output.WriteLine();
                output.WriteLine($"\tprotected abstract {FunctionReturns("Context")} On{s.ClrName()}();");
            }
        }

        private void GenerateStep(TextWriter output, string stepName, Step next)
        {
            var indent = "\t\t\t";

            output.WriteLine();
            output.WriteLine($"\tprivate {MethodModifier}{VoidMethodReturns} {stepName}()");
            output.WriteLine("\t{");
            output.WriteLine($"\t\tif (_step != Step.{stepName}) return;");
            output.WriteLine("\t\tusing (var tr = new Transition<Step, Context>(this))");
            output.WriteLine("\t\t{");
            if (Transactions)
            {
                output.WriteLine($"{indent}using (var txn = new TransactionScope({TransactionScopeOption}))");
                output.WriteLine($"{indent}{{");
                indent += "\t";
            }
            output.WriteLine($"{indent}{CallModifier}OnStart(Step.{stepName});");
            output.WriteLine($"{indent}_context = {CallModifier}On{stepName}();");
            output.WriteLine($"{indent}_step = Step.{NextStepName(next)};");
            output.WriteLine($"{indent}{CallModifier}OnEnd(Step.{stepName});");
            if (Transactions)
            {
                output.WriteLine($"{indent}txn.Complete();");
                output.WriteLine("\t\t\t}");
            }
            output.WriteLine("\t\t\ttr.Complete();");
            output.WriteLine("\t\t}");
            output.WriteLine("\t}");

        }

        protected string NextStepName(Step step) => step?.ClrName() ?? "Finished";

    }
}
