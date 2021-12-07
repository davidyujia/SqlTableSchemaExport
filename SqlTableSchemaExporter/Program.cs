using CommandLine;
using SqlTableSchemaExporter.Core;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SqlTableSchemaExporter
{
    public class Options
    {
        [Option('t', "type", Required = true, HelpText = "Database type.")]
        public string DbType { get; set; }
        [Option('c', "connectionString", Required = true, HelpText = "Database's connection string.")]
        public string ConnectionString { get; set; }
        [Option('e', "export", Required = false, Default = "html", HelpText = "Export file type.")]
        public string ExportType { get; set; }
        [Option('f', "fileName", Required = false, HelpText = "Export file name.")]
        public string ExportFileName { get; set; }
        [Option('n', "name", Required = false, HelpText = "Export database name.")]
        public string DatabaseName { get; set; }
    }

    internal class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args).MapResult(new Process().Run, e => 1);
        }
    }

    internal class Process
    {
        public int Run(Options option)
        {
            var service = GetTypeInstances<IDataSourceService>(x => x.DbTypeName(), option.DbType);

            var exportService = GetTypeInstances<ITableExportService>(x => x.DefaultFileExtensionName(), option.ExportType);

            var tableInfos = service.GetTableSchema(option.ConnectionString);

            var dbName = !string.IsNullOrWhiteSpace(option.DatabaseName)
                ? option.DatabaseName
                : service.TryGetDbName(option.ConnectionString, out var name)
                ? name : "Database";

            var stream = exportService.Export(dbName, tableInfos);

            var exportFileName = string.IsNullOrWhiteSpace(option.ExportFileName)
                ? $"{service.DbTypeName()}.{exportService.DefaultFileExtensionName()}"
                : option.ExportFileName;

            ExportToFile(stream, option.ExportFileName);

            return 0;
        }

        T GetTypeInstances<T>(Func<T, string> matchFunc, string match)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(T)
                .IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (T)Activator.CreateInstance(x))
                .Where(x => matchFunc(x).ToLower() == match)
                .FirstOrDefault();
        }

        void ExportToFile(Stream stream, string fileName)
        {
            var fi = new FileInfo(fileName);

            var ext = fi.Extension;

            var name = string.IsNullOrWhiteSpace(ext) ? fi.FullName : fi.FullName[..fi.FullName.LastIndexOf(ext)];

            var tempFileName = $"{name}{ext}";

            var i = 0;

            while (File.Exists(tempFileName))
            {
                tempFileName = $"{name}({++i}){ext}";
            }

            using var fileStream = File.Create(tempFileName);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }
    }
}
