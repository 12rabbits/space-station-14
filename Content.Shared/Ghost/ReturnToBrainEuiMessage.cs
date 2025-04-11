using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost;

[Serializable, NetSerializable]
public sealed class ReturnToBrainMessage : EuiMessageBase
{
    public readonly bool Accepted;

    public ReturnToBrainMessage(bool accepted)
    {
        Accepted = accepted;
    }
}
