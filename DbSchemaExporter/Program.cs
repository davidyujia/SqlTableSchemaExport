using System;
using System.Collections.Generic;
using System.IO;
using DbSchemaExporter.Core;
using DbSchemaExporter.MsSql;
using DbSchemaExporter.MySql;
using DbSchemaExporter.Postgresql;

namespace DbSchemaExporter
{
    class Program
    {
        const string _postgreSql = "pgsql";
        const string _msSql = "mssql";
        const string _mySql = "mysql";

        static void Main(string[] args)
        {
            try
            {
                var argList = ReadArgs(args);
                if (argList == null)
                {
                    return;
                }

                var setting = new DatabaseSettingModel
                {
                    DbType = GetSwitchInput(argList, "-t", "Database Type:\n1. SQL Server\n2. MySQL\n3. PostgreSQL", new Dictionary<string, string> { { "1", _msSql }, { "2", _mySql }, { "3", _postgreSql } }),
                    Host = GetInput(argList, "-s", "Server name or IP address:"),
                    DatabaseName = GetInput(argList, "-d", "Database name:"),
                    UserName = GetInput(argList, "-u", "Username:"),
                    Password = GetInput(argList, "-p", "Password:", true)
                };

                IDatabaseService service = null;

                switch (setting.DbType.ToLower())
                {
                    case _msSql:
                        service = new MsSqlService();
                        break;
                    case _mySql:
                        service = new MySqlService();
                        break;
                    case _postgreSql:
                        service = new PostgresqlService();
                        break;
                }


                IEnumerable<TableInfoWithColumnsModel> tableInfos = null;

                tableInfos = service.GetTableInfos(setting);

                if (tableInfos == null)
                {
                    throw new Exception("Can't get any table info");
                }

                IExporter exporter = new HtmlExporter();

                var stream = exporter.Export(setting, tableInfos);
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = new FileStream($"{setting.DatabaseName}.html", FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                System.Threading.Thread.Sleep(5000);
            }
        }

        static IDictionary<string, string> ReadArgs(string[] args)
        {
            var argList = new Dictionary<string, string>();

            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-e" || !args[i].StartsWith("-") || args[i].Length < 2)
                    {
                        continue;
                    }

                    if (args[i] == "-h")
                    {
                        Console.WriteLine("Argument:");
                        Console.WriteLine("-t\tDatabase Type");
                        Console.WriteLine("-s\tServer name or IP address");
                        Console.WriteLine("-d\tDatabase name");
                        Console.WriteLine("-u\tAccount");
                        Console.WriteLine("-p\tPassword");
                        return null;
                    }

                    argList.Add(args[i], i + 1 >= args.Length ? string.Empty : args[i + 1].Replace("\"", string.Empty));
                }
            }

            return argList;
        }

        static string GetInput(IDictionary<string, string> args, string key, string infoString, bool hideInput = false)
        {
            if (args.ContainsKey(key))
            {
                return args[key];
            }

        again: Console.WriteLine(infoString);

            var input = string.Empty;
            if (hideInput)
            {
                while (true)
                {
                    var readKey = Console.ReadKey(true);

                    if (readKey.Key == ConsoleKey.Backspace)
                    {
                        Console.Write("\b \b");
                    }
                    else if (readKey.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else
                    {
                        input += readKey.KeyChar;
                        Console.Write("*");
                    }
                }
            }
            else
            {
                input = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(input))
            {
                goto again;
            }

            return input;
        }

        private static string GetSwitchInput(IDictionary<string, string> args, string key, string infoString, IDictionary<string, string> keyValues)
        {
            if (args.ContainsKey(key))
            {
                return args[key];
            }

        again: Console.WriteLine(infoString);

            var input = string.Empty;

            input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || !keyValues.ContainsKey(input))
            {
                goto again;
            }

            return keyValues[input];
        }
    }
}
