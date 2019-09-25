using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace CardGame.Components
{
    public class CardComponent : Component
    {
        public StackComponent HoldingStack;                 //Stack component holding this card
        public string CName = "Card";
        //
        // Individual card in a deck
        //
        // The ranking depends on the card image that holds the faces of the cards
        // in our case images start with cardindex = 0 = 2 of hearts
        //
        public Sprite CardFace;
        public Sprite CardBack;

        public int Index;                   // 0 - 51 e.g. cardfaces[faceIndex];
        public int FaceImage = 0;           // 0 two,.. 8 ten, 9 jack, 10 queen, 11 king, 12 Ace
        public int Suit = 0;                // 0 heart, 1 dimond, 2 clubs, 3 spade
        public bool IsFaceUp = true;       // card face showing?
        public bool IsRed = true;           // could be used in game like Solitair
        public int Rank = 2;               // 2 two, 10 ten, 10 jack, 10 queen, 10 king, 11 Ace 
        public int RankExtra = 0;          // 1 Ace can also be ONE in blackjack

        public int CardStack = 0;               // Stack of cards this belongs (pointer)
        //
        // entity moving on its own
        //
        public bool IsMoving;

        public CardComponent()
        {

        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();


        }
    }
}
