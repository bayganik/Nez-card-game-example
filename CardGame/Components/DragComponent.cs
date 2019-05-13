using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace CardGame.Components
{
    public class DragComponent : Component
    {
        public Entity EntityOrig;                   //Stack Entity cards came from

        public DragComponent()
        {

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();


        }
    }
}
