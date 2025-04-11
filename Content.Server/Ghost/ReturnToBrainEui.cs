using Content.Server.EUI;
using Content.Server.Silicons.Borgs;
using Content.Shared.Eui;
using Content.Shared.Ghost;

namespace Content.Server.Ghost;

public sealed class ReturnToBrainEui : BaseEui
{
    private readonly EntityUid _mindId;
    private readonly BorgSystem _borgSystem;

    public ReturnToBrainEui(EntityUid mindId, BorgSystem borgSystem)
    {
        _mindId = mindId;
        _borgSystem = borgSystem;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not ReturnToBrainMessage choice || !choice.Accepted)
        {
            Close();
            // Raise deny event
            return;
        }

        _borgSystem.TransferMindToMMI(_mindId);
        Close();

    }

    public override void Closed()
    {
        base.Closed();

        _borgSystem.CloseEui(Player);
    }
}
