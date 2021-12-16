using ClosedXML.Excel;
using SqlTableSchemaExporter.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlTableSchemaExporter.Export.Excel
{
    public class XlsxExporter : ITableExportService
    {
        public string DefaultFileExtensionName()
        {
            return "XLSX";
        }

        public Stream Export(string dbName, IEnumerable<TableInfoModel> tables)
        {
            var ms = new MemoryStream();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var i = 1;

                foreach (var table in tables)
                {
                    var ws = wb.Worksheet(i++);

                    ws.Name = table.Name;

                    var j = 1;

                    ws.Cell(j, 1).Value = table.Name;
                    ws.Cell(j, 2).Value = table.Comment;

                    j++;

                    ws.Cell(j, 1).Value = "";
                    ws.Cell(j, 2).Value = "Column Name";
                    ws.Cell(j, 3).Value = "PK";
                    ws.Cell(j, 4).Value = "Data Type";
                    ws.Cell(j, 5).Value = "Default Value";
                    ws.Cell(j, 6).Value = "Allow NULL";
                    ws.Cell(j, 7).Value = "Comment";

                    j++;

                    foreach (var column in table.Columns)
                    {
                        ws.Cell(j, 1).Value = j;
                        ws.Cell(j, 2).Value = table.Columns[j].Name;
                        ws.Cell(j, 3).Value = table.Columns[j].IsPrimaryKey;
                        ws.Cell(j, 4).Value = table.Columns[j].DataType;
                        ws.Cell(j, 5).Value = table.Columns[j].IsCanNull;
                        ws.Cell(j, 6).Value = table.Columns[j].DefaultValue;
                        ws.Cell(j, 7).Value = table.Columns[j].Comment;
                    }
                }

                wb.SaveAs(ms);
            }

            return ms;
        }
    }
}
