using System.Windows.Forms;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        sealed class SplitterDragHandler : DragHandler
        {
            class SplitterOutline
            {
                public SplitterOutline()
                {
                    m_dragForm = new DragForm();
                    SetDragForm(Rectangle.Empty);
                    DragForm.BackColor = Color.Black;
                    DragForm.Opacity = 0.7;
                    DragForm.Show(false);
                }

                readonly DragForm m_dragForm;
                DragForm DragForm => m_dragForm;

                public void Show(Rectangle rect)
                {
                    SetDragForm(rect);
                }

                public void Close()
                {
                    DragForm.Close();
                }

                void SetDragForm(Rectangle rect)
                {
                    DragForm.Bounds = rect;
                    if (rect == Rectangle.Empty)
                        DragForm.Region = new Region(Rectangle.Empty);
                    else if (DragForm.Region != null)
                        DragForm.Region = null;
                }
            }

            public SplitterDragHandler(DockPanel dockPanel)
                : base(dockPanel)
            {
            }

            public new ISplitterDragSource DragSource
            {
                get => base.DragSource as ISplitterDragSource;
                private set => base.DragSource = value;
            }

            SplitterOutline m_outline;

            SplitterOutline Outline
            {
                get => m_outline;
                set => m_outline = value;
            }

            Rectangle m_rectSplitter;

            Rectangle RectSplitter
            {
                get => m_rectSplitter;
                set => m_rectSplitter = value;
            }

            public void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
            {
                DragSource = dragSource;
                RectSplitter = rectSplitter;

                if (!BeginDrag())
                {
                    DragSource = null;
                    return;
                }

                Outline = new SplitterOutline();
                Outline.Show(rectSplitter);
                DragSource.BeginDrag(rectSplitter);
            }

            protected override void OnDragging()
            {
                Outline.Show(GetSplitterOutlineBounds(Control.MousePosition));
            }

            protected override void OnEndDrag(bool abort)
            {
                DockPanel.SuspendLayout(true);

                Outline.Close();

                if (!abort)
                    DragSource.MoveSplitter(GetMovingOffset(Control.MousePosition));

                DragSource.EndDrag();
                DockPanel.ResumeLayout(true, true);
            }

            int GetMovingOffset(Point ptMouse)
            {
                Rectangle rect = GetSplitterOutlineBounds(ptMouse);
                if (DragSource.IsVertical)
                    return rect.X - RectSplitter.X;
                return rect.Y - RectSplitter.Y;
            }

            Rectangle GetSplitterOutlineBounds(Point ptMouse)
            {
                Rectangle rectLimit = DragSource.DragLimitBounds;

                Rectangle rect = RectSplitter;
                if (rectLimit.Width <= 0 || rectLimit.Height <= 0)
                    return rect;

                if (DragSource.IsVertical)
                {
                    rect.X += ptMouse.X - StartMousePosition.X;
                    rect.Height = rectLimit.Height;
                }
                else
                {
                    rect.Y += ptMouse.Y - StartMousePosition.Y;
                    rect.Width = rectLimit.Width;
                }

                if (rect.Left < rectLimit.Left)
                    rect.X = rectLimit.X;
                if (rect.Top < rectLimit.Top)
                    rect.Y = rectLimit.Y;
                if (rect.Right > rectLimit.Right)
                    rect.X -= rect.Right - rectLimit.Right;
                if (rect.Bottom > rectLimit.Bottom)
                    rect.Y -= rect.Bottom - rectLimit.Bottom;

                return rect;
            }
        }

        SplitterDragHandler m_splitterDragHandler = null;

        SplitterDragHandler GetSplitterDragHandler()
        {
            if (m_splitterDragHandler is null)
                m_splitterDragHandler = new SplitterDragHandler(this);
            return m_splitterDragHandler;
        }

        internal void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
        {
            GetSplitterDragHandler().BeginDrag(dragSource, rectSplitter);
        }
    }
}
