using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using CardGame.Components;
using CardGame.Scenes;


namespace CardGame.Systems
{
    public class StackDragDispSystem : EntityProcessingSystem
    {
        MouseState CurrentMouse;
        MouseState PrevMouse;
        //
        // Entities with DragComponent system to display where the mouse is
        //
        Vector2 fanOutDistannce;
        public StackDragDispSystem(Matcher matcher) : base(matcher)
        {
        }
        public override void process(Entity entity)
        {
            //
            // We have a DragDisp entity (holds all cards entities)
            //
            StackComponent sc = entity.getComponent<StackComponent>();
            if (sc != null)
            {
                if (sc.CardsInStack.Count <= 0)
                    return;                         //no cards to drag
            }

            var _mouseCollider = entity.getComponent<BoxCollider>();
            PrevMouse = CurrentMouse;
            CurrentMouse = Mouse.GetState();
            //
            // Current location of the mouse used for the hand icon
            //
            entity.transform.position = scene.camera.screenToWorldPoint(new Vector2(CurrentMouse.Position.X, CurrentMouse.Position.Y));

            Entity lastCardonStack = sc.CardsInStack.LastOrDefault();
            //
            // Display of stack by fan out direction
            //
            switch (sc.FannedDirection)
            {
                case 0:
                    fanOutDistannce = Vector2.Zero;
                    break;
                case 1:
                    fanOutDistannce = new Vector2(30f, 0);
                    break;
                case 2:
                    fanOutDistannce = new Vector2(-30f, 0);
                    break;
                case 3:
                    fanOutDistannce = new Vector2(0, -30f);
                    break;
                case 4:
                    fanOutDistannce = new Vector2(0, 30f);
                    break;

            }
            //
            // All cards are Entities in this stack
            //
            int ind = 0;                            //cars number in stack
            int multiplier = -1;
            for (int i=0; i < sc.CardsInStack.Count; i++)
            {
                Entity cardEntity = sc.CardsInStack[i];
                cardEntity.enabled = true;
                cardEntity.transform.position = entity.transform.position + fanOutDistannce * ind ;
                //
                // Get the sprite (face/back)
                //
                var cardComp = cardEntity.getComponent<CardComponent>();          //cardcomponent has the data
                var renderComp = cardEntity.getComponent<Sprite>();               //sprite renderer of the card
                //
                // -100 because we want to render dragging cards on top of all ohters
                //
                renderComp.renderLayer = (ind * -1) - 1000 ;
                renderComp.subtexture = cardComp.CardFace;          //we only drag face up cards

                ind += 1;
            }
 
        }
    }
}
