using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Labs
{
    class Program
    {
        /* Change m_CurrentTask to the current task you are working on.
         * Valid tasks are L1, L2_1, L2_2, L3, Challenge, L4_1, L4_2, L5 and ACW
         */
        private static Lab m_CurrentLab = Lab.ACW;

        #region No Changes Required Here Ever!
        /* 
         * By all means look at this code, but you will not be required to change
         * any of this code to complete any lab work
         */
        private enum Lab { L1, L2_1, L2_2, L3, Challenge, L4_1, L4_2, L5, ACW };
        static void Main(string[] args)
        {
            GameWindow window = null;
            switch(m_CurrentLab)
            {
                //case Lab.L1:
                //    window = new Lab1.Lab1Window();
                //    break;
                //case Lab.L2_1:
                //    window = new Lab2.Lab2_1Window();
                //    break;
                //case Lab.L2_2:
                //    window = new Lab2.Lab2_2Window();
                //    break;
                //case Lab.L3:
                //    window = new Lab3.Lab3Window();
                //    break;
                //case Lab.Challenge:
                //    window = new Challenge.ChallengeWindow();
                //    break;
                //case Lab.L4_1:
                //    window = new Lab4.Lab4_1Window();
                //    break;
                //case Lab.L4_2:
                //    window = new Lab4.Lab4_2Window();
                ////    break;
                //case Lab.L5:
                //    window = new Lab5.Lab5Window();
                //    break;
                case Lab.ACW:
                    window = new ACW.ACWWindow();
                    break;
                default:
                    throw new Exception("Unrecognised lab number");
            }

            if (window != null)
            {
                window.Run();
            }
        }
        #endregion
    }
}
