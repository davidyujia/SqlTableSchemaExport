using SqlTableSchemaExporter.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SqlTableSchemaExporter
{
    internal class HtmlExporter : ITableExportService
    {
        public string DefaultFileExtensionName()
        {
            return "html";
        }

        public Stream Export(string dbName, IEnumerable<TableInfoModel> tables)
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                sb.AppendLine($@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content = ""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"">
    <title>{dbName} Table Schema</title>
    <style>
        table {{
            width: 100%;
            border-collapse: collapse;
        }}

        table, th, td {{
            border: 1px solid black;
        }}
    </style>
</head>
<body>");
                sb.AppendLine($"<h1>{table.Name}</h1>");
                sb.AppendLine(@"<table>");
                sb.AppendLine("<tr><th>Column Name</th><th>PK</th><th>Data Type</th><th>Default Value</th><th>Allow NULL</th><th>Comment</th></tr>");

                foreach (var columnInfo in table.Columns)
                {
                    sb.AppendLine($"<tr><td>{columnInfo.Name}</td><td>{columnInfo.IsPrimaryKey}</td><td>{columnInfo.DataType}</td><td>{columnInfo.DefaultValue}</td><td>{columnInfo.IsCanNull}</td><td>{columnInfo.Comment}</td></tr>");
                }

                sb.Append("</table></body></html>");
            }
            var byteArray = Encoding.UTF8.GetBytes(sb.ToString());

            return new MemoryStream(byteArray);
        }
    }
}
