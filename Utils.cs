using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RectangleF = SharpDX.RectangleF;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;

namespace PackBeast;

public partial class PackBeast
{
    public bool StashIsOpen => GameController.IngameState.IngameUi.StashElement.IsVisible;

    public bool InventoryIsOpen => GameController.IngameState.IngameUi.InventoryPanel.IsVisible;
}

struct SlotInventory
{
    public bool Full { get; set; }
    public SharpDX.RectangleF Rect { get; set; }
    public string Name { get; set; }

    public SlotInventory(bool full, SharpDX.RectangleF rect, string name)
    {
        this.Full = full;
        this.Rect = rect;
        this.Name = name;
    }
}
