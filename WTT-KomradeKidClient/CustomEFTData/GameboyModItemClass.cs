#if !UNITY_EDITOR
using ChartAndGraph;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WTTClientCommonLib.Attributes;


namespace GameBoyEmulator.CustomEFTData;

[CustomParent("66f16b85ed966fb78f5563d8", // Template ID
            null,   // Item type
            typeof(GameBoyModTemplateType)      // Template type
)]
public class GameBoyModTemplateType(string romName, string cartridgeImage, string accessoryType)
    : CompoundItemTemplate
{
    public readonly string RomName = romName;
    public readonly string CartridgeImage = cartridgeImage;
    public readonly string AccessoryType = accessoryType;
}

public class GameBoyModItemType(string id, GameBoyModTemplateType template) : CompoundItem(id, template)
{
}

[CustomParent(
                "6704271a4cc9e20c610eb120", // Template ID
            typeof(GameBoyAccessory),   // Item type
            typeof(GameBoyModTemplateType)         // Template type
)]
public class GameBoyAccessory : GameBoyModItemType
{
    public GameBoyAccessory(string id, GameBoyModTemplateType template) : base(id, template)
    {
        AccessoryType = template.AccessoryType;
        Components.Add(_tag = new TagComponent(this));
    }

    public string AccessoryType { get; }


    public override IEnumerable<EItemInfoButton> ItemInteractionButtons
    {
        get
        {
            foreach (var itemInfoButton in GetBaseInteractions())
            {
                yield return itemInfoButton;
            }
            yield return EItemInfoButton.Install;
            yield return EItemInfoButton.Uninstall;
            if (!string.IsNullOrEmpty(_tag?.Name))
            {
                yield return EItemInfoButton.ResetTag;
            }
        }
    }


    private IEnumerable<EItemInfoButton> GetBaseInteractions()
    {
        return base.ItemInteractionButtons;
    }

    [SimpleAttribute] private readonly TagComponent _tag;

}

[CustomParent(
                "66f17b4cb59dbccbf12990e6", // Template ID
            typeof(GameBoyCartridge),   // Item type
            typeof(GameBoyModTemplateType)         // Template type
    )]
public class GameBoyCartridge : GameBoyModItemType
{
    public GameBoyCartridge(string id, GameBoyModTemplateType template) : base(id, template)
    {
        RomName = template.RomName;
        CartridgeImage = template.CartridgeImage;
        
        Components.Add(_tag = new TagComponent(this));
    }

    public string RomName { get; }
    private string CartridgeImage { get; }

    public override IEnumerable<EItemInfoButton> ItemInteractionButtons
    {
        get
        {
            foreach (var itemInfoButton in method_41())
            {
                yield return itemInfoButton;
            }
            yield return EItemInfoButton.Install;
            yield return EItemInfoButton.Uninstall;
            if (!string.IsNullOrEmpty(_tag?.Name))
            {
                yield return EItemInfoButton.ResetTag;
            }
        }
    }

    private IEnumerable<EItemInfoButton> method_41()
    {
        return base.ItemInteractionButtons;
    }

    [SimpleAttribute] private readonly TagComponent _tag;

    [CanBeNull]
    public GameBoyCartridge GetCurrentCartridge()
    {
        return this;
    }

    public IEnumerable<CustomUsableItem> GetSuitableGameBoy(CompoundItem[] collections)
    {
        CustomUsableItem[] array = collections.GetAllItemsFromCollections()
            .Concat(collections)
            .OfType<CustomUsableItem>()
            .Distinct()
            .ToArray();

        List<CustomUsableItem> list = new List<CustomUsableItem>();
        foreach (CustomUsableItem gameboy in array)
        {
            if (gameboy.IsCartridgeSuitable(this))
            {
                list.Add(gameboy);
            }
        }
        return list;
    }
    public void ApplyStickerTexture(GameObject cartridgeObject)
    {
        if (!string.IsNullOrEmpty(CartridgeImage))
        {
            var stickerPlaneRenderer = GetStickerRenderer(cartridgeObject);

            if (stickerPlaneRenderer != null)
            {
                Texture2D stickerTexture = LoadTextureFromFile(CartridgeImage);

                if (stickerTexture != null)
                {
                    stickerPlaneRenderer.material.mainTexture = stickerTexture;
                    stickerPlaneRenderer.enabled = true;
                }
            }
        }
    }

    private Renderer GetStickerRenderer(GameObject cartridgeObject)
    {
        var stickerPlaneObject = cartridgeObject.transform.Find("StickerPlane");

        if (stickerPlaneObject != null)
        {
            return stickerPlaneObject.GetComponent<Renderer>();
        }

        return null;
    }

    private Texture2D LoadTextureFromFile(string fileName)
    {

        string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string path = Path.Combine(pluginPath ?? throw new InvalidOperationException(), "Images", fileName);

        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(fileData))
            {
                return texture;
            }
        }
        return null;
    }

}
#endif