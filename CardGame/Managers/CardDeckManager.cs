using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using CardGame.Components;
/*
 * This static manager deals with playing cards.  It handles one deck of card
 * Tracking its faces, values, colors and suits.
 * It uses "CardPrefab" in Resources folder
 * It uses "CardDeck_72X100" card image in Images folder (that is broken up into piecs)
 * 
 */
namespace CardGame
{
    public class CardDeckManager
    {
        static object syncRoot = new System.Object();       //object for locking
        static CardDeckManager _Instance;

        //
        // One deck of cards
        //
        // deckTotal gives the bounds of cardsInDeck
        // currentDeckNumber is zero based, so 0 is deck one
        // currentCardNumber is zero based, so 0 is 2 of hearts (in our image)
        //
        //private List<StacksItems> cardStacks;         // card stack definitions
        static CardComponent[] cardDeck;                // card objects
        static int[] cardDeckPointer;                   // shuffeled card numbers for cardDeck
        static int deckTotal = 1;                       // total number of decks
        static Sprite _deckLocation;                    // object in the game that is location of deck of cards

        static int currentDeckNumber = 0;               // current deck of cards (if only one then value 0)
        static int currentCardNumber = 0;               // current card Number  in the deck 
        static int currentCardBack = 6;                 // current back of a card
        static float deckFanOut = 0.03f;                // value added to fan out the deck 

        public static bool endOfGame = false;                  //game stops
        public static int score = 0;                           //game score
        public static Scene scene;                      //scene
        public static int cardWidth = 72;            //width of each card
        public static int cardHeight = 100;        //height of top showing (when fanned out)
        //
        //znznznznznznznznznznznznznznzn
        // sprite image of each card
        //znznznznznznznznznznznznznznzn
        //
        // Pre-load this array in Editor by dragging Asset\Images\CardDeck_72100
        // cards 0-51
        //
        static SpriteRenderer[] cardfaces;
        static SpriteRenderer[] cardBacks;
        //
        // Joker is card 56
        //
        static SpriteRenderer cardJokers;
        public static void InitAllCards(Texture2D _cardImage)
        {
            //
            // card image has 5 rows of 13 cards
            //
            List<Sprite> subtexture = Sprite.SpritesFromAtlas(_cardImage, 72, 100);

            cardfaces = new SpriteRenderer[52];
            for (int i = 0; i < 52; i++)
            {
                cardfaces[i] = new SpriteRenderer(subtexture[i]);
            }
            //
            // back of cards 52-63 
            //
            cardBacks = new SpriteRenderer[12];
            for (int i = 0; i < 12; i++)
            {
                cardBacks[i] = new SpriteRenderer(subtexture[i+52]);
            }
            //
            // Joker is card 64
            //
            cardJokers = new SpriteRenderer();
            cardJokers = new SpriteRenderer(subtexture[64]);

            currentCardBack = 6;
            currentCardNumber = 0;
            currentDeckNumber = 0;
            

        }
        public static void CreateDeckOfCards(Scene _scene)
        {
            scene = _scene;
            cardDeckPointer = new int[52];
            cardDeck = new CardComponent[52];
            //
            // Create 52 CardComponents
            //
            for (int i = 0; i < 52 ; i++)
            {
                CardComponent card = new CardComponent();
                //
                // get face and back images
                //
                card.CardFace = cardfaces[i].Sprite;
                card.CardBack = cardBacks[currentCardBack].Sprite;

                card.Index = i;
                card.IsFaceUp = true;
                //
                // 0 heart, 1 dimond, 2 clubs, 3 spade
                //
                string tempSuit = "";
                switch (i)
                {
                    case int n when (n <= 12):
                        card.IsRed = true;                //hearts
                        card.Suit = 0;
                        tempSuit = "_Heart";
                        break;
                    case int n when (n >= 13 && n <= 25):
                        card.IsRed = true;                //diamond
                        card.Suit = 1;
                        tempSuit = "_Diamond";
                        break;
                    case int n when (n >= 26 && n <= 38):
                        card.IsRed = false;                //clubs
                        card.Suit = 2;
                        tempSuit = "_Club";
                        break;
                    case int n when (n >= 39):
                        card.IsRed = false;                //spades
                        card.Suit = 3;
                        tempSuit = "_Spade";
                        break;
                }
                //
                // This ranking depends on the card image that holds the face of the cards
                // 0 two, 1 three, 2 four, 3 five,... 8 ten, 9 jack, 10 queen, 11 king, 12 Ace
                //
                //
                card.FaceImage = i % 13;
                //
                // If this is a blackjack game the jack,queen,king = 10 points
                // all number cards are their values, except Ace to be 1 or 11
                //

                if (card.FaceImage <= 8)
                {
                    card.Rank = card.FaceImage + 2;
                    card.FaceImage = card.FaceImage + 2;
                }
                else if (card.FaceImage > 8 && card.FaceImage < 12)   //face cards
                {
                    card.Rank = 10;
                    card.FaceImage = card.FaceImage + 2;
                }
                else
                {
                    card.FaceImage = 1;              //Ace
                    card.Rank = 1;                   //Ace is one
                    card.RankExtra = 11;             //Ace is also 11
                }
                card.CName = "C" + card.FaceImage.ToString("00") + tempSuit;
                currentCardNumber = 0;
                cardDeckPointer[i] = i;

                cardDeck[i]=card;
            }

            Shuffle();
        }
        public static void Shuffle()
        {
            //
            // cardDeckPoint is shuffled and first card number = 0
            //
            int count = 51;
            for (int j = count; j > 1; j--)
            {
                int temp = cardDeckPointer[j];
                int Number = Nez.Random.NextInt(j + 1);

                cardDeckPointer[j] = cardDeckPointer[Number];
                cardDeckPointer[Number] = temp;
            }

            currentCardNumber = 0;
        }
        public static int GetACard()
        {
            if (currentCardNumber > 51)
                return -1;

            //
            // 5/16/2017 there is only ONE deck of cards
            //
            int cardPTR = cardDeckPointer[currentCardNumber];

            if (currentCardNumber > 51)
                cardPTR =  -1;

            currentCardNumber += 1;
            return cardPTR;
        }
        public static Entity DealACard(bool _faceup = true)
        {
            //
            // There are no colliders on cards, Stack of Cards have colliders
            //
            int cardnum = GetACard();
            if (cardnum < 0)
                return null;

            CardComponent cc = GetCardComponent(cardnum);
            cc.IsFaceUp = _faceup;

            Entity _card = scene.CreateEntity(cc.CName);
            //
            // The card we deal has no colliders
            //
            _card.Enabled = false;
            SpriteRenderer cface = new SpriteRenderer();

            cface.Sprite = cc.CardFace;
            _card.AddComponent(cface);
            //_card.AddComponent(new BoxCollider(new Rectangle(-36,-50,72,25)));      //collider is on Top of card only
            //_card.AddComponent(new BoxCollider(72f,100f));      //collider is on Top of card only
            _card.AddComponent(cc);
            _card.Tag = -1;                          //tag to identify this entity as a card
            return _card;
        }
        public static CardComponent GetCardComponent(int cardPTR)
        {
            //
            // The CardComponent of the card is returned
            //
            CardComponent cardObj = cardDeck[cardPTR];
            return cardObj;
        }
        public static SpriteRenderer GetCardFace(int cardPTR)
        {
            //
            // The face image of the card is returned
            //
            SpriteRenderer cardObj = cardfaces[cardPTR];
            return cardObj;
        }
        public static SpriteRenderer GetCardBack()
        {
            //
            // The back image of the card is returned
            //
            SpriteRenderer cardObj = cardBacks[currentCardBack];
            return cardObj;
        }
        //public static Sprite CardBack()
        //{
        //    Sprite st = GetCardBack();
        //    //Sprite cback = new SpriteRenderer(st);
        //    return st;
        //}
    }
}
