using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez;

namespace CardGame.Components
{
    public class StackComponent : Component
    {
        public int StackID;                 //0 - 99 
        public string CName = "Stack of Cards";
        public Vector2 Location;
        public int InitDealCnt = 0;         //initial num of cards dealt
        public int MaxDealCnt = 0;          //Max num of cards dealt
        //
        // id the card stack eg. Dealer, Player, Discard, etc
        //
        public float xOffset = 0.35f;       //disp offset on x-axis
        public float yOffset;               //disp offset on y-axis
        public float xPan;                  //disp panning on x-axis
        public float yPan;                  //disp panning on y-axis
        public bool FaceUp = true;         //are cards face up for this stack?
        //
        // Tracking a Stack of cards 
        //
        public List<Entity> CardsInStack;
        public List<int> BlockedStacks;     // Other stacks this one is blocking, if empty then none.
        //
        // are cards in this stack fanned out?
        //
        public int FannedDirection = 0;     // 0=stack on top eachother, 1=right, 2=left, 3=up, 4=down
        public float FannedOffset = 0.35f;  // how far to separate the cards from eachother
        //
        // entity moving on its own
        //
        public bool IsMoving;

        public StackComponent()
        {
            CardsInStack = new List<Entity>();
            BlockedStacks = new List<int>();
        }
        public Entity GetLastCard()
        {
            if (CardsInStack.Count <= 0)
                return null;

            return CardsInStack.LastOrDefault();
        }
        public Entity GetFirstCard()
        {
            if (CardsInStack.Count <= 0)
                return null;

            return CardsInStack[0];
        }
        public Entity GetCard(int _cardSeq)
        {
            if (_cardSeq > CardsInStack.Count - 1)
                return null;

            return CardsInStack[_cardSeq];
        }
        public int GetCardFaceImageValue(int _cardSeq)
        {
            Entity card = GetCard(_cardSeq);
            if (card == null)
                return 0;
            CardComponent ccomp = card.GetComponent<CardComponent>();
            return ccomp.FaceImage;
        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
        }
    }
}
