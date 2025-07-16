using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.Shared.Enums;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PackBeast;

public partial class PackBeast : BaseSettingsPlugin<PackBeastSettings>
{
    private SharpDX.Vector2 windowOffset;
    private DateTime _lastExecutionTime = DateTime.MinValue;

    private bool isWork = false;

    private IList<Element> beastElements;

    private MouseActionType cursorActionType;

    SlotInventory[,] playerInventory = new SlotInventory[12, 5];

    Vector2 freeSlot = new Vector2(0, 0);

    public override bool Initialise()
    {
        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        cursorActionType = GameController.IngameState.IngameUi.Cursor.Action;

        beastElements = GameController.IngameState.IngameUi
                .GetChildAtIndex(47)
                ?.GetChildAtIndex(2)
                ?.GetChildAtIndex(0)
                ?.GetChildAtIndex(1)
                ?.GetChildAtIndex(1)
                ?.GetChildAtIndex(8)
                ?.GetChildAtIndex(0)
                ?.GetChildAtIndex(18)
                ?.GetChildAtIndex(1)
                ?.GetChildAtIndex(0).Children;

        return base.Initialise();
    }

    public override Job Tick()
    {
        if (Settings.StopHotKey.PressedOnce())
        {
            isWork = false;
            return null;
        }

        if (Settings.StartStopHotKey.PressedOnce())
        {
            isWork = !isWork;
        }

        cursorActionType = GameController.IngameState.IngameUi.Cursor.Action;

        if (!isWork || !InventoryIsOpen) return null;

        DestroyWindowCheck();

        var now = DateTime.Now;
        if ((now - _lastExecutionTime).TotalMilliseconds < Settings.ActionDelay)
            return null;

        var beastClass = beastElements.FirstOrDefault(x => x.IsVisible);
        if (beastClass == null)
        {
            isWork = false;
        }

        var beastsCurrent = beastClass.GetChildAtIndex(1).Children;
        var beast = beastsCurrent.FirstOrDefault(x => x.IsVisible);
        if (beast == null)
        {
            isWork = false;
        }

        freeSlot = SearchFreeSpace();

        if (freeSlot.IsZero || beastClass == null)
        {
            isWork = false;
        }

        if (cursorActionType == MouseActionType.UseItem)
        {
            GrabBeast(beast.GetClientRectCache.Center);
            return null;
        }

        if (cursorActionType == MouseActionType.HoldItem)
        {
            PlaceBeast();
            return null;
        }

        if (cursorActionType == MouseActionType.Free)
        {
            GetBestiaryOrb();
            return null;
        }

        return null;
    }

    public void DestroyWindowCheck()
    {
        var destroyWindow = GameController.IngameState.IngameUi.DestroyConfirmationWindow;
        if (destroyWindow.IsVisible)
        {
            Thread.Sleep(50);
            Utils.Keyboard.KeyDown(System.Windows.Forms.Keys.Escape);
            Thread.Sleep(50);
            Utils.Keyboard.KeyUp(System.Windows.Forms.Keys.Escape);
            Thread.Sleep(50);
        }
    }

    public bool ReleaseBeast(Vector2 pos)
    {
        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        Utils.Mouse.MouseMoveNonLinear(pos + windowOffset);

        Utils.Keyboard.KeyDown(System.Windows.Forms.Keys.LControlKey);

        Utils.Mouse.LeftDown(50);
        Utils.Mouse.LeftUp(50);

        Utils.Keyboard.KeyUp(System.Windows.Forms.Keys.LControlKey);

        Thread.Sleep(5);

        return true;
    }

    public bool PlaceBeast()
    {
        var itemsOnCursor = GameController.IngameState.ServerData.PlayerInventories.Where(x => x.TypeId == ExileCore.Shared.Enums.InventoryNameE.Cursor1);
        if (itemsOnCursor.First().Inventory.ItemCount == 0) return false;

        Utils.Mouse.MouseMoveNonLinear(freeSlot + windowOffset);
        Thread.Sleep(5);

        Utils.Mouse.LeftDown(50);
        Utils.Mouse.LeftUp(50);

        Thread.Sleep(5);

        return true;
    }

    private SharpDX.Vector2 SearchFreeSpace()
    {
        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        if (inventoryItems.Count > 60)
        {
            return new Vector2(0, 0);
        }

        SlotInventory[,] inventorySlot = new SlotInventory[12, 5];

        var inventoryRect = GameController.IngameState.IngameUi.InventoryPanel.Children[2].GetClientRectCache;
        var invSlotW = inventoryRect.Width / 12;
        var invSlotH = inventoryRect.Height / 5;

        float offsetX = inventoryRect.X;
        float offsetY = inventoryRect.Y;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new RectangleF(offsetX, offsetY, invSlotW, invSlotH);

                inventorySlot[x, y] = new SlotInventory(false, rectSlot, "");

                offsetY += invSlotW;
            }
            offsetY = inventoryRect.Y;
            offsetX += invSlotH - 2;
        }

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
                    inventorySlot[row, col] = new SlotInventory(true, inventoryItem.GetClientRect(), inventoryItem.Address.ToString());
                }
            }
        }

        // check for any empty slots
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (inventorySlot[x, y].Full == false)
                {
                    return inventorySlot[x, y].Rect.Center;
                }
            }
        }

        // no empty slots, so inventory is full
        return new Vector2(0, 0);
    }

    public bool GrabBeast(Vector2 beastPos)
    {
        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        Utils.Mouse.MouseMoveNonLinear(beastPos + windowOffset);
        Thread.Sleep(25);

        Utils.Mouse.LeftDown(25);
        Utils.Mouse.LeftUp(25);

        Thread.Sleep(25);

        return true;
    }

    public bool GetBestiaryOrb()
    {
        string bsOrb = "Metadata/Items/Currency/CurrencyItemiseCapturedMonster";

        var playerInventory = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        var bestiartOrbs = playerInventory.Where(item => item.Item.Metadata == bsOrb).ToList();

        if (!bestiartOrbs.Any()) return false;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        var firstItem = bestiartOrbs.FirstOrDefault();
        if (firstItem == null)
        {
            LogMessage("Could not find item.");
            return false;
        }

        Vector2 itemPos = firstItem.GetClientRect().Center;

        Utils.Mouse.MouseMoveNonLinear(itemPos + windowOffset);
        Thread.Sleep(25);

        Utils.Mouse.RightDown(25);
        Utils.Mouse.RightUp(25);

        Thread.Sleep(25);

        return true;
    }
}