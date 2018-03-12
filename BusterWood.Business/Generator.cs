using BusterWood.Collections;
using BusterWood.Goodies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    }

    public interface IGenerator
    {
        void Generate(Model model, string outputFolder);
    }


}
