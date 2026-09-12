#if !UNITY_EDITOR
using EFT.InventoryLogic;
using System.Collections.Generic;

namespace GameBoyEmulator.CustomEFTData;

public abstract class NewTemplateIdToObjectMappingClass
{
    public static readonly List<TemplateIdToObjectType> CustomMappings =
    [
        // Add CustomUsableItem Item
        new(
            "66e42bd851fa456a1ee37885", // Template ID
            typeof(CustomUsableItem),   // Item type
            typeof(CompoundItemTemplate),         // Template type
            (id, template) => new CustomUsableItem(id, (CompoundItemTemplate)template) // Constructor
        ),
        // Add GameBoyCartridge Template
        new(
            "66f16b85ed966fb78f5563d8", // Template ID
            null,   // Item type
            typeof(GameBoyModTemplateType),         // Template type
            null // Constructor
        ),
        // Add GameBoyCartridge Item
        new(
            "66f17b4cb59dbccbf12990e6", // Template ID
            typeof(GameBoyCartridge),   // Item type
            typeof(GameBoyModTemplateType),         // Template type
            (id, template) => new GameBoyCartridge(id, (GameBoyModTemplateType)template) // Constructor
        ),
        // Add GameBoy Accessory Item
        new(
            "6704271a4cc9e20c610eb120", // Template ID
            typeof(GameBoyAccessory),   // Item type
            typeof(GameBoyModTemplateType),         // Template type
            (id, template) => new GameBoyAccessory(id, (GameBoyModTemplateType)template) // Constructor
        )
    ];
}
#endif