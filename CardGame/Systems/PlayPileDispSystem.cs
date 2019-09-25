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
    public class PlayPileDispSystem : EntityProcessingSystem
    {
        //
        // Entities with StackComponent system to display what is on their location (Fanned out, in place, etc.)
        //
        Vector2 fanOutDistannce;
        public PlayPileDispSystem(Matcher matcher) : base(matcher)
        {
        }
        public override void Process(Entity entity)
        {
            //
            // entity = PlayStack
            //
            StackComponent sc = entity.GetComponent<StackComponent>();
            Entity lastCardonStack = sc.CardsInStack.LastOrDefault();

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
            int ind = 0;                            //cards number in stack

            for (int i=0; i < sc.CardsInStack.Count; i++)
            {
                Entity cardEntity = sc.CardsInStack[i];
                cardEntity.Enabled = true;
                cardEntity.Transform.Position = entity.Transform.Position + fanOutDistannce * new Vector2(ind, ind);
                //
                // Get the sprite (face/back)
                //
                var cardComp = cardEntity.GetComponent<CardComponent>();          //cardcomponent has the data
                var renderComp = cardEntity.GetComponent<SpriteRenderer>();               //sprite renderer of the card
                //
                // -1 is first to display and -9 is last layer to display
                //
                renderComp.RenderLayer = ind * -1;
                //if ((entity.Tag < 8) && (i == sc.CardsInStack.Count - 1))
                //    cardComp.IsFaceUp = true;

                if (cardComp.IsFaceUp)
                {
                    renderComp.Sprite = cardComp.CardFace;
                }
                else
                {
                    renderComp.Sprite = cardComp.CardBack;
                }
                ind += 1;
            }

        }
    }
}
