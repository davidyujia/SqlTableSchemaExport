using System.Collections.Generic;
using System.IO;
using System.Text;
using DbSchemaExporter.Core;

namespace DbSchemaExporter
{
    public interface IExporter
    {
        Stream Export(DatabaseSettingModel setting, IEnumerable<TableInfoWithColumnsModel> tableInfos);
    }

    public class HtmlExporter : IExporter
    {
        public Stream Export(DatabaseSettingModel setting, IEnumerable<TableInfoWithColumnsModel> tableInfos)
        {
            var sb = new StringBuilder();
            foreach (var tableInfo in tableInfos)
            {
                sb.AppendLine($@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content = ""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content = ""ie=edge"">
    <title>{setting.DatabaseName} Table Schema</title>
</head>
<body>");
                var comment = string.IsNullOrWhiteSpace(tableInfo.Comment) ? string.Empty : $"<h4>{tableInfo.Comment}</h4>";
                sb.AppendLine($"<h1>{tableInfo.Name}</h1>{comment}");
                sb.AppendLine(@"<table border=""1"" width=""100%"">");
                sb.AppendLine("<tr><th>Column Name</th><th>Data Type</th><th>Default Value</th><th>Allow NULL</th><th>Comment</th></tr>");

                foreach (var columnInfo in tableInfo.Columns)
                {
                    sb.AppendLine($"<tr><td>{columnInfo.Name}</td><td>{columnInfo.Type}</td><td>{columnInfo.DefaultValue}</td><td>{columnInfo.IsCanNull}</td><td>{columnInfo.Comment}</td></tr>");
                }

                sb.Append("</table></body></html>");
            }
            var byteArray = Encoding.UTF8.GetBytes(sb.ToString());

            return new MemoryStream(byteArray);
        }
    }
}
