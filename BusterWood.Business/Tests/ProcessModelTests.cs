using BusterWood.Testing;
using System.Linq;

namespace BusterWood.Business
{
    public class ProcessModelTests
    {
        public static void try_to_read_empty_file_throws_parse_exception(Test t)
        {
            var reader = new ProcessModelReader(new Line[0]);
            t.AssertThrows<ParseException>(() => reader.ToList());
        }

        public static void can_read_zero_processes(Test t)
        {
            var reader = new ProcessModelReader(new Line[] {
                new Identifier(ProcessModelReader.ModelName, 1),
            });
            var procs = reader.ToList();
            t.Assert(() => procs.Count == 0);
        }

        public static void can_read_one_process_without_any_steps(Test t)
        {
            var reader = new ProcessModelReader(new Line[] {
                new Identifier(ProcessModelReader.ModelName, 1),
                new Identifier("place trade", 1)
            });
            var procs = reader.ToList();
            t.Assert(() => procs.Count == 1);
            t.Assert(() => procs[0].Is("place trade"));
            t.Assert(() => procs[0].Steps.Count == 0);
        }

        public static void cannot_read_step_without_process(Test t)
        {
            var reader = new ProcessModelReader(new Line[] {
                new Identifier(ProcessModelReader.ModelName, 1),
                new Line("place trade", 1)
            });
            t.AssertThrows<ParseException>(() => reader.ToList());
        }

        public static void can_read_one_process_with_one_step(Test t)
        {
            var reader = new ProcessModelReader(new Line[] {
                new Identifier(ProcessModelReader.ModelName, 1),
                new Identifier("place trade", 1),
                new Line("step 1", 1),
            });
            var procs = reader.ToList();
            t.Assert(() => procs.Count == 1);
            BusinessProcess process = procs[0];
            t.Assert(() => process.Is("place trade"));
            t.Assert(() => process.Steps.Count == 1);
            t.Assert(() => process.Steps[0].Is("step 1"));
        }

        public static void can_read_multiple_processs_with_steps(Test t)
        {
            var reader = new ProcessModelReader(new Line[] {
                new Identifier(ProcessModelReader.ModelName, 1),
                new Identifier("place trade", 1),
                new Line("step 1", 2),
                new Line("step 2", 3),
                new Identifier("book broker order", 4),
                new Line("step a", 5),
                new Line("step b", 6),
            });
            var procs = reader.ToList();
            t.Assert(() => procs.Count == 2);

            BusinessProcess book = procs[0];
            t.Assert(() => book.Is("place trade"));
            t.Assert(() => book.Steps.Count == 2);
            t.Assert(() => book.Steps[0].Is("step 1"));
            t.Assert(() => book.Steps[1].Is("step 2"));

            BusinessProcess fill = procs[1];
            t.Assert(() => fill.Is("book broker order"));
            t.Assert(() => fill.Steps.Count == 2);
            t.Assert(() => fill.Steps[0].Is("step a"));
            t.Assert(() => fill.Steps[1].Is("step b"));
        }
    }
}
