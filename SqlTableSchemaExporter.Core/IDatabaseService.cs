using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SqlTableSchemaExporter.Core
{
    public interface IDatabaseService
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
    }

    public abstract class DatabaseServiceBase : IDatabaseService
    {
        /// <summary>
        /// Name of database type.
        /// </summary>
        /// <returns></returns>
        public abstract string DbTypeName();

        /// <summary>
        /// Get table schema.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns></returns>
        public IEnumerable<TableInfoModel> GetTableSchema(string connectionString)
        {
            var conn = GetDbConnection(connectionString);

            var command = GetDbCommand(conn);

            var adapter = GetDataAdapter(command);

            var dataSet = new DataSet();

            adapter.Fill(dataSet);

            return Parse(dataSet);
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        protected abstract IDbConnection GetDbConnection(string connectionString);

        /// <summary>
        /// Gets the database command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        protected abstract IDbCommand GetDbCommand(IDbConnection connection);

        /// <summary>
        /// Gets the data adapter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        protected abstract IDataAdapter GetDataAdapter(IDbCommand command);

        /// <summary>
        /// Parses the specified tables information.
        /// </summary>
        /// <param name="tablesInfo">The tables information.</param>
        /// <returns></returns>
        protected abstract IEnumerable<TableInfoModel> Parse(DataSet tablesInfo);
    }

    public interface ITableExportService
    {
        /// <summary>
        /// Export service name.
        /// </summary>
        /// <returns></returns>
        string ExportName();

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
        public IEnumerable<ColumnInfoModel> Columns { get; set; }
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
