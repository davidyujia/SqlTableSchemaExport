using System.Collections.Generic;

namespace SqlTableSchemaExporter.Core
{
    public interface IDataSourceService
    {
        /// <summary>
        /// Name of database type.
        /// </summary>
        /// <returns></returns>
        string DbTypeName();

        /// <summary>
        /// Get table schema.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns></returns>
        IEnumerable<TableInfoModel> GetTableSchema(string connectionString);

        /// <summary>
        /// Try get database name (if can).
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        bool TryGetDbName(string connectionString,out string dbName);
    }
}
