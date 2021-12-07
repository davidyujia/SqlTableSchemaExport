using System.Collections.Generic;
using System.IO;

namespace SqlTableSchemaExporter.Core
{
    public interface ITableExportService
    {
        /// <summary>
        /// Default file extension name.
        /// </summary>
        /// <returns></returns>
        string DefaultFileExtensionName();

        /// <summary>
        /// Exports the specified database name.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        /// <param name="tables">The tables.</param>
        Stream Export(string dbName, IEnumerable<TableInfoModel> tables);
    }
}
