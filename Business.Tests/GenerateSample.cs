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
                var mod = Model.Parse(f);
                var gen = new CsGenerator();
                gen.Generate(mod, Environment.CurrentDirectory);
                gen.Compile(mod, Path.Combine(Environment.CurrentDirectory, "sample.dll"));
            }
        }
    }
}
