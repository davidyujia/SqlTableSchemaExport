using System;
using System.Collections.Generic;
using System.Data;

namespace DbSchemaExporter.Core
{
    public interface IDbSchemaExporter
    {
        void Export(DbInfoModel dbInfo);
    }

    public abstract class DbSchemaExportProecss : IDisposable
    {
        protected IDbConnection Conn;

        protected DbSchemaExportProecss(IDbConnection conn)
        {
            Conn = conn;
            Conn.Open();
        }

        protected abstract IDbConnection GetConnection(DatabaseSettingModel setting);

        protected abstract IEnumerable<TableInfoModel> GetTableInfos();

        protected abstract IEnumerable<ColumnInfoModel> GetColumsInfos(string tableName);

        protected IEnumerable<TableInfoModel> FillTableInfoModel(IDbCommand cmd)
        {
            var infos = new List<TableInfoModel>();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                infos.Add(new TableInfoModel
                {
                    Name = Convert.ToString(reader["Name"]),
                    Comment = Convert.ToString(reader["Comment"]),
                });
            }

            return infos;
        }

        protected IEnumerable<ColumnInfoModel> FillColumnInfoModel(IDbCommand cmd)
        {
            var infos = new List<ColumnInfoModel>();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                infos.Add(new ColumnInfoModel
                {
                    Name = Convert.ToString(reader["Name"]),
                    Comment = Convert.ToString(reader["Comment"]),
                    Type = Convert.ToString(reader["Comment"]),
                    IsCanNull = Convert.ToBoolean(reader["IsCanNull"]),
                    IsPrimaryKey = Convert.ToBoolean(reader["IsPrimaryKey"]),
                    DefaultValue = Convert.ToString(reader["DefaultValue"]),
                });
            }

            return infos;
        }

        private void ShowTaskInfo(string message, params object[] args)
        {
            var msg = string.Format(message, args);
            Console.WriteLine(msg);
        }

        public void Start(DatabaseSettingModel setting, IDbSchemaExporter exporter)
        {
            ShowTaskInfo("Getting tables info...");
            var tableInfos = GetTableInfos();
            var list = new List<TableInfoWithColumnsModel>();
            foreach (var tableInfo in tableInfos)
            {
                ShowTaskInfo($"Getting \"{tableInfo.Name}\" columns info...");
                var columnInfos = GetColumsInfos(tableInfo.Name);
                list.Add(new TableInfoWithColumnsModel(tableInfo, columnInfos));
            }

            ShowTaskInfo("Exporting...");
            exporter.Export(new DbInfoModel
            {
                Name = setting.DatabaseName,
                Tables = list
            });
        }

        public void Dispose()
        {
            Conn.Close();
            Conn.Dispose();
            Conn = null;
        }
    }
}