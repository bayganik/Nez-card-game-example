using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace CardGame.Components
{
    public class MouseComponent : Component
    {
        public float Speed = 50f;
        //
        // entity moving on its own
        //
        public bool IsMoving;

        public MouseComponent()
        {

        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();


        }
    }
}
