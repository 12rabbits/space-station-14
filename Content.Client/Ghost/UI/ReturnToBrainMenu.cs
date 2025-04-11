using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;


namespace Content.Client.Ghost.UI;

public sealed class ReturnToBrainMenu : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button AcceptButton;

    public ReturnToBrainMenu()
    {
        Title = Loc.GetString("ghost-return-to-brain-title");
        Contents.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Children =
            {
                new Label()
                {
                    Text = Loc.GetString("ghost-return-to-brain-text")
                },
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    Align = AlignMode.Center,
                    Children =
                    {
                        (AcceptButton = new Button
                        {
                            Text = Loc.GetString("ghost-return-to-brain-accept-button"),
                        }),

                        (new Control()
                        {
                            MinSize = new Vector2(20, 0)
                        }),

                        (DenyButton = new Button
                        {
                            Text = Loc.GetString("ghost-return-to-brain-deny-button"),
                        })
                    }
                }
            }
        });
    }
}
