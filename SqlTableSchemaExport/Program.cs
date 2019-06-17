using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTableSchemaExport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

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
                        Console.WriteLine("-s\tServer name or IP address");
                        Console.WriteLine("-d\tDatabase name");
                        Console.WriteLine("-u\tAccount");
                        Console.WriteLine("-p\tPassword");
                        return;
                    }

                    argList.Add(args[i], i + 1 >= args.Length ? string.Empty : args[i + 1].Replace("\"", string.Empty));
                }
            }
            string input;
            if (!argList.ContainsKey("-s"))
            {
                s: Console.WriteLine("Server name or IP address:");
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    goto s;
                }
                argList.Add("-s", input);
            }

            if (!argList.ContainsKey("-d"))
            {
                d: Console.WriteLine("Database name:");
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    goto d;
                }
                argList.Add("-d", input);
            }
            if (!argList.ContainsKey("-u"))
            {
                u: Console.WriteLine("Account:");
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    goto u;
                }
                argList.Add("-u", input);
            }
            if (!argList.ContainsKey("-p"))
            {
                input = string.Empty;
                p: Console.WriteLine("Password:");

                ConsoleKeyInfo key;
                while (true)
                {
                    key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else
                    {
                        input += key.KeyChar;
                        Console.Write("*");
                    }
                }

                if (string.IsNullOrEmpty(input))
                {
                    goto p;
                }
                argList.Add("-p", input);
            }

            var resultTable = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection($"Data Source={argList["-s"]};Initial Catalog={argList["-d"]};Persist Security Info=True;User ID={argList["-u"]};Password={argList["-p"]}");
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
                command.CommandTimeout = int.MaxValue;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = sqlCommandString;


                var adapter = new SqlDataAdapter(command);

                adapter.Fill(resultTable);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                System.Threading.Thread.Sleep(5000);
                return;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            var currentTableName = string.Empty;
            var sb = new StringBuilder();
            var isFrist = true;
            foreach (DataRow row in resultTable.Rows)
            {
                var table = Convert.ToString(row["Table"]);
                var column = Convert.ToString(row["Column"]);
                var dataType = Convert.ToString(row["DataType"]);
                var lengthX = Convert.ToInt32(row["Length"]);
                var length = lengthX == -1 ? "(MAX)" : lengthX == 0 ? "" : $"({lengthX})";
                var defaultValue = Convert.ToString(row["DefaultValue"]);
                var isNull = Convert.ToString(row["IsNull"]);
                var description = Convert.ToString(row["Description"]);

                if ("dbo.sysdiagrams" == table)
                {
                    continue;
                }

                if (currentTableName != table)
                {
                    currentTableName = table;
                    if (!isFrist)
                    {
                        sb.Append("</table>");
                    }

                    isFrist = false;
                    sb.Append($"<h1>{table}</h1>");
                    sb.Append(@"<table border=""1"" width=""100%"">");
                    sb.Append("<tr><th>欄位名稱</th><th>資料型態</th><th>預設值</th><th>允許NULL</th><th>描述</th></tr>");
                }
                sb.Append($"<tr><td>{column}</td><td>{dataType}{length}</td><td>{defaultValue}</td><td>{isNull}</td><td>{description}</td></tr>");
            }
            sb.Append("</table>");

            System.IO.File.WriteAllText($"{argList["-d"]}.html", sb.ToString());
        }
    }
}
