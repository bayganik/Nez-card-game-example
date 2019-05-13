using Nez;
using Nez.Sprites;
using Nez.UI;
using CardGame.Scenes;
using Nez.ImGuiTools;

namespace CardGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        //
        // Very important screen size is NOT bigger than the map
        //
        public Game1() : base(960, 800, false, true, "Card Game Example")
        { }
        protected override void Initialize()
        {

            base.Initialize();
            //System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));

            IsMouseVisible = false;
            debugRenderEnabled = true;
            Window.AllowUserResizing = true;
            debugRenderEnabled = false;
            //
            //
            //
            //var imGuiManager = new ImGuiManager();
            //Core.registerGlobalManager(imGuiManager);

            scene = new Scenes.MainScene();
        }


    }
}
