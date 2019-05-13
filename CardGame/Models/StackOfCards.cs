using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Nez.UI;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;

using CardGame.Components;
using CardGame.Systems;
using Microsoft.Xna.Framework;

namespace CardGame.Models
{
    public class StackOfCards
    {
        public int Tag { get; set; }                            //tag value of stack entity
        public StackComponent StackComp { get; set; }
        public Entity LastCardonStack { get; set; }             //last card in stack
        public List<Entity> CardsInStack { get; set; }          //bring cards up one level
        public int FannedDirection { get; set; }                //fanning direction
        public Vector2 FanOutDistannce { get; set; }            //distance of cards from each other
        public int TotalCards { get; set; }
        public StackOfCards(Entity _cardStack)
        {
            //
            // Stack entity holding cards
            //
            Tag = _cardStack.tag;
            StackComp = new StackComponent();
            LastCardonStack = new Entity();

            StackComp = _cardStack.getComponent<StackComponent>();
            if (StackComp == null)
                return;

            if (StackComp.CardsInStack.Count == 0)
                return;
            else
                TotalCards = StackComp.CardsInStack.Count;

            LastCardonStack = StackComp.CardsInStack.LastOrDefault();
            CardsInStack = StackComp.CardsInStack;
            FannedDirection = StackComp.FannedDirection;
            switch (FannedDirection)
            {
                case 0:
                    FanOutDistannce = Vector2.Zero;
                    break;
                case 1:
                    FanOutDistannce = new Vector2(30f, 0);
                    break;
                case 2:
                    FanOutDistannce = new Vector2(-30f, 0);
                    break;
                case 3:
                    FanOutDistannce = new Vector2(0, -30f);
                    break;
                case 4:
                    FanOutDistannce = new Vector2(0, 30f);
                    break;
            }
        }
        public Entity GetCard(int _no)
        {
            if ((StackComp.CardsInStack.Count < _no) || (_no > StackComp.CardsInStack.Count))
                return null;

            return StackComp.CardsInStack[_no];
        }
    }
}
