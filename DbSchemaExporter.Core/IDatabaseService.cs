using System.Collections.Generic;

namespace DbSchemaExporter.Core
{
    public interface IDatabaseService
    {
        IEnumerable<TableInfoWithColumnsModel> GetTableInfos(DatabaseSettingModel settingModel);
    }
}
