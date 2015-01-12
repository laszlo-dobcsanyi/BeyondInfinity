using System;

namespace BeyondInfinity_Server
{
    public partial class Person
    {
        #region Person0 - Leader0
        public string Dialog(Character Character, byte Answer)
        {
            Character.Dialogs[DialogID, 1] = Answer;

            switch (Character.Dialogs[DialogID, 0])
            {
                case 0:
                    switch (Character.Dialogs[DialogID, 1])
                    {
                        case 0: return "0\t1";
                        case 1: return "1\t2,3";
                        case 2: Character.Dialogs[DialogID, 0] = 1; Character.Dialogs[DialogID, 1] = 0; return "2\t4";
                        case 3: Character.Dialogs[DialogID, 0] = 2; Character.Dialogs[DialogID, 1] = 0; return "3\t4";
                        case 4: return "-1\t-1";
                    }
                    break;

                case 1:
                    switch (Character.Dialogs[DialogID, 1])
                    {
                        case 0: return "4\t5";
                        case 5: return "-1\t-1";
                    }
                    break;

                case 2:
                    switch (Character.Dialogs[DialogID, 1])
                    {
                        case 0: return "5\t6";
                        case 6: return "-1\t1";
                    }

                    break;
            }

            return "-1\t-1";
        }
        #endregion

        #region Person1 - Leader1

        #endregion

        #region Person2 - Leader2

        #endregion
    }
}
