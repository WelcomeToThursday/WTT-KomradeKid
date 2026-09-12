#if !UNITY_EDITOR
using EFT.NextObservedPlayer;
using UnityEngine;

namespace GameBoyEmulator.CustomEFTData;

public class CustomUsableItemInterfaceClass : IObservedUsableItem
{
	public void Initialize(GameObject gameObject)
	{
	}

	public void UpdateData(ObservedUsableItemUpdatedData observedUsableItemUpdatedData)
	{
	}

	public void Disable()
	{
	}
}
#endif