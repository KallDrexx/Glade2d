using System.Collections.ObjectModel;
using Glade2d.Graphics;
using Gum;
using RenderingLibrary;
using RenderingLibrary.Graphics;

namespace Glade2d.GumTest;

public class CustomSpriteRenderable : IVisible, IRenderableIpso, ISetClipsChildren
{
    private float _x, _y;
    
    public Sprite Sprite { get; }
    
    public bool AbsoluteVisible
    {
        get
        {
            if (((IVisible)this).Parent == null)
            {
                return Visible;
            }
            else
            {
                return Visible && ((IVisible)this).Parent.AbsoluteVisible;
            }
        }
    }

    public BlendState BlendState => BlendState.NonPremultiplied;

    ObservableCollection<IRenderableIpso> children = new ObservableCollection<IRenderableIpso>();
    public ObservableCollection<IRenderableIpso> Children => children;

    ColorOperation IRenderableIpso.ColorOperation => ColorOperation.Modulate;

    // If a GUE uses this, it needs to support storing the values.
    public bool ClipsChildren { get; set; }

    float height;

    public float Height
    {
        get { return height; }
        set
        {
#if DEBUG
            if (float.IsPositiveInfinity(value))
            {
                throw new ArgumentException();
            }
#endif
            height = value;
        }
    }

    public string Name { get; set; }

    IRenderableIpso mParent;

    public IRenderableIpso Parent
    {
        get { return mParent; }
        set
        {
            if (mParent != value)
            {
                if (mParent != null)
                {
                    mParent.Children.Remove(this);
                }

                mParent = value;
                if (mParent != null)
                {
                    mParent.Children.Add(this);
                }
            }
        }
    }

    public float Rotation { get; set; }

    public object Tag { get; set; }

    public bool Visible { get; set; } = true;

    public float Width { get; set; }

    public bool Wrap => false;

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            Sprite.X = this.GetAbsoluteX();
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            Sprite.Y = this.GetAbsoluteY();
        }
    }

    public float Z { get; set; }

    public bool FlipHorizontal { get; set; }
    
    public CustomSpriteRenderable(Sprite sprite)
    {
        Sprite = sprite;
        Width = sprite.Width;
        Height = sprite.Height;
    }

    IVisible IVisible.Parent
    {
        get { return Parent as IVisible; }
    }

    public void PreRender()
    {
    }

    public void Render(ISystemManagers managers)
    {
    }


    void IRenderableIpso.SetParentDirect(IRenderableIpso parent)
    {
        mParent = parent;
    }

    public override string ToString()
    {
        return Name;
    }
}