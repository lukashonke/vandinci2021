using InControl;

namespace _Chi.Scripts.Mono.Controls
{
    public class MainPlayerActions : PlayerActionSet
    {
        public PlayerAction Left;
        public PlayerAction Right;
        public PlayerAction Up;
        public PlayerAction Down;
        public PlayerTwoAxisAction Move;

        public PlayerAction Skill1;
        public PlayerAction Skill2;
        public PlayerAction Skill3;
        public PlayerAction Skill4;

        public MainPlayerActions()
        {
            Left = CreatePlayerAction("Move Left");
            Right = CreatePlayerAction("Move Right");
            Up = CreatePlayerAction("Move Up");
            Down = CreatePlayerAction("Move Down");

            Skill1 = CreatePlayerAction("Skill1");
            Skill2 = CreatePlayerAction("Skill2");
            Skill3 = CreatePlayerAction("Skill3");
            Skill4 = CreatePlayerAction("Skill4");

            Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
        }
    }
}