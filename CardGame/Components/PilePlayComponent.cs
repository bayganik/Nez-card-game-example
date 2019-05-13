using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace CardGame.Components
{
    public class PilePlayComponent : Component
    {
        //
        // Empty component meant to tag 7 Play Piles so they can have their own systems
        //
        public Entity EntityOrig;                   //Stack Entity cards came from

        public PilePlayComponent()
        {

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();


        }
    }
}
