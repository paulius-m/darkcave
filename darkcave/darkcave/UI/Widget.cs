using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave.UI
{
    interface IWidget : Instanced
    {

    }

    class Widget : IWidget
    {
        Vector3 Postion;
        Vector3 Size;
        Vector3 Texture;

        public void GetInstanceData(RenderGroup instancer)
        {
            throw new NotImplementedException();
        }
    }



    class WidgetCollection : IWidget
    {
        private List<IWidget> list = new List<IWidget>();
        
        public void GetInstanceData(RenderGroup instancer)
        {
            throw new NotImplementedException();
        }
    }

    public static class WidgetFactory
    {
    }

}
