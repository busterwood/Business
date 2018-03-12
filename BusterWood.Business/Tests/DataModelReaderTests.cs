using BusterWood.Testing;
using System.Linq;

namespace BusterWood.Business
{
    public class DataModelReaderTests
    {
        public static void try_to_read_empty_file_throws_parse_exception(Test t)
        {
            var reader = new DataModelReader(new Line[0]);
            t.AssertThrows<ParseException>(() => reader.ToList());
        }

        public static void can_read_zero_tables(Test t)
        {
            var reader = new DataModelReader(new Line[] {
                new Identifier(DataModelReader.ModelName, 1),
            });
            var tables = reader.ToList();
            t.Assert(() => tables.Count == 0);
        }

        public static void can_read_one_table_without_any_fields(Test t)
        {
            var reader = new DataModelReader(new Line[] {
                new Identifier(DataModelReader.ModelName, 1),
                new Identifier("order", 1)
            });
            var tables = reader.ToList();
            t.Assert(() => tables.Count == 1);
            t.Assert(() => tables[0].Is("order"));
            t.Assert(() => tables[0].Fields.Count == 0);
        }

        public static void cannot_read_field_without_table(Test t)
        {
            var reader = new DataModelReader(new Line[] {
                new Identifier(DataModelReader.ModelName, 1),
                new Line("order", 1)
            });
            t.AssertThrows<ParseException>(() => reader.ToList());
        }

        public static void can_read_one_table_with_one_field(Test t)
        {
            var reader = new DataModelReader(new Line[] {
                new Identifier(DataModelReader.ModelName, 1),
                new Identifier("order", 1),
                new Line("user", 1),
            });
            var tables = reader.ToList();
            t.Assert(() => tables.Count == 1);
            Table table = tables[0];
            t.Assert(() => table.Is("order"));
            t.Assert(() => table.Fields.Count == 1);
            t.Assert(() => table.Fields[0].Is("user"));
        }

        public static void can_read_multiple_tables_with_fields(Test t)
        {
            var reader = new DataModelReader(new Line[] {
                new Identifier(DataModelReader.ModelName, 1),
                new Identifier("order", 1),
                new Line("user", 2),
                new Line("basket", 3),
                new Identifier("fill", 4),
                new Line("price", 5),
                new Line("quantity", 6),
            });
            var tables = reader.ToList();
            t.Assert(() => tables.Count == 2);

            Table order = tables[0];
            t.Assert(() => order.Is("order"));
            t.Assert(() => order.Fields.Count == 2);
            t.Assert(() => order.Fields[0].Is("user"));
            t.Assert(() => order.Fields[1].Is("basket"));

            Table fill = tables[1];
            t.Assert(() => fill.Is("fill"));
            t.Assert(() => fill.Fields.Count == 2);
            t.Assert(() => fill.Fields[0].Is("price"));
            t.Assert(() => fill.Fields[1].Is("quantity"));
        }

        public static void process_model_terminates_the_data_model(Test t)
        {
            var reader = new DataModelReader(new Line[] {
                new Identifier(DataModelReader.ModelName, 1),
                new Identifier("order", 1),
                new Line("user", 2),
                new Line("basket", 3),
                new Identifier(ProcessModelReader.ModelName, 4), // terminates the data model
                new Line("price", 5),
                new Line("quantity", 6),
            });
            var tables = reader.ToList();
            t.Assert(() => tables.Count == 1);

            Table order = tables[0];
            t.Assert(() => order.Is("order"));
            t.Assert(() => order.Fields.Count == 2);
            t.Assert(() => order.Fields[0].Is("user"));
            t.Assert(() => order.Fields[1].Is("basket"));
        }

    }
}
