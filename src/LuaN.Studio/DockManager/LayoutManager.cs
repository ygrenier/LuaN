using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

namespace LuaN.Studio.DockManager
{
    public class LayoutManager : ILayoutUpdateStrategy
    {
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            var tool = anchorableToShow.Content as ViewModels.ToolViewModel;
            if (tool != null)
            {
                AnchorSide pSide = AnchorSide.Right;
                if (tool is ViewModels.IInteractiveLuaToolViewModel)
                {
                    pSide = AnchorSide.Bottom;
                    anchorableToShow.AutoHideMinHeight = 320;
                }
                LayoutAnchorSide aSide = null;
                switch (pSide)
                {
                    case AnchorSide.Left:
                        aSide = layout.LeftSide;
                        break;
                    case AnchorSide.Top:
                        aSide = layout.TopSide;
                        break;
                    case AnchorSide.Bottom:
                        aSide = layout.BottomSide;
                        break;
                    case AnchorSide.Right:
                    default:
                        aSide = layout.RightSide;
                        break;
                }
                var parentGroup = aSide.Children.FirstOrDefault();
                if (parentGroup == null)
                {
                    parentGroup = new LayoutAnchorGroup();
                    aSide.Children.Add(parentGroup);
                }
                parentGroup.Children.Add(anchorableToShow);
                return true;
            }
            return false;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
