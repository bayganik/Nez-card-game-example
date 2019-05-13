using System;
using Nez;
using Nez.UI;

namespace CardGame.Scenes
{
    public abstract class BaseScene : Scene
    {
        public BaseScene() { }
        public void SetupScene()
        {
            addRenderer(new DefaultRenderer());

        }
    }
}
