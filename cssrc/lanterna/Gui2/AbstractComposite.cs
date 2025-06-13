using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// This abstract implementation contains common code for the different Composite implementations. A
/// Composite component is one that encapsulates a single component, like borders. Because of this, a
/// Composite can be seen as a special case of a Container and indeed this abstract class does in fact
/// implement the Container interface as well, to make the composites easier to work with internally.
/// </summary>
/// <typeparam name="T">Should always be itself, see AbstractComponent</typeparam>
public abstract class AbstractComposite<T> : AbstractComponent<T>, IComposite, IContainer 
    where T : class, IContainer
{
    private IComponent? _component;

    /// <summary>
    /// Default constructor
    /// </summary>
    protected AbstractComposite()
    {
        _component = null;
    }

    public virtual void SetComponent(IComponent? component)
    {
        var oldComponent = _component;
        if (oldComponent == component)
        {
            return;
        }
        
        if (oldComponent != null)
        {
            RemoveComponent(oldComponent);
        }
        
        if (component != null)
        {
            _component = component;
            component.OnAdded(this);
            
            var basePane = BasePane;
            if (basePane != null)
            {
                var menuBar = basePane.MenuBar;
                if (menuBar == null)
                {
                    component.Position = TerminalPosition.TopLeftCorner;
                }
                else
                {
                    component.Position = TerminalPosition.TopLeftCorner.WithRelativeRow(1);
                }
            }
            Invalidate();
        }
    }

    public virtual IComponent? GetComponent()
    {
        return _component;
    }

    public IComponent? Component
    {
        get => GetComponent();
        set => SetComponent(value);
    }

    public int ChildCount => _component != null ? 1 : 0;

    public IReadOnlyList<IComponent> GetChildrenList()
    {
        if (_component != null)
        {
            return new List<IComponent> { _component };
        }
        else
        {
            return new List<IComponent>();
        }
    }

    public IReadOnlyList<IComponent> ChildrenList => GetChildrenList();

    public IReadOnlyCollection<IComponent> Children => GetChildrenList();

    public IEnumerable<IComponent> GetChildren()
    {
        return GetChildrenList();
    }

    public bool ContainsComponent(IComponent component)
    {
        return component != null && component.HasParent(this);
    }

    public bool RemoveComponent(IComponent component)
    {
        if (_component == component)
        {
            _component = null;
            component.OnRemoved(this);
            Invalidate();
            return true;
        }
        return false;
    }

    public override bool IsInvalid
    {
        get
        {
            return base.IsInvalid || (_component != null && _component.IsInvalid);
        }
    }

    public override void Invalidate()
    {
        base.Invalidate();

        // Propagate
        _component?.Invalidate();
    }

    public virtual IInteractable? NextFocus(IInteractable? fromThis)
    {
        if (fromThis == null && GetComponent() is IInteractable interactable)
        {
            if (interactable.Enabled)
            {
                return interactable;
            }
        }
        else if (GetComponent() is IContainer container)
        {
            return container.NextFocus(fromThis);
        }
        return null;
    }

    public virtual IInteractable? PreviousFocus(IInteractable? fromThis)
    {
        if (fromThis == null && GetComponent() is IInteractable interactable)
        {
            if (interactable.Enabled)
            {
                return interactable;
            }
        }
        else if (GetComponent() is IContainer container)
        {
            return container.PreviousFocus(fromThis);
        }
        return null;
    }

    public virtual bool HandleInput(Lanterna.Input.KeyStroke keyStroke)
    {
        // For now, return false - can be implemented later
        return false;
    }

    public virtual void UpdateLookupMap(IInteractableLookupMap interactableLookupMap)
    {
        if (GetComponent() is IContainer container)
        {
            container.UpdateLookupMap(interactableLookupMap);
        }
        else if (GetComponent() is IInteractable interactable)
        {
            interactableLookupMap.Add(interactable, interactable.GlobalPosition);
        }
    }

    public virtual IInteractable? FocusedInteractable => null; // Simplified for now

    public virtual void SetFocusedInteractable(IInteractable? interactable)
    {
        // Simplified for now
    }

    public ILayoutManager? LayoutManager
    {
        get => null; // Composites don't use layout managers
        set { } // Do nothing
    }

    public void AddComponent(IComponent component)
    {
        SetComponent(component);
    }

    public void AddComponent(IComponent component, ILayoutData? layoutData)
    {
        // Note: LayoutData assignment would need to be on a settable property
        SetComponent(component);
    }

    public void RemoveAllComponents()
    {
        SetComponent(null);
    }
}