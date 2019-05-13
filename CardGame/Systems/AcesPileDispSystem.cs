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
    public class AcesPileDispSystem : EntityProcessingSystem
    {
        //
        // Entities with StackComponent system to display what is on their location (Fanned out, in place, etc.)
        //
        Vector2 fanOutDistannce;
        public AcesPileDispSystem(Matcher matcher) : base(matcher)
        {
        }
        public override void process(Entity entity)
        {
            //
            // Aces STACK entities come here 
            //
            StackComponent sc = entity.getComponent<StackComponent>();
            if (sc.StackID == 12)
            {
                string j = sc.CName;
            }
            Entity lastCardonStack = sc.CardsInStack.LastOrDefault();
            fanOutDistannce = Vector2.Zero;
            //
            // All cards are Entities in this stack
            //
            int ind = 0;                            //cars number in stack

            for (int i=0; i < sc.CardsInStack.Count; i++)
            {
                Entity cardEntity = sc.CardsInStack[i];
                cardEntity.enabled = true;
                cardEntity.transform.position = entity.transform.position + fanOutDistannce * ind;
                //
                // Get the sprite faces 
                //
                var cardComp = cardEntity.getComponent<CardComponent>();          //cardcomponent has the data
                var renderComp = cardEntity.getComponent<Sprite>();               //sprite renderer of the card
                //
                // -1 is first to display and -9 is last layer to display
                //
                renderComp.renderLayer = ind * -1;
                renderComp.subtexture = cardComp.CardFace;

                ind += 1;
            }

        }
    }
}
