using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeyondInfinity
{
    public partial class Mark
    {
        public void Effect_Start()
        {
            switch (EffectID)
            {
                case 2: Game.Character.Global_Haste -= Stack * 50; break;
                case 4: Game.Character.Global_Resistance -= Stack * 50; break;
                case 5: Game.Character.Global_Accuracy -= Stack * 50; break;

                case 6: Game.Character.Global_ClearcastChance -= Stack * 50; break; //not proper
                case 7: Game.Character.Global_ClearcastChance += Stack * 50; break; //not proper
                case 9: Game.Character.Global_Resistance += Stack * 50; break; //not proper
                case 11: Game.Character.Global_Haste += Stack * 50; break; //not proper

                case 38: Game.Character.Global_Accuracy += Stack * 100; break; //not proper
            }
        }

        public void Effect_StackModify(int Value)
        {
            switch (EffectID)
            {
                case 2: Game.Character.Global_Haste -= Value * 50; break;
                case 4: Game.Character.Global_Resistance -= Value * 50; break;
                case 5: Game.Character.Global_Accuracy -= Value * 50; break;

                case 6: Game.Character.Global_ClearcastChance -= Value * 50; break; //not proper
                case 7: Game.Character.Global_ClearcastChance += Value * 50; break; //not proper
                case 9: Game.Character.Global_Resistance += Value * 50; break; //not proper
                case 11: Game.Character.Global_Haste += Value * 50; break; //not proper

                case 38: Game.Character.Global_Accuracy += Value * 100; break; //not proper
            }
        }

        public void Effect_End()
        {
            switch (EffectID)
            {
                case 2: Game.Character.Global_Haste += Stack * 50; break;
                case 4: Game.Character.Global_Resistance += Stack * 50; break;
                case 5: Game.Character.Global_Accuracy += Stack * 50; break;

                case 6: Game.Character.Global_ClearcastChance += Stack * 50; break; //not proper
                case 7: Game.Character.Global_ClearcastChance -= Stack * 50; break; //not proper
                case 9: Game.Character.Global_Resistance -= Stack * 50; break; //not proper
                case 11: Game.Character.Global_Haste -= Stack * 50; break; //not proper

                case 38: Game.Character.Global_Accuracy -= Stack * 100; break; //not proper
            }
        }
    }
}
