using System.Collections.Generic;

namespace SqlTableSchemaExporter.Core
{
    public class TableInfoModel
    {
        /// <summary>
        /// Table name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Table comment.
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Table columns.
        /// </summary>
        public IList<ColumnInfoModel> Columns { get; set; }
    }

    public class ColumnInfoModel
    {
        /// <summary>
        /// Column name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Is column can null.
        /// </summary>
        public bool IsCanNull { get; set; }
        /// <summary>
        /// Is column primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// Column data type.
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// Column comment.
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Column default value.
        /// </summary>
        public string DefaultValue { get; set; }
    }
}
