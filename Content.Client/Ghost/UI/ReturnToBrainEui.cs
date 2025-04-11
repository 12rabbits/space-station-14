using Content.Client.Eui;
using Content.Shared.Ghost;
using Robust.Client.Graphics;

namespace Content.Client.Ghost.UI;

public sealed class ReturnToBrainEui : BaseEui
{
    private readonly ReturnToBrainMenu _menu;

    public ReturnToBrainEui()
    {
        _menu = new ReturnToBrainMenu();

        _menu.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new ReturnToBrainMessage(true));
            _menu.Close();
        };

        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new ReturnToBrainMessage(false));
            _menu.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        SendMessage(new ReturnToBrainMessage(false));
        _menu.Close();
    }
}
