using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Windows.Forms;

namespace PackBeast;

public class PackBeastSettings : ISettings
{
    //Mandatory setting to allow enabling/disabling your plugin
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Start")]
    public HotkeyNode StartStopHotKey { get; set; } = new HotkeyNode(Keys.F7);

    [Menu("Stop")]
    public HotkeyNode StopHotKey { get; set; } = new HotkeyNode(Keys.Delete);

    [Menu("Action Delay")]
    public RangeNode<int> ActionDelay { get; set; } = new RangeNode<int>(200, 0, 2000);
}