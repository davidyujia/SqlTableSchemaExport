using Npgsql;
using SqlTableSchemaExporter.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace SqlTableSchemaExporter.PostgreSQL
{
    public class Service : IDataSourceService
    {
        public string DbTypeName()
        {
            return "Npgsql";
        }

        public IEnumerable<TableInfoModel> GetTableSchema(string connectionString)
        {
            var resultTable = new DataTable();
            NpgsqlConnection connection = null;
            try
            {
                if (!TryGetDbName(connectionString, out var dbName))
                {
                    throw new Exception();
                }

                connection = new NpgsqlConnection(connectionString);
                connection.Open();

                #region SqlCommandString
                var sqlCommandString = @"SELECT col.table_catalog, col.table_schema,col.table_name, col.column_name, col.column_default, col.is_nullable, col.data_type,  col.character_maximum_length, g.description
FROM information_schema.columns AS col

LEFT JOIN (SELECT c.relname AS table_name, a.attname As column_name,  d.description
   FROM pg_class As c
    INNER JOIN pg_attribute As a ON c.oid = a.attrelid
   LEFT JOIN pg_namespace n ON n.oid = c.relnamespace
   LEFT JOIN pg_tablespace t ON t.oid = c.reltablespace
   LEFT JOIN pg_description As d ON (d.objoid = c.oid AND d.objsubid = a.attnum)
   WHERE  c.relkind IN('r', 'v') AND  n.nspname = 'public'
   ORDER BY n.nspname, c.relname, a.attname) AS g ON col.table_name = g.table_name AND col.column_name = g.column_name

WHERE table_schema NOT IN ('information_schema' , 'pg_catalog' ,'topology')
AND table_catalog = @dbname
AND col.table_name NOT IN ('raster_overviews','raster_columns','spatial_ref_sys')
ORDER BY col.table_schema ASC, col.table_name ASC, col.ordinal_position ASC;";

                #endregion
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlCommandString;

                command.Parameters.AddWithValue("@dbname", dbName);

                var adapter = new NpgsqlDataAdapter(command);

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
                var schema = Convert.ToString(row["table_schema"]);
                var table = schema == "public" ? Convert.ToString(row["table_name"]) : $"{schema}.{Convert.ToString(row["table_name"])}";
                var column = Convert.ToString(row["column_name"]);
                var dataType = Convert.ToString(row["data_type"]);
                var lengthX = row["character_maximum_length"] == DBNull.Value ? 0 : Convert.ToInt32(row["character_maximum_length"]);
                var length = lengthX == -1 ? "(MAX)" : lengthX == 0 ? "" : $"({lengthX})";
                var defaultValue = Convert.ToString(row["column_default"]);
                var isNull = Convert.ToString(row["is_nullable"]) != "NO";
                var description = Convert.ToString(row["description"]);

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
