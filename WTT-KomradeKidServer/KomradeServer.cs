using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using System.Reflection;
using Path = System.IO.Path;
using Range = SemanticVersioning.Range;

namespace KomradeKidServer;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = ModConstants.Guid;
    public string Name { get; init; } = ModConstants.Name;
    public string Author { get; init; } = ModConstants.Author;
    public List<string>? Contributors { get; init; }

    public SemanticVersioning.Version Version { get; init; } =
        new(ModConstants.Version);

    public Range SptVersion { get; init; } =
        new(ModConstants.SptVersion);

    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = new(ModConstants.License);
    public bool HasPrepatcher { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.Preload + 2)]
public class KomradeServer(
    WTTServerCommonLib.WTTServerCommonLib wttCommon,
    TemplateTable templateTable) : IOnLoad
{

    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        
        Assembly assembly = Assembly.GetExecutingAssembly();

        var itemsDb = templateTable.Items;

        wttCommon.CustomSlotImageService.CreateSlotImages(assembly);
        await wttCommon.CustomItemParentService.CreateCustomParents(assembly);
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly, Path.Join("db", "CustomCartridges"));
        await Task.CompletedTask;
    }
}
