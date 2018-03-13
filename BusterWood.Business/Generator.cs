using System.Collections.Generic;

namespace BusterWood.Business
{
    public interface IGenerator
    {
        void Generate(Model model, string outputFolder, IReadOnlyDictionary<string, object> options);
    }
}
