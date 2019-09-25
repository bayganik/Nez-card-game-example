using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace CardGame.Components
{
    public class PileDisplayedComponent : Component
    {
        //
        // Empty component meant to tag 4 Ace Piles so they can have their own systems
        //
        public PileDisplayedComponent()
        {

        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();


        }
    }
}
