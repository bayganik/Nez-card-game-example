using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using CardGame.Components;
using CardGame.Scenes;


namespace CardGame.Systems
{
    public class MouseClickSystem : EntityProcessingSystem
    {
        MouseState CurrentMouse;
        MouseState PrevMouse;
        bool Dragging = false;
        MainScene MainGameScene;

        Vector2 MousePos;
        public MouseClickSystem(Matcher matcher) : base(matcher)
        {

        }
        public override void Process(Entity entity)
        {
            //
            // ONLY MOUSE entity comes here
            //
            MainGameScene = entity.Scene as MainScene;              //hand entity belongs to MainScene
            //
            // Hand image is the entity that comes here
            //
            var _mouseCollider = entity.GetComponent<BoxCollider>();
            PrevMouse = CurrentMouse;
            CurrentMouse = Mouse.GetState();
            //
            // Current location of the mouse used for the hand icon
            //
            entity.Transform.Position = Scene.Camera.ScreenToWorldPoint(new Vector2(CurrentMouse.Position.X, CurrentMouse.Position.Y));
            MousePos = new Vector2(CurrentMouse.Position.X, CurrentMouse.Position.Y);

            if (Input.LeftMouseButtonReleased)
            {
                if (Dragging)
                {
                    Dragging = false;
                    if (!_mouseCollider.CollidesWithAny(out CollisionResult collisionResult))
                        return;

                    Entity collidedEntity = collisionResult.Collider.Entity;

                    //
                    // Dealt Card is released but was not put on a stack
                    //

                    if (collidedEntity.Tag == 80)
                    {
                        //
                        // Ace pile drop
                        //
                        MainGameScene.DropCardFromDrag2AceStat(collidedEntity);
                        return;                     //ace pile stack
                    }
                    if ((collidedEntity.Tag >= 1) && (collidedEntity.Tag <= 7))
                    {
                        //
                        // Play pile drop
                        //
                        MainGameScene.DropCardFromDrag2PlayStack(collidedEntity);
                        return;
                    }
                    //
                    // mouse released but not on Ace or Play area, return card to its place
                    //
                    MainGameScene.ReturnCardFromDrag2Stack();
                    return;                     //drap disp stack (release of mouse outside of play area)
                }
                else
                {
                    //
                    // test card if last and face down, then flip it up
                    //
                }
            }
            if (Input.LeftMouseButtonPressed)
            {
                if (CardDeckManager.endOfGame)
                    return;

                if (!_mouseCollider.CollidesWithAny(out CollisionResult collisionResult))
                    return;                                             //clicking on entities that have no colliders
                //
                // We have clicked on a box collider
                //

                Entity collidedEntity = collisionResult.Collider.Entity;
                //
                // Stacks with tag=90 dealer pile displayed
                // Stacks with tag=80 are Ace piles or Disp deal card
                // Stacks with tag=70 dealer pile not displayed
                // Stacks with tags 1 - 7 are play stacks
                //
                if (collidedEntity.Tag == 80)
                {
                    //
                    // Ace pile
                    //
                    Dragging = true;
                    MainGameScene.DealCard2Drag(collidedEntity);

                    return;
                }
                if (collidedEntity.Tag == 90)
                {
                    //
                    // Dealer stack with displayed cards
                    //
                    Dragging = false;
                    MainGameScene.DealCard2Disp(collidedEntity);

                    return;
                }
                if ((collidedEntity.Tag >= 1) && (collidedEntity.Tag <= 7))
                {
                    //
                    // Play stacks 
                    //
                    Entity cardEntity = MainGameScene.FindCardInPlayStack(collidedEntity, new Vector2(MousePos.X, MousePos.Y));
                    if (cardEntity == null)
                        return;
                    //
                    // We have clicked on a Card/Cards that are going to be dragged
                    //
                    CardComponent isCard = cardEntity.GetComponent<CardComponent>();
                    if (isCard != null)
                    {
                        //
                        // if not face up, then leave it alone
                        //
                        //if (!isCard.IsFaceUp)
                        //    return;
                        //
                        // we have hit a card drag it (and all others under it)
                        //
                        Dragging = true;
                        MainGameScene.TakeCards2Drag(cardEntity);
                    }
                }
            }
        }
    }
}
