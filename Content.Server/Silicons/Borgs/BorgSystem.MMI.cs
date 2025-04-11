using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Server.Roles;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Server.Silicons.Borgs;

/// <inheritdoc/>
public sealed partial class BorgSystem
{

    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    private readonly Dictionary<ICommonSession, ReturnToBrainEui> _openEuis = new();

    public void InitializeMMI()
    {
        SubscribeLocalEvent<MMIComponent, ComponentInit>(OnMMIInit);
        SubscribeLocalEvent<MMIComponent, EntInsertedIntoContainerMessage>(OnMMIEntityInserted);
        SubscribeLocalEvent<MMIComponent, MindAddedMessage>(OnMMIMindAdded);
        SubscribeLocalEvent<MMIComponent, MindRemovedMessage>(OnMMIMindRemoved);

        SubscribeLocalEvent<MMILinkedComponent, MindAddedMessage>(OnMMILinkedMindAdded);
        SubscribeLocalEvent<MMILinkedComponent, EntGotRemovedFromContainerMessage>(OnMMILinkedRemoved);
    }

    private void OnMMIInit(EntityUid uid, MMIComponent component, ComponentInit args)
    {
        if (!TryComp<ItemSlotsComponent>(uid, out var itemSlots))
            return;

        if (ItemSlots.TryGetSlot(uid, component.BrainSlotId, out var slot, itemSlots))
            component.BrainSlot = slot;
        else
            ItemSlots.AddItemSlot(uid, component.BrainSlotId, component.BrainSlot, itemSlots);
    }

    private void OnMMIEntityInserted(EntityUid uid, MMIComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.BrainSlotId)
            return;

        var ent = args.Entity;
        var linked = EnsureComp<MMILinkedComponent>(ent);
        linked.LinkedMMI = uid;
        Dirty(uid, component);

        if (_mind.TryGetMind(ent, out var mindId, out var mind))
        {
            // send return prompt first if player is logged in & ghosted
            if (mind.Session is { } session && mind.CurrentEntity != ent)
            {
                OpenEui(session, ent);
            }
            else
            {
                _mind.TransferTo(mindId, uid, true, mind: mind);

                if (!_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
                    _roles.MindAddRole(mindId, "MindRoleSiliconBrain", silent: true);
            }
        }

        _appearance.SetData(uid, MMIVisuals.BrainPresent, true);
    }

    private void OnMMIMindAdded(EntityUid uid, MMIComponent component, MindAddedMessage args)
    {
        _appearance.SetData(uid, MMIVisuals.HasMind, true);
    }

    private void OnMMIMindRemoved(EntityUid uid, MMIComponent component, MindRemovedMessage args)
    {
        _appearance.SetData(uid, MMIVisuals.HasMind, false);
    }

    private void OnMMILinkedMindAdded(EntityUid uid, MMILinkedComponent component, MindAddedMessage args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind) ||
            component.LinkedMMI == null)
            return;

        _mind.TransferTo(mindId, component.LinkedMMI, true, mind: mind);
    }

    private void OnMMILinkedRemoved(EntityUid uid, MMILinkedComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (Terminating(uid))
            return;

        if (component.LinkedMMI is not { } linked)
            return;

        // Close the return prompt if the mind is still in the brain
        if (_mind.TryGetMind(uid, out var brainId, out var brainMind) && brainMind.Session is { } session)
        {
            CloseEui(session);
        }
        RemComp(uid, component);

        if (_mind.TryGetMind(linked, out var mindId, out var mind))
        {
            if (_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
                _roles.MindRemoveRole<SiliconBrainRoleComponent>(mindId);

            _mind.TransferTo(mindId, uid, true, mind: mind);
        }

        _appearance.SetData(linked, MMIVisuals.BrainPresent, false);
    }

    internal void TransferMindToMMI(EntityUid ent)
    {
        if (!TryComp<MMILinkedComponent>(ent, out var linked) || !_mind.TryGetMind(ent, out var mindId, out var mind))
            return;


        _mind.TransferTo(mindId, linked.LinkedMMI, true, mind: mind);

        if (!_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
            _roles.MindAddRole(mindId, "MindRoleSiliconBrain", silent: true);
    }

    public void OpenEui(ICommonSession session, EntityUid owner)
    {
        if (_openEuis.ContainsKey(session))
            return;
        var eui = new ReturnToBrainEui(owner, this);
        _euiManager.OpenEui(eui, session);
        _openEuis.Add(session, eui);
    }

    public void CloseEui(ICommonSession session)
    {
        if (!_openEuis.TryGetValue(session, out var eui))
            return;

        _openEuis.Remove(session);
        eui.Close();
    }
}
