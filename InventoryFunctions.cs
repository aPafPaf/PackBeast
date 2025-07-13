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

    public void UpdateInventoryPlayer()
    {
        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        // track each inventory slot
        SlotInventory[,] inventorySlots = new SlotInventory[12, 5];

        //get pos inventory slots
        var inventoryRect = GameController.IngameState.IngameUi.GetChildFromIndices(37, 3, 25).GetClientRectCache;
        var invSlotW = inventoryRect.Width / 12;
        var invSlotH = inventoryRect.Height / 5;

        float offsetX = inventoryRect.X;
        float offsetY = inventoryRect.Y;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new(offsetX, offsetY, invSlotW, invSlotH);

                inventorySlots[x, y] = new SlotInventory(false, rectSlot, "");

                offsetY += invSlotW;
            }
            offsetY = inventoryRect.Y;
            offsetX += invSlotH;
        }

        // iterate through each item in the inventory and mark used slots
        foreach (var inventoryItem in inventoryItems)
        {
            int x = inventoryItem.PosX;
            int y = inventoryItem.PosY;
            int height = inventoryItem.SizeY;
            int width = inventoryItem.SizeX;
            for (int row = x; row < x + width; row++)
            {
                for (int col = y; col < y + height; col++)
                {
                    if (inventoryItem.Item.TryGetComponent(out Base itemBase))
                    {
                        inventorySlots[row, col] = new SlotInventory(true, inventoryItem.GetClientRect(), itemBase.Name);
                    }
                    else
                    {
                        inventorySlots[row, col] = new SlotInventory(true, inventoryItem.GetClientRect(), "");
                    }
                }
            }
        }

        playerInventory = inventorySlots;
    }

    void MoveItem(SlotInventory item)
    {
        var windowOffset = GameController.Window.GetWindowRectangle().Location;

        Utils.Keyboard.KeyDown(Keys.ControlKey);

        Utils.Mouse.MoveMouse(item.Rect.Center + windowOffset);
        Utils.Mouse.LeftDown(1);
        Utils.Mouse.LeftUp(1);

        Utils.Keyboard.KeyUp(Keys.ControlKey);
    }

    public Element FindChildRecursiveLocal(Element elem, string text, bool contains = false)
    {
        if (elem.Text == text || (contains && (elem.Text?.Contains(text) ?? false)))
        {
            return elem;
        }

        foreach (var child in elem.Children)
        {
            Element result = FindChildRecursiveLocal(child, text, contains);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
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
