using System;
using UnityGameFramework.Runtime;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface IUISystemEvent
    {
        
        public void OnUIOpen(UIWindow window);
        
        public void OnUIClose(UIWindow window);

    }
}