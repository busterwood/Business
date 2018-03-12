using BusterWood.Collections;
using BusterWood.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Business
{
    public class GenerateSample
    {
        public static void generate_sample(Test t)
        {
            using (var f = new StreamReader(@"..\..\..\sample.txt"))
            {
                var lex = new LineLexer(f);
                var e = new LookAheadEnumerator<Line>(lex.GetEnumerator());
                var dmr = new DataModelReader(e);
                var pmr = new ProcessModelReader(e);
                var mod = new Model(dmr.ToUniqueList(), pmr.ToUniqueList());
                var gen = new CsGenerator();
                gen.Generate(mod, Environment.CurrentDirectory);
            }
        }
    }
}
