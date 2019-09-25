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
    public class EndOfGameSystem : EntityProcessingSystem
    {
        //
        // Entities with EndGameComponent to determine if this game is over
        //
        MainScene MainGameScene;
        public EndOfGameSystem(Matcher matcher) : base(matcher)
        {
        }
        public override void Process(Entity entity)
        {
            //
            // TextEntity comes here
            //
            MainGameScene = entity.Scene as MainScene;              //hand entity belongs to MainScene
            MainGameScene.DispGameScore();

            if (MainGameScene.EndOfGameTest())
            {
                MainGameScene.DispGameOver();
            }
            
        }
    }
}
