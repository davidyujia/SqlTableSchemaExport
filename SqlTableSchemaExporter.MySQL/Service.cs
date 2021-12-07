using MySql.Data.MySqlClient;
using SqlTableSchemaExporter.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace SqlTableSchemaExporter.MySQL
{
    public class Service : IDataSourceService
    {
        public string DbTypeName()
        {
            return "MySQL";
        }

        public IEnumerable<TableInfoModel> GetTableSchema(string connectionString)
        {
            var resultTable = new DataTable();
            MySqlConnection connection = null;
            try
            {
                if (!TryGetDbName(connectionString, out var dbName))
                {
                    throw new Exception();
                }

                connection = new MySqlConnection(connectionString);
                connection.Open();

                #region SqlCommandString
                var sqlCommandString = @"
SELECT 
TABLE_NAME AS ""Table"",
COLUMN_NAME AS ""Column"",
DATA_TYPE AS ""DataType"",
CHARACTER_MAXIMUM_LENGTH AS ""Length"",
COLUMN_DEFAULT AS ""DefaultValue"",
IS_NULLABLE AS ""IsNull"",
COLUMN_COMMENT AS ""Description""
FROM  INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA like @dbname
ORDER BY TABLE_NAME , ORDINAL_POSITION
";

                #endregion
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlCommandString;
                command.Parameters.AddWithValue("@dbname", dbName);

                var adapter = new MySqlDataAdapter(command);

                adapter.Fill(resultTable);

            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            var result = new List<TableInfoModel>();

            TableInfoModel tableModel = null;

            foreach (DataRow row in resultTable.Rows)
            {
                var table = Convert.ToString(row["Table"]);
                var column = Convert.ToString(row["Column"]);
                var dataType = Convert.ToString(row["DataType"]);
                var lengthX = Convert.ToString(row["Length"]);
                var length = string.IsNullOrWhiteSpace(lengthX) ? string.Empty : $"({lengthX})";
                var defaultValue = Convert.ToString(row["DefaultValue"]);
                var isNull = Convert.ToString(row["IsNull"]) != "NO";
                var description = Convert.ToString(row["Description"]);

                if (tableModel == null || tableModel.Name != table)
                {
                    if (tableModel != null)
                    {
                        result.Add(tableModel);
                    }
                    tableModel = new TableInfoModel { Name = table, Columns = new List<ColumnInfoModel>() };
                }

                tableModel.Columns.Add(new ColumnInfoModel
                {
                    Name = column,
                    DataType = $"{dataType}{length}",
                    DefaultValue = defaultValue,
                    IsCanNull = isNull,
                    Comment = description,
                });
            }

            result.Add(tableModel);

            return result;
        }

        public bool TryGetDbName(string connectionString, out string dbName)
        {
            return Extensions.TryGetDbName(connectionString, "database", out dbName);
        }
    }
}
