using System;
using System.Collections.Generic;
using System.Data;
using DbSchemaExporter.Core;
using MySql.Data.MySqlClient;

namespace DbSchemaExporter.MySql
{
    public class MySqlService : IDatabaseService
    {
        public IEnumerable<TableInfoWithColumnsModel> GetTableInfos(DatabaseSettingModel settingModel)
        {
            var resultTable = new DataTable();
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(
                    settingModel.HasConnectionString ? settingModel.ConnectionString :
                     $"server={settingModel.Host};port={settingModel.Port};database={settingModel.DatabaseName};user id={settingModel.UserName};password={settingModel.Password};charset=utf8;");
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
                command.CommandTimeout = 120;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = sqlCommandString;
                command.Parameters.AddWithValue("@dbname", settingModel.DatabaseName);

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

            var result = new List<TableInfoWithColumnsModel>();

            TableInfoModel tableModel = null;
            var columnInfos = new List<ColumnInfoModel>();
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
                        result.Add(new TableInfoWithColumnsModel(tableModel, columnInfos));
                    }
                    tableModel = new TableInfoModel { Name = table };
                    columnInfos = new List<ColumnInfoModel>();
                }

                columnInfos.Add(new ColumnInfoModel
                {
                    Name = column,
                    Type = $"{dataType}{length}",
                    DefaultValue = defaultValue,
                    IsCanNull = isNull,
                    Comment = description,
                });
            }

            result.Add(new TableInfoWithColumnsModel(tableModel, columnInfos));

            return result;
        }
    }
}
