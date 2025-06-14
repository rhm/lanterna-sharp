using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// Abstract implementation of IBorder interface that has some of the methods filled out. If you want to create
/// your own IBorder implementation, you should probably extend from this.
/// </summary>
public abstract class AbstractBorder : AbstractComposite<IBorder>, IBorder
{
    public override void SetComponent(IComponent? component)
    {
        base.SetComponent(component);
        if (component != null)
        {
            component.Position = TerminalPosition.TopLeftCorner;
        }
    }

    public new IBorder.IBorderRenderer Renderer => (IBorder.IBorderRenderer)base.Renderer;

    public virtual IBorder SetSize(TerminalSize size)
    {
        Size = size;
        var component = GetComponent();
        if (component != null)
        {
            component.Size = GetWrappedComponentSize(size);
        }
        return Self;
    }

    public override ILayoutData? LayoutData
    {
        get
        {
            var component = GetComponent();
            if (component == null)
            {
                return base.LayoutData;
            }
            return component.LayoutData;
        }
    }

    public override IComponent SetLayoutData(ILayoutData? layoutData)
    {
        var component = GetComponent();
        if (component == null)
        {
            base.LayoutData = layoutData;
        }
        else
        {
            component.SetLayoutData(layoutData);
        }
        return this;
    }

    public override TerminalPosition? ToBasePane(TerminalPosition position)
    {
        var terminalPosition = base.ToBasePane(position);
        if (terminalPosition == null)
        {
            return null;
        }
        return terminalPosition.Value.WithRelative(GetWrappedComponentTopLeftOffset());
    }

    public override TerminalPosition? ToGlobal(TerminalPosition position)
    {
        var terminalPosition = base.ToGlobal(position);
        if (terminalPosition == null)
        {
            return null;
        }
        return terminalPosition.Value.WithRelative(GetWrappedComponentTopLeftOffset());
    }

    private TerminalPosition GetWrappedComponentTopLeftOffset()
    {
        return Renderer.GetWrappedComponentTopLeftOffset();
    }

    private TerminalSize GetWrappedComponentSize(TerminalSize borderSize)
    {
        return Renderer.GetWrappedComponentSize(borderSize);
    }
}