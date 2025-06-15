using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

public class Panel : AbstractComponent<Panel>, IContainer, IPanel
{
    private readonly List<IComponent> _components;
    private ILayoutManager _layoutManager;
    private TerminalSize? _cachedPreferredSize;
    private ITextColor? _fillColorOverride;

    public Panel() : this(new LinearLayout())
    {
    }

    public Panel(ILayoutManager? layoutManager)
    {
        if (layoutManager == null)
        {
            layoutManager = new AbsoluteLayout();
        }
        _components = new List<IComponent>();
        _layoutManager = layoutManager;
        _cachedPreferredSize = null;
    }

    public IPanel AddComponent(IComponent component)
    {
        return AddComponent(int.MaxValue, component);
    }

    public Panel AddComponent(int index, IComponent component)
    {
        if (component == null)
        {
            throw new ArgumentException("Cannot add null component");
        }
        
        lock (_components)
        {
            if (_components.Contains(component))
            {
                return this;
            }
            
            if (component.Parent != null)
            {
                component.Parent.RemoveComponent(component);
            }
            
            if (index > _components.Count)
            {
                index = _components.Count;
            }
            else if (index < 0)
            {
                index = 0;
            }
            
            _components.Insert(index, component);
        }
        
        component.OnAdded(this);
        Invalidate();
        return this;
    }

    public IPanel AddComponent(IComponent component, ILayoutData? layoutData)
    {
        if (component != null)
        {
            component.SetLayoutData(layoutData);
            AddComponent(component);
        }
        return this;
    }

    public bool ContainsComponent(IComponent component)
    {
        return component != null && component.HasParent(this);
    }

    public bool RemoveComponent(IComponent component)
    {
        if (component == null)
        {
            throw new ArgumentException("Cannot remove null component");
        }
        
        lock (_components)
        {
            int index = _components.IndexOf(component);
            if (index == -1)
            {
                return false;
            }
            
            if (BasePane?.FocusedInteractable == component)
            {
                BasePane.SetFocusedInteractable(null);
            }
            
            _components.RemoveAt(index);
        }
        
        component.OnRemoved(this);
        Invalidate();
        return true;
    }

    public Panel RemoveAllComponents()
    {
        lock (_components)
        {
            foreach (var component in _components.ToList())
            {
                RemoveComponent(component);
            }
        }
        return this;
    }

    public Panel SetLayoutManager(ILayoutManager? layoutManager)
    {
        lock (this)
        {
            if (layoutManager == null)
            {
                layoutManager = new AbsoluteLayout();
            }
            _layoutManager = layoutManager;
            Invalidate();
            return this;
        }
    }

    public ITextColor? FillColorOverride
    {
        get => _fillColorOverride;
        set => _fillColorOverride = value;
    }

    public ILayoutManager LayoutManager
    {
        get => _layoutManager;
        set => SetLayoutManager(value);
    }

    public int ChildCount
    {
        get
        {
            lock (_components)
            {
                return _components.Count;
            }
        }
    }

    public IReadOnlyCollection<IComponent> Children => ChildrenList;

    public IReadOnlyList<IComponent> ChildrenList
    {
        get
        {
            lock (_components)
            {
                return _components.ToList();
            }
        }
    }

    protected override IComponentRenderer<Panel> CreateDefaultRenderer()
    {
        return new DefaultPanelRenderer(this);
    }

    public class DefaultPanelRenderer : IComponentRenderer<Panel>
    {
        private readonly Panel _panel;
        private bool _fillAreaBeforeDrawingComponents = true;

        public DefaultPanelRenderer(Panel panel)
        {
            _panel = panel;
        }

        public void SetFillAreaBeforeDrawingComponents(bool fillAreaBeforeDrawingComponents)
        {
            _fillAreaBeforeDrawingComponents = fillAreaBeforeDrawingComponents;
        }

        public TerminalSize GetPreferredSize(Panel component)
        {
            lock (component._components)
            {
                component._cachedPreferredSize = component._layoutManager.GetPreferredSize(component._components);
            }
            return component._cachedPreferredSize.Value;
        }

        public void DrawComponent(ITextGUIGraphics graphics, Panel panel)
        {
            if (panel.IsInvalid)
            {
                panel.Layout(graphics.Size);
            }

            if (_fillAreaBeforeDrawingComponents)
            {
                graphics.ApplyThemeStyle(panel.ThemeDefinition.Normal);
                if (panel._fillColorOverride != null)
                {
                    graphics.SetBackgroundColor(panel._fillColorOverride);
                }
                graphics.Fill(' ');
            }

            lock (panel._components)
            {
                foreach (var child in panel._components)
                {
                    if (!child.Visible)
                    {
                        continue;
                    }
                    var componentGraphics = graphics.NewTextGraphics(child.Position, child.Size);
                    child.Draw(componentGraphics);
                }
            }
        }
    }

    protected override TerminalSize CalculatePreferredSize()
    {
        if (_cachedPreferredSize.HasValue && !IsInvalid)
        {
            return _cachedPreferredSize.Value;
        }
        return base.CalculatePreferredSize();
    }

    public override bool IsInvalid
    {
        get
        {
            lock (_components)
            {
                foreach (var component in _components)
                {
                    if (component.Visible && component.IsInvalid)
                    {
                        return true;
                    }
                }
            }
            return base.IsInvalid || _layoutManager.HasChanged;
        }
    }

    public IInteractable? NextFocus(IInteractable? fromThis)
    {
        bool chooseNextAvailable = (fromThis == null);

        lock (_components)
        {
            foreach (var component in _components)
            {
                if (!component.Visible)
                {
                    continue;
                }
                
                if (chooseNextAvailable)
                {
                    if (component is IInteractable interactable && interactable.Enabled && interactable.IsFocusable)
                    {
                        return interactable;
                    }
                    else if (component is IContainer container)
                    {
                        var firstInteractable = container.NextFocus(null);
                        if (firstInteractable != null)
                        {
                            return firstInteractable;
                        }
                    }
                    continue;
                }

                if (component == fromThis)
                {
                    chooseNextAvailable = true;
                    continue;
                }

                if (component is IContainer containerComp)
                {
                    if (fromThis != null && fromThis.IsInside(containerComp))
                    {
                        var next = containerComp.NextFocus(fromThis);
                        if (next == null)
                        {
                            chooseNextAvailable = true;
                        }
                        else
                        {
                            return next;
                        }
                    }
                }
            }
            return null;
        }
    }

    public IInteractable? PreviousFocus(IInteractable? fromThis)
    {
        bool chooseNextAvailable = (fromThis == null);

        List<IComponent> reversedComponentList;
        lock (_components)
        {
            reversedComponentList = new List<IComponent>(_components);
        }
        reversedComponentList.Reverse();

        foreach (var component in reversedComponentList)
        {
            if (!component.Visible)
            {
                continue;
            }
            
            if (chooseNextAvailable)
            {
                if (component is IInteractable interactable && interactable.Enabled && interactable.IsFocusable)
                {
                    return interactable;
                }
                if (component is IContainer container)
                {
                    var lastInteractable = container.PreviousFocus(null);
                    if (lastInteractable != null)
                    {
                        return lastInteractable;
                    }
                }
                continue;
            }

            if (component == fromThis)
            {
                chooseNextAvailable = true;
                continue;
            }

            if (component is IContainer containerComp)
            {
                if (fromThis != null && fromThis.IsInside(containerComp))
                {
                    var next = containerComp.PreviousFocus(fromThis);
                    if (next == null)
                    {
                        chooseNextAvailable = true;
                    }
                    else
                    {
                        return next;
                    }
                }
            }
        }
        return null;
    }

    public bool HandleInput(KeyStroke key)
    {
        return false;
    }

    public void UpdateLookupMap(IInteractableLookupMap interactableLookupMap)
    {
        lock (_components)
        {
            foreach (var component in _components)
            {
                if (!component.Visible)
                {
                    continue;
                }
                
                if (component is IContainer container)
                {
                    container.UpdateLookupMap(interactableLookupMap);
                }
                else if (component is IInteractable interactable && interactable.Enabled && interactable.IsFocusable)
                {
                    var globalPos = component.ToGlobal(TerminalPosition.TopLeftCorner);
                    if (globalPos.HasValue)
                    {
                        interactableLookupMap.Add(interactable, globalPos.Value);
                    }
                }
            }
        }
    }

    public override void Invalidate()
    {
        base.Invalidate();

        lock (_components)
        {
            foreach (var component in _components)
            {
                component.Invalidate();
            }
        }
    }

    private void Layout(TerminalSize size)
    {
        lock (_components)
        {
            _layoutManager.DoLayout(size, _components);
        }
    }
}