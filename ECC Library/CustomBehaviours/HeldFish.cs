using ECCLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECCLibrary.Internal
{
    public class HeldFish : DropTool
    {
        public string animationName;

        public override string animToolName
        {
            get
            {
                if (string.IsNullOrEmpty(animationName))
                {
                    ECCLog.AddMessage("Item {0} has an invalid ReferenceHoldingAnimation TechType");
                }
                return animationName;
            }
        }
    }
}
