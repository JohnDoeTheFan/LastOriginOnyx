using Onyx.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Ai
{
    [CreateAssetMenu(menuName = "Utility/Ai/Sniper")]
    public class SniperAiScriptFactory : AiScriptFactoryBase
    {
        public override AiScriptBase ProductAiScript()
        {
            return new SniperAiScript();
        }

        public class SniperAiScript : AiScriptBase
        {
            public override void OnUpdate()
            {
                controller.SetButtonDown(InputSource.Button.R2);
            }
        }
    }
}