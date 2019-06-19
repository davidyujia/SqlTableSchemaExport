using System.Collections.Generic;

namespace DbSchemaExporter.Core
{
    public class DbInfoModel
    {
        public string Name { get; set; }
        public IEnumerable<TableInfoModel> Tables { get; set; }
    }
    public class TableInfoModel
    {
        public string Name { get; set; }
        public string Comment { get; set; }
    }

    public class TableInfoWithColumnsModel : TableInfoModel
    {
        public TableInfoWithColumnsModel(TableInfoModel model) : this(model, new List<ColumnInfoModel>())
        {
        }

        public TableInfoWithColumnsModel(TableInfoModel model, IEnumerable<ColumnInfoModel> models)
        {
            Name = model.Name;
            Comment = model.Comment;
            Columns = models;
        }

        public IEnumerable<ColumnInfoModel> Columns { get; set; }
    }

    public class ColumnInfoModel
    {
        public string Name { get; set; }
        public bool IsCanNull { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string Type { get; set; }
        public string Comment { get; set; }
        public string DefaultValue { get; set; }
    }
}
