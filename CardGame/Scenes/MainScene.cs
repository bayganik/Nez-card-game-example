using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;
//using CardGame.Entities;
using CardGame.Components;
using CardGame.Systems;
using CardGame.Models;

namespace CardGame.Scenes
{
    public class MainScene : BaseScene
    {
        /*
         * Example card game scene.  This is a solitaire game.  It teaches:
         *      Creation of entities on screen
         *      Assign entity "Tag" a value to distiguish them
         *      Creation of systems for particular entities (using components)
         *      Give each entity multiple images (subTexture)
         *      Adding/removing components on-fly
         *      Creation of buttons and text on screen (using UI canvas)
         *      Pressing buttons
         * Card Game
         *      Creation of cards from a single texture
         *      shuffle
         *      deal
         *      creation of different piles(stacks) of cards
         *      dragging while displaying cards
         *      dropping cards on new stack or returning them to their original stack
        */
        //Sprite<int> eachCard;
        SceneResolutionPolicy policy;
        int cardBackNum = 6;
        Vector2 drawStack = new Vector2(90, 100);
        Vector2 aceStack1 = new Vector2(390, 100);
        Vector2 drawStackDisp = new Vector2(190, 100);

        Entity DealDeck;
        Entity DrawDisp;
        Entity DragDisp;
        Entity TextEntity;
        Entity ScoreEntity;

        UICanvas UIC;
        List<Entity> PlayStacks = new List<Entity>();
        List<Entity> AceStacks = new List<Entity>();
        //
        // play stack
        //
        Vector2 playStack1 = new Vector2(90, 250);
        float colliderHeight = 500f;
        float colliderWidth = 72f;
        float colliderX = -36f;
        float colliderY = -50f;
        NezSpriteFont font;
        public ImageButton PlayButton { get; set; }
        public TextButton ExitButton { get; set; }
        public TextButton NewButton { get; set; }
        public Label Msg { get; set; }
        /*
         * Stack entity tag of 80 can only have top displayed card dragged
         * Stack entity tag of 70 can only be clicked on (not dragged)
         */
        public MainScene()
        {
            policy = Scene.SceneResolutionPolicy.ExactFit;
        }
        public override void Initialize()
        {
            base.Initialize();
            font = new NezSpriteFont(Content.Load<SpriteFont>("Arial"));

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // put a Canvas entity on upper right hand side for UI
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            Entity uiCanvas = CreateEntity("ui-canvas");
            UIC = uiCanvas.AddComponent(new UICanvas());

            ExitButton = UIC.Stage.AddElement(new TextButton("End !", Skin.CreateDefaultSkin()));
            ExitButton.SetPosition(800f, 30f);
            ExitButton.SetSize(60f, 20f);
            ExitButton.OnClicked += ExitButton_OnClicked;

            NewButton = UIC.Stage.AddElement(new TextButton("Play !", Skin.CreateDefaultSkin()));
            NewButton.SetPosition(800f, 60f);
            NewButton.SetSize(60f, 20f);
            NewButton.OnClicked += NewButton_OnClicked;

            Msg = UIC.Stage.AddElement(new Label("Label Msg"));
            Msg.SetPosition(800f, 90f);
            Msg.SetSize(100f, 50f);

            ImageButtonStyle stl = new ImageButtonStyle();
            PlayButton = UIC.Stage.AddElement(new ImageButton(stl));
            PlayButton.SetPosition(800f, 120);

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Text entity with component (Game name label)
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            TextEntity = CreateEntity("txt");
            TextEntity.Transform.Position = new Vector2(350, 20);
            TextEntity.Transform.Scale = new Vector2(1, 1);
            var txt = new TextComponent(Graphics.Instance.BitmapFont, "Card Play Game", new Vector2(0, 0), Color.White);
            txt.SetFont(font);
            TextEntity.AddComponent(txt);

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Score Text entity 
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            ScoreEntity = CreateEntity("score");
            ScoreEntity.Transform.Position = new Vector2(10, 20);
            ScoreEntity.Transform.Scale = new Vector2(1, 1);
            txt = new Nez.TextComponent(Graphics.Instance.BitmapFont, "Score 0", new Vector2(0, 0), Color.White);
            txt.SetFont(font);
            ScoreEntity.AddComponent(txt);
            ScoreEntity.AddComponent(new EndGameComponent());               //end of game goes with score entity
            CardDeckManager.score = 0;
            CardDeckManager.endOfGame = false;  
            //
            // Create a deck of cards from image
            //
            var texture = Content.Load<Texture2D>("CardDeck_72x100");
            CardDeckManager.InitAllCards(texture);                      //pass the texture to manager
            CardDeckManager.CreateDeckOfCards(this);                    //pass the scene to manager
            
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // mouse entity has image that is a hand
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            var mouseEntity = CreateEntity("mouse");
            var mouseSprite = mouseEntity.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("hand1")));
            mouseSprite.RenderLayer = -99;
            mouseEntity.Transform.Scale = new Vector2(.30f, .30f);
            
            mouseEntity.Transform.Position = new Vector2(30, 30);
            mouseEntity.AddComponent(new BoxCollider());
            mouseEntity.AddComponent(new MouseComponent());
            
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Deal Deck (image to click on for next card)
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            DealDeck = CreateEntity("DealerStack", drawStack);
            DealDeck.Tag = 90;
            DealDeck.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            DealDeck.AddComponent(new BoxCollider());
            DealDeck.AddComponent(new StackComponent() { StackID = 0 });
            DealDeck.AddComponent(new PilePlayComponent());

            //znznznznznznznznznznznznznznznznznznznznznzn
            // 4 Ace stacks to collect cards
            //znznznznznznznznznznznznznznznznznznznznznzn
            var as1 = CreateEntity("AceStack1", aceStack1);
            as1.Tag = 80;               //special tag for Ace pile
            as1.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            as1.AddComponent(new BoxCollider());
            as1.AddComponent(new StackComponent() { StackID = 1, InitDealCnt = 0, CName = "AceStack" });
            as1.AddComponent(new PileDisplayedComponent());
            AceStacks.Add(as1);

            var as2 = CreateEntity("AceStack2", aceStack1 + new Vector2(100, 0));
            as2.Tag = 80;               //special tag for Ace pile
            as2.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            as2.AddComponent(new BoxCollider());
            as2.AddComponent(new StackComponent() { StackID = 2 });
            as2.AddComponent(new PileDisplayedComponent());
            AceStacks.Add(as2);

            var as3 = CreateEntity("AceStack3", aceStack1 + new Vector2(200, 0));
            as3.Tag = 80;               //special tag for Ace pile
            as3.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            as3.AddComponent(new BoxCollider());
            as3.AddComponent(new StackComponent() { StackID = 3 });
            as3.AddComponent(new PileDisplayedComponent());
            AceStacks.Add(as3);

            var as4 = CreateEntity("AceStack4", aceStack1 + new Vector2(300, 0));
            as4.Tag = 80;               //special tag for Ace pile
            as4.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            as4.AddComponent(new BoxCollider());
            as4.AddComponent(new StackComponent() { StackID = 4 });
            as4.AddComponent(new PileDisplayedComponent());
            AceStacks.Add(as4);
            
            //znznznznznznznznznznznznznznznznznznznznznzn
            // 7 playing stacks
            //znznznznznznznznznznznznznznznznznznznznznzn
            var pl1 = CreateEntity("PlayStack1", playStack1);
            pl1.Tag = 1;
            pl1.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl1.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl1.AddComponent(new StackComponent() { StackID = 5, FannedDirection = 4 });
            pl1.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl1);

            var pl2 = CreateEntity("PlayStack2", playStack1 + new Vector2(100, 0));
            pl2.Tag = 2;
            pl2.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl2.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl2.AddComponent(new StackComponent() { StackID = 6, FannedDirection = 4 });
            pl2.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl2);

            var pl3 = CreateEntity("PlayStack3", playStack1 + new Vector2(200, 0));
            pl3.Tag = 3;
            pl3.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl3.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl3.AddComponent(new StackComponent() { StackID = 7, FannedDirection = 4 });
            pl3.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl3);

            var pl4 = CreateEntity("PlayStack4", playStack1 + new Vector2(300, 0));
            pl4.Tag = 4;
            pl4.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl4.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl4.AddComponent(new StackComponent() { StackID = 8, FannedDirection = 4 });
            pl4.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl4);

            var pl5 = CreateEntity("PlayStack5", playStack1 + new Vector2(400, 0));
            pl5.Tag = 5;
            pl5.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl5.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl5.AddComponent(new StackComponent() { StackID = 9, FannedDirection = 4 });
            pl5.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl5);

            var pl6 = CreateEntity("PlayStack6", playStack1 + new Vector2(500, 0));
            pl6.Tag = 6;
            pl6.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl6.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl6.AddComponent(new StackComponent() { StackID = 10, FannedDirection = 4 });
            pl6.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl6);

            var pl7 = CreateEntity("PlayStack7", playStack1 + new Vector2(600, 0));
            pl7.Tag = 7;
            pl7.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            pl7.AddComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl7.AddComponent(new StackComponent() { StackID = 11, FannedDirection = 4 });
            pl7.AddComponent(new PilePlayComponent());
            PlayStacks.Add(pl7);
            
            //znznznznznznznznznznznznznznznznznznznznznzn
            // Draw stack (all cards are faceup)
            //znznznznznznznznznznznznznznznznznznznznznzn
            DrawDisp = CreateEntity("DrawDisp", drawStack + new Vector2(100, 0));
            DrawDisp.Tag = 80;
            DrawDisp.AddComponent(new SpriteRenderer(Content.Load<Texture2D>("EmptyHolder")));
            DrawDisp.AddComponent(new BoxCollider());
            DrawDisp.AddComponent(new StackComponent() { StackID = 0, CName = "DrawDisp", FannedDirection = 0 });
            DrawDisp.AddComponent(new PileDisplayedComponent());
            
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Drag stack (all cards are faceup and are being moved)
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            DragDisp = CreateEntity("DragDisp", new Vector2(0, 0));
            DragDisp.Tag = 85;
            
            DragDisp.AddComponent(new BoxCollider());
            DragDisp.AddComponent(new StackComponent() { StackID = 0, FannedDirection = 4 });

            Fill_All_Stacks();

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Systems to process our requests
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            //
            this.AddEntityProcessor(new MouseClickSystem(new Matcher().All(typeof(MouseComponent))));
            this.AddEntityProcessor(new PlayPileDispSystem(new Matcher().All(typeof(PilePlayComponent))));
            this.AddEntityProcessor(new AcesPileDispSystem(new Matcher().All(typeof(PileDisplayedComponent))));
            this.AddEntityProcessor(new StackDragDispSystem(new Matcher().All(typeof(DragComponent))));
            this.AddEntityProcessor(new EndOfGameSystem(new Matcher().All(typeof(EndGameComponent))));

        }
        public bool EndOfGameTest()
        {
            return false;

            //StackComponent scDeal = DealDeck.GetComponent<StackComponent>();
            //for (int i = 0; i < scDeal.CardsInStack.Count; i++)
            //{
            //    Entity card1 = scDeal.GetCard(i);
            //    if (card1 == null)
            //        break;
            //    var dragCard = card1.GetComponent<CardComponent>();
            //    //
            //    // test this card to put on PlayStack
            //    //
            //    for (int pi = 0; pi < PlayStacks.Count; pi++)
            //    {
            //        StackComponent sc = PlayStacks[pi].GetComponent<StackComponent>();
            //        Entity card2 = sc.GetLastCard();
            //        if (card2 == null)
            //            break;
            //        var dropCard = card2.GetComponent<CardComponent>();
            //        if (TestCardsForPlayStack(dragCard, dropCard))
            //            return false;               //card can be played, not end of game
            //    }

            //}
            //return true;
        }
        public void DispGameScore()
        {
            var txt = ScoreEntity.GetComponent<TextComponent>();
            txt.RenderLayer = -100;
            txt.SetText("Score " + CardDeckManager.score.ToString());

        }
        public void DispGameOver()
        {
            TextEntity.Transform.Position = new Vector2(350, 400);
            var txt = TextEntity.GetComponent<TextComponent>();
            txt.RenderLayer = -100;
            txt.SetText("GAME IS OVER !");
            txt.SetColor(Color.Black);
        }
        private void ExitButton_OnClicked(Button button)
        {
            //
            // Exit button is pressed
            //
            TextEntity.Transform.Position = new Vector2(350, 400);
            var txt = TextEntity.GetComponent<TextComponent>();
            txt.RenderLayer = -100;
            txt.SetText("GAME IS OVER !");
            txt.SetColor(Color.Black);

            CardDeckManager.endOfGame = true;
        }
        private void NewButton_OnClicked(Button button)
        {
            //CardDeckManager.CreateDeckOfCards(this);   //create a new deck and shuffle
            //Fill_All_Stacks();
            //
            // New button is pressed
            //
            var msg = UIC.Stage.GetElements();
            foreach(Element el in msg)
            {
                if ((el.GetType() == typeof(Label)))
                {
                    var lbl = (Label)el;
                    lbl.SetText("New Button Pushed");
                }
            }
            //
            // put back the main heading
            //
            TextEntity.Transform.Position = new Vector2(350, 20);
            var txt = TextEntity.GetComponent<TextComponent>();
            txt.RenderLayer = -100;
            txt.SetText("GAME is new again !");
            txt.SetColor(Color.Black);

            CardDeckManager.score = 0;
            CardDeckManager.endOfGame = false;
        }
        public void ReturnCardFromDrag2Stack()
        {
            DragComponent scDragComp = DragDisp.GetComponent<DragComponent>();
            if (scDragComp == null)
                return;

            Entity fromEntity = scDragComp.EntityOrig;
            StackComponent scDrag = DragDisp.GetComponent<StackComponent>();            //cards being dragged
            StackComponent scFrom = fromEntity.GetComponent<StackComponent>();            //cards to give back
            for (int i=0; i < scDrag.CardsInStack.Count; i++)
            {
                scFrom.CardsInStack.Add(scDrag.CardsInStack[i]);
            }
            DragStackClear();

        }
        public void DropCardFromDrag2AceStat(Entity _playStack)
        {
            StackComponent scDrag = DragDisp.GetComponent<StackComponent>();            //cards being dragged

            if (scDrag.CardsInStack.Count != 1)
            {
                ReturnCardFromDrag2Stack();
                return;
            }
            DragComponent scDragComp = DragDisp.GetComponent<DragComponent>();
            if (scDragComp.EntityOrig == _playStack)
            {
                ReturnCardFromDrag2Stack();
                return;
            }

            StackComponent scPlay = _playStack.GetComponent<StackComponent>();            //cards being dropped
            //
            // first card of drag needs to match last card of drop
            //
            Entity firstCardonStack = scDrag.CardsInStack[0];               //get first card of drag
            CardComponent firstCard = firstCardonStack.GetComponent<CardComponent>();
            //
            // Make sure this stack is not empty
            //
            if (scPlay.CardsInStack.Count == 0)
            {
                if (firstCard.FaceImage != 1)          //only an ACE will sit on empty stack
                {
                    ReturnCardFromDrag2Stack();
                    return;
                }
                //
                //  first card of drap is ACE, drop all of them
                //
                for (int i = 0; i < scDrag.CardsInStack.Count; i++)
                {
                    scPlay.CardsInStack.Add(scDrag.CardsInStack[i]);
                    CardDeckManager.score += scDrag.GetCardFaceImageValue(i);       //card added, calc score
                }
                DragStackClear();
                return;
            }
            //
            // play stack is NOT empty, test cards
            //
            Entity lastCardonStack = scPlay.CardsInStack.LastOrDefault();               //get last card
            CardComponent lastCard = lastCardonStack.GetComponent<CardComponent>();
            if (TestCardsForAceStack(firstCard, lastCard))
            {
                for (int i = 0; i < scDrag.CardsInStack.Count; i++)
                {
                    scPlay.CardsInStack.Add(scDrag.CardsInStack[i]);
                    CardDeckManager.score += scDrag.GetCardFaceImageValue(i);       //card added, calc score
                }
                DragStackClear();
            }
            else
            {
                ReturnCardFromDrag2Stack();
            }
        }
        public void DropCardFromDrag2PlayStack(Entity _playStack)
        {
            StackComponent scDrag = DragDisp.GetComponent<StackComponent>();            //cards being dragged
            StackComponent scPlay = _playStack.GetComponent<StackComponent>();            //cards being dropped
            if (scDrag.CardsInStack.Count == 0)
                return;
            //
            // first card of drag needs to match last card of drop
            //
            Entity firstCardonStack = scDrag.CardsInStack[0];               //get first card
            CardComponent firstCard = firstCardonStack.GetComponent<CardComponent>();
            //
            // Make sure this stack is not empty
            //
            if (scPlay.CardsInStack.Count == 0)
            {
                if (firstCard.FaceImage < 13)          //only a king will sit on empty stack
                {
                    ReturnCardFromDrag2Stack();
                    return;
                }
                //
                //  first card of drap is king, drop all of them
                //
                for (int i=0; i < scDrag.CardsInStack.Count; i++)
                {
                    scPlay.CardsInStack.Add(scDrag.CardsInStack[i]);
                }
                //
                // set the holding stack for this card pile
                //
                for (int i = 0; i < scPlay.CardsInStack.Count; i++)
                {
                    CardComponent _cc = scPlay.CardsInStack[i].GetComponent<CardComponent>();
                    _cc.HoldingStack = scPlay;
                }
                DragStackClear();
                return;
            }
            //
            // play stack is NOT empty, test cards
            //
            Entity lastCardonStack = scPlay.CardsInStack.LastOrDefault();               //get last card
            CardComponent lastCard = lastCardonStack.GetComponent<CardComponent>();
            if (TestCardsForPlayStack(firstCard, lastCard))
            {
                for (int i = 0; i < scDrag.CardsInStack.Count; i++)
                {
                    scPlay.CardsInStack.Add(scDrag.CardsInStack[i]);
                }
                //
                // set the holding stack for this card pile
                //
                for (int i = 0; i < scPlay.CardsInStack.Count; i++)
                {
                    CardComponent _cc = scPlay.CardsInStack[i].GetComponent<CardComponent>();
                    _cc.HoldingStack = scPlay;
                }
                DragStackClear();
            }
            else
            {
                ReturnCardFromDrag2Stack();
            }

        }
        public bool TestCardsForAceStack(CardComponent dragCard, CardComponent dropCard)
        {
            bool result = false;
            if ((dragCard == null) || (dropCard == null))
                return result;
             

            if (dropCard.Suit != dragCard.Suit)
                return result;
            //
            // Face value test
            //
            if (dropCard.FaceImage == dragCard.FaceImage - 1)
                result = true;

            return result;
        }
        public bool TestCardsForPlayStack(CardComponent dragCard, CardComponent dropCard)
        {
            bool result = false;

            if ((dragCard == null) || (dropCard == null))
                return false;

            //
            // Test colors
            //
            if ((dropCard.IsRed) && (dragCard.IsRed))               //both red
                return false;
            if ((!dropCard.IsRed) && (!dragCard.IsRed))             //both black
                return false;
            //
            // Face value test
            //
            if (dropCard.FaceImage == dragCard.FaceImage + 1)
                return true;

            return result;
        }
        public void TakeCards2Drag(Entity _entity)
        {
            //
            // Get the top card from 1-7 play stacks (_entity is a card)
            //
            CardComponent _cc = _entity.GetComponent<CardComponent>();
            if (_cc == null)
                return;

            StackComponent scTemp = _cc.HoldingStack;
            Entity fromEntity = scTemp.Entity;
            //
            // index of card we are dragging in Play Stack
            //
            int cInd = scTemp.CardsInStack.FindIndex(x => x.Id == _entity.Id);
            StackComponent scDrag = DragDisp.GetComponent<StackComponent>();
            //
            // if cInd is less than zero, then something is wrong
            //
            if (cInd < 0)
                return;

            for (int i=cInd; i <= scTemp.CardsInStack.Count - 1; i++)
            {
                Entity lastCard = scTemp.CardsInStack[i];
                CardComponent cc = lastCard.GetComponent<CardComponent>();
                //cc.IsFaceUp = true;
                scDrag.CardsInStack.Add(lastCard);
            }
            //
            // remove cards from original play stack
            //
            for (int i = 0; i < scDrag.CardsInStack.Count ; i++)
            {
                Entity lastCard = scDrag.CardsInStack[i];
                scTemp.CardsInStack.Remove(lastCard);

            }
            //
            // add cards to DispDrag
            //
            DragComponent sdc = new DragComponent() { EntityOrig = fromEntity };
            DragDisp.AddComponent<DragComponent>(sdc);
        }
        public void DealCard2Drag(Entity _entity)
        {
            //
            // Get the top card from either Ace piles or DrawDisp and add to DragDisp stack
            //
            StackComponent scDrag = DragDisp.GetComponent<StackComponent>();
            StackComponent scTemp = _entity.GetComponent<StackComponent>();
            if (scTemp == null)
                return;         //not a stack entity
            if (scTemp.CardsInStack.Count == 0)
                return;

            Entity lastCardonStack = scTemp.CardsInStack.LastOrDefault();               //get last card
            scTemp.CardsInStack.Remove(lastCardonStack);

            CardComponent lastCard = lastCardonStack.GetComponent<CardComponent>();
            lastCard.IsFaceUp = true;


            scDrag.CardsInStack.Add(lastCardonStack);
            DragComponent sdc = new DragComponent() { EntityOrig = _entity };
            DragDisp.AddComponent<DragComponent>(sdc);
        }
        public void DragStackClear()
        {
            DragDisp.RemoveComponent<DragComponent>();
            StackComponent sc = DragDisp.GetComponent<StackComponent>();
            sc.CardsInStack.Clear();
        }
        public void DealCard2Disp(Entity _entity)
        {
            //
            // take last card from the deal deck
            //
            StackComponent scDisp = DrawDisp.GetComponent<StackComponent>();
            StackComponent scTemp = DealDeck.GetComponent<StackComponent>();
            if (scTemp == null)
                return;         //not a stack entity
            //
            // if deal deck is empty, put all face up cards back in deal deck
            //
            if (scTemp.CardsInStack.Count <= 0)
            {
                for (int i = scDisp.CardsInStack.Count - 1; i >= 0; i--)
                {
                    CardComponent _card = scDisp.CardsInStack[i].GetComponent<CardComponent>();
                    _card.IsFaceUp = false;
                    scTemp.CardsInStack.Add(scDisp.CardsInStack[i]);
                }
                scDisp.CardsInStack.Clear();
                return;
            }

            Entity lastCardonStack = scTemp.CardsInStack.LastOrDefault();               //get last card
            scTemp.CardsInStack.Remove(lastCardonStack);

            CardComponent lastCard = lastCardonStack.GetComponent<CardComponent>();
            lastCard.IsFaceUp = true;


            scDisp.CardsInStack.Add(lastCardonStack);

        }
        public Entity FindCardInPlayStack(Entity _playStack, Vector2 _mousePos)
        {
            //
            // entity = PlayStack
            //
            StackOfCards Cards = new StackOfCards(_playStack);
            Vector2 startPos = _playStack.Transform.Position;
            int imax = Cards.CardsInStack.Count - 1;
            int ind = imax;

            for (int i = imax; i >= 0; i--)
            {
                Entity cardEntity = Cards.CardsInStack[i];
                CardComponent cc = cardEntity.GetComponent<CardComponent>();
                if (cc.IsFaceUp)
                {
                    //
                    // Card we are testing is face up
                    // Find its location, draw a rectangle and see if mouse click is in area
                    // 
                    Vector2 cardPos = startPos + Cards.FanOutDistannce * ind;
                    Rectangle cardRect = new Rectangle((int)cardPos.X - CardDeckManager.cardWidth/2, (int)cardPos.Y - CardDeckManager.cardHeight/2, 
                                                       CardDeckManager.cardWidth, CardDeckManager.cardHeight);
                    if (cardRect.Contains(_mousePos))
                    {
                        return cardEntity;
                    }
                }
                else
                {
                    if (cardEntity == Cards.LastCardonStack)
                    {
                        //
                        // last card on stack needs to be face up
                        //
                        cc.IsFaceUp = true;
                        return cardEntity;
                    }
                }
                ind -= 1;
            }
            return null;
        }
        public void Fill_All_Stacks()
        {
            CardDeckManager.Shuffle();
            StackComponent scDisp = DrawDisp.GetComponent<StackComponent>();
            scDisp.CardsInStack.Clear();
            //
            // Play Stack 0 gets 1 card
            // Play Stack 1 gets 2 cards
            // Play Stack 6 gets 7 cards
            //
            for (int i = 0; i < PlayStacks.Count; i++)
            {
                StackComponent sc = PlayStacks[i].GetComponent<StackComponent>();
                sc.CardsInStack.Clear();
                for (int j = 0; i >= j; j++)
                {
                    //
                    // Create a card Enitty
                    //
                    var card = CardDeckManager.DealACard(false);
                    var ccomp = card.GetComponent<CardComponent>();
                    ccomp.CardStack = sc.StackID;
                    ccomp.HoldingStack = sc;
                    sc.CardsInStack.Add(card);
                }
                //
                // Find last card in this stack and flip it face up
                //
                StackComponent scTemp = PlayStacks[i].GetComponent<StackComponent>();
                Entity lastCardonStack = scTemp.CardsInStack.LastOrDefault();               //get last card
                var ccompTemp = lastCardonStack.GetComponent<CardComponent>();              //turn it face up
                ccompTemp.IsFaceUp = true;

            }
            //
            // Dealer Stack gets rest of cards (24 of them)
            //
            StackComponent scDeal = DealDeck.GetComponent<StackComponent>();
            scDeal.CardsInStack.Clear();
            for (int i = 0; i < 52 ; i++)
            {
                var card = CardDeckManager.DealACard(false);
                if (card == null)
                    break;
                var ccomp = card.GetComponent<CardComponent>();
                ccomp.CardStack = scDeal.StackID;
                ccomp.HoldingStack = scDeal;
                scDeal.CardsInStack.Add(card);
            }
        }
    }
}
