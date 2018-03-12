using BusterWood.Collections;
using System.IO;

namespace BusterWood.Business
{
    public class Model
    {
        public UniqueList<Table> Tables { get; }
        public UniqueList<BusinessProcess> BusinessProcesses { get; }

        public Model(UniqueList<Table> tables, UniqueList<BusinessProcess> businessProcesses)
        {
            Tables = tables;
            BusinessProcesses = businessProcesses;
        }

        public static Model Parse(TextReader reader)
        {
            var lex = new LineLexer(reader);
            var e = new LookAheadEnumerator<Line>(lex.GetEnumerator());
            var dmr = new DataModelReader(e);
            var pmr = new ProcessModelReader(e);
            return new Model(dmr.ToUniqueList(), pmr.ToUniqueList());
        }
    }
}
