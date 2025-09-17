using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rental.Data;

public static class SchemaUpdater
{
    private const string EnsureTotalUnitsColumnSql = """
IF COL_LENGTH(N'[dbo].[Properties]', 'TotalUnits') IS NULL
BEGIN
    ALTER TABLE [dbo].[Properties]
        ADD [TotalUnits] INT NOT NULL CONSTRAINT [DF_Properties_TotalUnits] DEFAULT(0) WITH VALUES;
END;
""";

    private const string EnsureUnitsTableSql = """
IF OBJECT_ID(N'[dbo].[Units]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Units]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PropertyId] INT NOT NULL,
        [UnitNumber] NVARCHAR(60) NOT NULL,
        [UnitType] NVARCHAR(100) NOT NULL,
        [Notes] NVARCHAR(200) NULL,
        CONSTRAINT [PK_Units] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Units_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Units_PropertyId] ON [dbo].[Units]([PropertyId]);
END;
""";

    private const string BackfillTotalUnitsSql = """
IF COL_LENGTH(N'[dbo].[Properties]', 'TotalUnits') IS NOT NULL
BEGIN
    UPDATE P
    SET TotalUnits = ISNULL(Counts.UnitCount, 0)
    FROM [dbo].[Properties] AS P
    OUTER APPLY
    (
        SELECT COUNT(*) AS UnitCount
        FROM [dbo].[Units] AS U
        WHERE U.PropertyId = P.Id
    ) AS Counts
    WHERE P.TotalUnits = 0;
END;
""";

    public static async Task EnsureLatestSchemaAsync(RentalContext context)
    {
        if (!context.Database.IsSqlServer())
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync(EnsureTotalUnitsColumnSql);
        await context.Database.ExecuteSqlRawAsync(EnsureUnitsTableSql);
        await context.Database.ExecuteSqlRawAsync(BackfillTotalUnitsSql);
    }
}
