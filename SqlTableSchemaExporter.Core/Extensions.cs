using System;
using System.Collections.Generic;
using System.Text;

namespace SqlTableSchemaExporter.Core
{
    public static class Extensions
    {
        public static bool TryGetDbName(string connectionString, string dbTag, out string dbName)
        {
            dbName = null;

            var sp1 = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in sp1)
            {
                var sp2 = s.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (sp2.Length != 2 || sp2[0].Trim().ToLower() != dbTag.Trim())
                {
                    continue;
                }

                dbName = sp2[1].Trim();

                return true;
            }

            return false;
        }
    }
}
