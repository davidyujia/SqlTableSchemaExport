using SqlTableSchemaExporter.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlTableSchemaExporter.MSSQL
{

    public class Service : IDataSourceService
    {
        public string DbTypeName()
        {
            return "MSSQL";
        }

        public IEnumerable<TableInfoModel> GetTableSchema(string connectionString)
        {
            var resultTable = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                #region SqlCommandString
                var sqlCommandString = @"
SELECT a.Table_schema +'.'+a.Table_name   as [Table]
       ,b.COLUMN_NAME                     as [Column]   
       ,b.DATA_TYPE                       as [DataType]   
       ,isnull(b.CHARACTER_MAXIMUM_LENGTH,'') as [Length]   
       ,isnull(b.COLUMN_DEFAULT,'')           as [DefaultValue]   
       ,b.IS_NULLABLE                         as [IsNull]   
       ,( SELECT value   
          FROM fn_listextendedproperty (NULL, 'schema', a.Table_schema, 'table', a.TABLE_NAME, 'column', default)   
          WHERE name='MS_Description' and objtype='COLUMN'    
          and objname Collate Chinese_Taiwan_Stroke_CI_AS = b.COLUMN_NAME   
        ) as [Description]   
FROM INFORMATION_SCHEMA.TABLES  a   
 LEFT JOIN INFORMATION_SCHEMA.COLUMNS b ON a.TABLE_NAME = b.TABLE_NAME   
WHERE TABLE_TYPE='BASE TABLE'
ORDER BY a.TABLE_NAME , b.ORDINAL_POSITION 
";

                #endregion
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlCommandString;


                var adapter = new SqlDataAdapter(command);

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
                var lengthX = Convert.ToInt32(row["Length"]);
                var length = lengthX == -1 ? "(MAX)" : lengthX == 0 ? "" : $"({lengthX})";
                var defaultValue = Convert.ToString(row["DefaultValue"]);
                var isNull = Convert.ToString(row["IsNull"]) != "NO";
                var description = Convert.ToString(row["Description"]);

                if ("dbo.sysdiagrams" == table)
                {
                    continue;
                }

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
