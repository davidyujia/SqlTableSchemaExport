using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DbSchemaExporter.Core;

namespace DbSchemaExporter.MsSql
{
    public class MsSqlService : IDatabaseService
    {
        public IEnumerable<TableInfoWithColumnsModel> GetTableInfos(DatabaseSettingModel settingModel)
        {
            var resultTable = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection($"Data Source={settingModel.Host};Initial Catalog={settingModel.DatabaseName};Persist Security Info=True;User ID={settingModel.UserName};Password={settingModel.Password}");
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
                command.CommandTimeout = 120;
                command.CommandType = System.Data.CommandType.Text;
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

            var result = new List<TableInfoWithColumnsModel>();

            TableInfoModel tableModel = null;
            var columnInfos = new List<ColumnInfoModel>();
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
