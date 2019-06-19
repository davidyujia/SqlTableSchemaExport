using System;
using System.Collections.Generic;
using DbSchemaExporter.Core;
using RazorEngine;
using RazorEngine.Templating;

namespace DbSchemaExporter.Razor
{
    public class RazorExporter : IDbSchemaExporter
    {
        public void Export(DbInfoModel dbInfo)
        {
            string template = @"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv="""" X-UA-Compatible content="""" ie=""edge"">
    <title>@Model.Name Table Schema</title>
</head>

<body>
    @foreach(var tableInfo in Model.Tables)
    {
    <h1>@tableInfo.Name</h1>
    <table style=""border: 1px"" width=""100%"">
        <tr>
            <th>Column Name</th>
            <th>Data Type</th>
            <th>Default Value</th>
            <th>Allow Null</th>
            <th>Description</th>
        </tr>
        @foreach(var columnInfo in tableInfo.Columns)
        {
        <tr>
            <td>@columnInfo.Name</td>
            <td>@columnInfo.Type</td>
            <td>@columnInfo.DefaultValue</td>
            <td>@columnInfo.IsCanNull</td>
            <td>@columnInfo.Comment</td>
        </tr>
        }
    </table>
    }
</body>

<body>";
            
            var result = Engine.Razor.RunCompile(template, "templateKey", null, dbInfo);

            System.IO.File.WriteAllText($"{dbInfo.Name}.html",result);
        }
    }
}
