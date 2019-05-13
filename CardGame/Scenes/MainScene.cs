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
        Sprite<int> eachCard;
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
        public override void initialize()
        {
            base.initialize();
            font = new NezSpriteFont(content.Load<SpriteFont>("Arial"));

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // put a Canvas entity on upper right hand side for UI
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            Entity uiCanvas = createEntity("ui-canvas");
            UIC = uiCanvas.addComponent(new UICanvas());

            ExitButton = UIC.stage.addElement(new TextButton("End !", Skin.createDefaultSkin()));
            ExitButton.setPosition(800f, 30f);
            ExitButton.setSize(60f, 20f);
            ExitButton.onClicked += ExitButton_onClicked;

            NewButton = UIC.stage.addElement(new TextButton("Play !", Skin.createDefaultSkin()));
            NewButton.setPosition(800f, 60f);
            NewButton.setSize(60f, 20f);
            NewButton.onClicked += NewButton_onClicked;

            Msg = UIC.stage.addElement(new Label("Label Msg"));
            Msg.setPosition(800f, 90f);
            Msg.setSize(100f, 50f);

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Text entity with component (Game name label)
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            TextEntity = createEntity("txt");
            TextEntity.transform.position = new Vector2(350, 20);
            TextEntity.transform.scale = new Vector2(1, 1);
            var txt = new Text(Graphics.instance.bitmapFont, "Card Play Game", new Vector2(0, 0), Color.White);
            txt.setFont(font);
            TextEntity.addComponent(txt);

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Score Text entity 
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            ScoreEntity = createEntity("score");
            ScoreEntity.transform.position = new Vector2(10, 20);
            ScoreEntity.transform.scale = new Vector2(1, 1);
            txt = new Text(Graphics.instance.bitmapFont, "Score 0", new Vector2(0, 0), Color.White);
            txt.setFont(font);
            ScoreEntity.addComponent(txt);
            ScoreEntity.addComponent(new EndGameComponent());               //end of game goes with score entity
            CardDeckManager.score = 0;
            CardDeckManager.endOfGame = false;  
            //
            // Create a deck of cards from image
            //
            var texture = content.Load<Texture2D>("CardDeck_72x100");
            CardDeckManager.InitAllCards(texture);                      //pass the texture to manager
            CardDeckManager.CreateDeckOfCards(this);                    //pass the scene to manager
            
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // mouse entity has image that is a hand
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            var mouseEntity = createEntity("mouse");
            var mouseSprite = mouseEntity.addComponent(new Sprite(content.Load<Texture2D>("hand1")));
            mouseSprite.renderLayer = -99;
            mouseEntity.transform.scale = new Vector2(.30f, .30f);
            
            mouseEntity.transform.position = new Vector2(30, 30);
            mouseEntity.addComponent(new BoxCollider());
            mouseEntity.addComponent(new MouseComponent());
            
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Deal Deck (image to click on for next card)
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            DealDeck = createEntity("DealerStack", drawStack);
            DealDeck.tag = 90;
            DealDeck.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            DealDeck.addComponent(new BoxCollider());
            DealDeck.addComponent(new StackComponent() { StackID = 0 });
            DealDeck.addComponent(new PilePlayComponent());

            //znznznznznznznznznznznznznznznznznznznznznzn
            // 4 Ace stacks to collect cards
            //znznznznznznznznznznznznznznznznznznznznznzn
            var as1 = createEntity("AceStack1", aceStack1);
            as1.tag = 80;               //special tag for Ace pile
            as1.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            as1.addComponent(new BoxCollider());
            as1.addComponent(new StackComponent() { StackID = 1, InitDealCnt = 0, CName = "AceStack" });
            as1.addComponent(new PileDisplayedComponent());
            AceStacks.Add(as1);

            var as2 = createEntity("AceStack2", aceStack1 + new Vector2(100, 0));
            as2.tag = 80;               //special tag for Ace pile
            as2.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            as2.addComponent(new BoxCollider());
            as2.addComponent(new StackComponent() { StackID = 2 });
            as2.addComponent(new PileDisplayedComponent());
            AceStacks.Add(as2);

            var as3 = createEntity("AceStack3", aceStack1 + new Vector2(200, 0));
            as3.tag = 80;               //special tag for Ace pile
            as3.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            as3.addComponent(new BoxCollider());
            as3.addComponent(new StackComponent() { StackID = 3 });
            as3.addComponent(new PileDisplayedComponent());
            AceStacks.Add(as3);

            var as4 = createEntity("AceStack4", aceStack1 + new Vector2(300, 0));
            as4.tag = 80;               //special tag for Ace pile
            as4.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            as4.addComponent(new BoxCollider());
            as4.addComponent(new StackComponent() { StackID = 4 });
            as4.addComponent(new PileDisplayedComponent());
            AceStacks.Add(as4);
            
            //znznznznznznznznznznznznznznznznznznznznznzn
            // 7 playing stacks
            //znznznznznznznznznznznznznznznznznznznznznzn
            var pl1 = createEntity("PlayStack1", playStack1);
            pl1.tag = 1;
            pl1.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl1.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl1.addComponent(new StackComponent() { StackID = 5, FannedDirection = 4 });
            pl1.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl1);

            var pl2 = createEntity("PlayStack2", playStack1 + new Vector2(100, 0));
            pl2.tag = 2;
            pl2.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl2.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl2.addComponent(new StackComponent() { StackID = 6, FannedDirection = 4 });
            pl2.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl2);

            var pl3 = createEntity("PlayStack3", playStack1 + new Vector2(200, 0));
            pl3.tag = 3;
            pl3.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl3.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl3.addComponent(new StackComponent() { StackID = 7, FannedDirection = 4 });
            pl3.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl3);

            var pl4 = createEntity("PlayStack4", playStack1 + new Vector2(300, 0));
            pl4.tag = 4;
            pl4.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl4.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl4.addComponent(new StackComponent() { StackID = 8, FannedDirection = 4 });
            pl4.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl4);

            var pl5 = createEntity("PlayStack5", playStack1 + new Vector2(400, 0));
            pl5.tag = 5;
            pl5.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl5.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl5.addComponent(new StackComponent() { StackID = 9, FannedDirection = 4 });
            pl5.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl5);

            var pl6 = createEntity("PlayStack6", playStack1 + new Vector2(500, 0));
            pl6.tag = 6;
            pl6.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl6.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl6.addComponent(new StackComponent() { StackID = 10, FannedDirection = 4 });
            pl6.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl6);

            var pl7 = createEntity("PlayStack7", playStack1 + new Vector2(600, 0));
            pl7.tag = 7;
            pl7.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            pl7.addComponent(new BoxCollider(colliderX, colliderY, colliderWidth, colliderHeight));
            pl7.addComponent(new StackComponent() { StackID = 11, FannedDirection = 4 });
            pl7.addComponent(new PilePlayComponent());
            PlayStacks.Add(pl7);
            
            //znznznznznznznznznznznznznznznznznznznznznzn
            // Draw stack (all cards are faceup)
            //znznznznznznznznznznznznznznznznznznznznznzn
            DrawDisp = createEntity("DrawDisp", drawStack + new Vector2(100, 0));
            DrawDisp.tag = 80;
            DrawDisp.addComponent(new Sprite(content.Load<Texture2D>("EmptyHolder")));
            DrawDisp.addComponent(new BoxCollider());
            DrawDisp.addComponent(new StackComponent() { StackID = 0, CName = "DrawDisp", FannedDirection = 0 });
            DrawDisp.addComponent(new PileDisplayedComponent());
            
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Drag stack (all cards are faceup and are being moved)
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            DragDisp = createEntity("DragDisp", new Vector2(0, 0));
            DragDisp.tag = 85;
            
            DragDisp.addComponent(new BoxCollider());
            DragDisp.addComponent(new StackComponent() { StackID = 0, FannedDirection = 4 });

            Fill_All_Stacks();

            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            // Systems to process our requests
            //znznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznznzn
            //
            this.addEntityProcessor(new MouseClickSystem(new Matcher().all(typeof(MouseComponent))));
            this.addEntityProcessor(new PlayPileDispSystem(new Matcher().all(typeof(PilePlayComponent))));
            this.addEntityProcessor(new AcesPileDispSystem(new Matcher().all(typeof(PileDisplayedComponent))));
            this.addEntityProcessor(new StackDragDispSystem(new Matcher().all(typeof(DragComponent))));
            this.addEntityProcessor(new EndOfGameSystem(new Matcher().all(typeof(EndGameComponent))));

        }
        public bool EndOfGameTest()
        {
            return false;

            //StackComponent scDeal = DealDeck.getComponent<StackComponent>();
            //for (int i = 0; i < scDeal.CardsInStack.Count; i++)
            //{
            //    Entity card1 = scDeal.GetCard(i);
            //    if (card1 == null)
            //        break;
            //    var dragCard = card1.getComponent<CardComponent>();
            //    //
            //    // test this card to put on PlayStack
            //    //
            //    for (int pi = 0; pi < PlayStacks.Count; pi++)
            //    {
            //        StackComponent sc = PlayStacks[pi].getComponent<StackComponent>();
            //        Entity card2 = sc.GetLastCard();
            //        if (card2 == null)
            //            break;
            //        var dropCard = card2.getComponent<CardComponent>();
            //        if (TestCardsForPlayStack(dragCard, dropCard))
            //            return false;               //card can be played, not end of game
            //    }

            //}
            //return true;
        }
        public void DispGameScore()
        {
            var txt = ScoreEntity.getComponent<Text>();
            txt.renderLayer = -100;
            txt.setText("Score " + CardDeckManager.score.ToString());

        }
        public void DispGameOver()
        {
            TextEntity.transform.position = new Vector2(350, 400);
            var txt = TextEntity.getComponent<Text>();
            txt.renderLayer = -100;
            txt.setText("GAME IS OVER !");
            txt.setColor(Color.Black);
        }
        private void ExitButton_onClicked(Button button)
        {
            //
            // Exit button is pressed
            //
            TextEntity.transform.position = new Vector2(350, 400);
            var txt = TextEntity.getComponent<Text>();
            txt.renderLayer = -100;
            txt.setText("GAME IS OVER !");
            txt.setColor(Color.Black);

            CardDeckManager.endOfGame = true;
        }
        private void NewButton_onClicked(Button button)
        {
            //CardDeckManager.CreateDeckOfCards(this);   //create a new deck and shuffle
            //Fill_All_Stacks();
            //
            // New button is pressed
            //
            var msg = UIC.stage.getElements();
            foreach(Element el in msg)
            {
                if ((el.GetType() == typeof(Label)))
                {
                    var lbl = (Label)el;
                    lbl.setText("New Button Pushed");
                }
            }
            //
            // put back the main heading
            //
            TextEntity.transform.position = new Vector2(350, 20);
            var txt = TextEntity.getComponent<Text>();
            txt.renderLayer = -100;
            txt.setText("GAME is new again !");
            txt.setColor(Color.Black);

            CardDeckManager.score = 0;
            CardDeckManager.endOfGame = false;
        }
        public void ReturnCardFromDrag2Stack()
        {
            DragComponent scDragComp = DragDisp.getComponent<DragComponent>();
            if (scDragComp == null)
                return;

            Entity fromEntity = scDragComp.EntityOrig;
            StackComponent scDrag = DragDisp.getComponent<StackComponent>();            //cards being dragged
            StackComponent scFrom = fromEntity.getComponent<StackComponent>();            //cards to give back
            for (int i=0; i < scDrag.CardsInStack.Count; i++)
            {
                scFrom.CardsInStack.Add(scDrag.CardsInStack[i]);
            }
            DragStackClear();

        }
        public void DropCardFromDrag2AceStat(Entity _playStack)
        {
            StackComponent scDrag = DragDisp.getComponent<StackComponent>();            //cards being dragged

            if (scDrag.CardsInStack.Count != 1)
            {
                ReturnCardFromDrag2Stack();
                return;
            }
            DragComponent scDragComp = DragDisp.getComponent<DragComponent>();
            if (scDragComp.EntityOrig == _playStack)
            {
                ReturnCardFromDrag2Stack();
                return;
            }

            StackComponent scPlay = _playStack.getComponent<StackComponent>();            //cards being dropped
            //
            // first card of drag needs to match last card of drop
            //
            Entity firstCardonStack = scDrag.CardsInStack[0];               //get first card of drag
            CardComponent firstCard = firstCardonStack.getComponent<CardComponent>();
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
            CardComponent lastCard = lastCardonStack.getComponent<CardComponent>();
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
            StackComponent scDrag = DragDisp.getComponent<StackComponent>();            //cards being dragged
            StackComponent scPlay = _playStack.getComponent<StackComponent>();            //cards being dropped
            if (scDrag.CardsInStack.Count == 0)
                return;
            //
            // first card of drag needs to match last card of drop
            //
            Entity firstCardonStack = scDrag.CardsInStack[0];               //get first card
            CardComponent firstCard = firstCardonStack.getComponent<CardComponent>();
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
                    CardComponent _cc = scPlay.CardsInStack[i].getComponent<CardComponent>();
                    _cc.HoldingStack = scPlay;
                }
                DragStackClear();
                return;
            }
            //
            // play stack is NOT empty, test cards
            //
            Entity lastCardonStack = scPlay.CardsInStack.LastOrDefault();               //get last card
            CardComponent lastCard = lastCardonStack.getComponent<CardComponent>();
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
                    CardComponent _cc = scPlay.CardsInStack[i].getComponent<CardComponent>();
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
            CardComponent _cc = _entity.getComponent<CardComponent>();
            if (_cc == null)
                return;

            StackComponent scTemp = _cc.HoldingStack;
            Entity fromEntity = scTemp.entity;
            //
            // index of card we are dragging in Play Stack
            //
            int cInd = scTemp.CardsInStack.FindIndex(x => x.id == _entity.id);
            StackComponent scDrag = DragDisp.getComponent<StackComponent>();
            //
            // if cInd is less than zero, then something is wrong
            //
            if (cInd < 0)
                return;

            for (int i=cInd; i <= scTemp.CardsInStack.Count - 1; i++)
            {
                Entity lastCard = scTemp.CardsInStack[i];
                CardComponent cc = lastCard.getComponent<CardComponent>();
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
            DragDisp.addComponent<DragComponent>(sdc);
        }
        public void DealCard2Drag(Entity _entity)
        {
            //
            // Get the top card from either Ace piles or DrawDisp and add to DragDisp stack
            //
            StackComponent scDrag = DragDisp.getComponent<StackComponent>();
            StackComponent scTemp = _entity.getComponent<StackComponent>();
            if (scTemp == null)
                return;         //not a stack entity
            if (scTemp.CardsInStack.Count == 0)
                return;

            Entity lastCardonStack = scTemp.CardsInStack.LastOrDefault();               //get last card
            scTemp.CardsInStack.Remove(lastCardonStack);

            CardComponent lastCard = lastCardonStack.getComponent<CardComponent>();
            lastCard.IsFaceUp = true;


            scDrag.CardsInStack.Add(lastCardonStack);
            DragComponent sdc = new DragComponent() { EntityOrig = _entity };
            DragDisp.addComponent<DragComponent>(sdc);
        }
        public void DragStackClear()
        {
            DragDisp.removeComponent<DragComponent>();
            StackComponent sc = DragDisp.getComponent<StackComponent>();
            sc.CardsInStack.Clear();
        }
        public void DealCard2Disp(Entity _entity)
        {
            //
            // take last card from the deal deck
            //
            StackComponent scDisp = DrawDisp.getComponent<StackComponent>();
            StackComponent scTemp = DealDeck.getComponent<StackComponent>();
            if (scTemp == null)
                return;         //not a stack entity
            //
            // if deal deck is empty, put all face up cards back in deal deck
            //
            if (scTemp.CardsInStack.Count <= 0)
            {
                for (int i = scDisp.CardsInStack.Count - 1; i >= 0; i--)
                {
                    CardComponent _card = scDisp.CardsInStack[i].getComponent<CardComponent>();
                    _card.IsFaceUp = false;
                    scTemp.CardsInStack.Add(scDisp.CardsInStack[i]);
                }
                scDisp.CardsInStack.Clear();
                return;
            }

            Entity lastCardonStack = scTemp.CardsInStack.LastOrDefault();               //get last card
            scTemp.CardsInStack.Remove(lastCardonStack);

            CardComponent lastCard = lastCardonStack.getComponent<CardComponent>();
            lastCard.IsFaceUp = true;


            scDisp.CardsInStack.Add(lastCardonStack);

        }
        public Entity FindCardInPlayStack(Entity _playStack, Vector2 _mousePos)
        {
            //
            // entity = PlayStack
            //
            StackOfCards Cards = new StackOfCards(_playStack);
            Vector2 startPos = _playStack.transform.position;
            int imax = Cards.CardsInStack.Count - 1;
            int ind = imax;

            for (int i = imax; i >= 0; i--)
            {
                Entity cardEntity = Cards.CardsInStack[i];
                CardComponent cc = cardEntity.getComponent<CardComponent>();
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
            StackComponent scDisp = DrawDisp.getComponent<StackComponent>();
            scDisp.CardsInStack.Clear();
            //
            // Play Stack 0 gets 1 card
            // Play Stack 1 gets 2 cards
            // Play Stack 6 gets 7 cards
            //
            for (int i = 0; i < PlayStacks.Count; i++)
            {
                StackComponent sc = PlayStacks[i].getComponent<StackComponent>();
                sc.CardsInStack.Clear();
                for (int j = 0; i >= j; j++)
                {
                    //
                    // Create a card Enitty
                    //
                    var card = CardDeckManager.DealACard(false);
                    var ccomp = card.getComponent<CardComponent>();
                    ccomp.CardStack = sc.StackID;
                    ccomp.HoldingStack = sc;
                    sc.CardsInStack.Add(card);
                }
                //
                // Find last card in this stack and flip it face up
                //
                StackComponent scTemp = PlayStacks[i].getComponent<StackComponent>();
                Entity lastCardonStack = scTemp.CardsInStack.LastOrDefault();               //get last card
                var ccompTemp = lastCardonStack.getComponent<CardComponent>();              //turn it face up
                ccompTemp.IsFaceUp = true;

            }
            //
            // Dealer Stack gets rest of cards (24 of them)
            //
            StackComponent scDeal = DealDeck.getComponent<StackComponent>();
            scDeal.CardsInStack.Clear();
            for (int i = 0; i < 52 ; i++)
            {
                var card = CardDeckManager.DealACard(false);
                if (card == null)
                    break;
                var ccomp = card.getComponent<CardComponent>();
                ccomp.CardStack = scDeal.StackID;
                ccomp.HoldingStack = scDeal;
                scDeal.CardsInStack.Add(card);
            }
        }
    }
}
