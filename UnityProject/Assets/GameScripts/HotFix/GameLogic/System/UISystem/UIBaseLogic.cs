using System;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 界面逻辑基类。
    /// </summary>
    public abstract class UIBaseLogic : MonoBehaviour
    {
        private UIBase m_UIBase;

        /// <summary>
        /// 获取界面。
        /// </summary>
        public UIBase UIBase => m_UIBase;

        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public RectTransform CachedTransform => m_UIBase.rectTransform;

        /// <summary>
        /// 界面初始化。
        /// </summary>
        protected internal virtual void OnInit(UIBase uiBase)
        {
            m_UIBase = uiBase;
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        protected internal virtual void OnOpen()
        {
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        protected internal virtual void OnClose()
        {
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        protected internal virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        protected internal virtual void OnSortDepth(int depth)
        {
        }
        
        #region UIEvent

        protected void AddUIEvent(int eventType, Action handler)
        {
            m_UIBase.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T>(int eventType, Action<T> handler)
        {
            m_UIBase.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T, U>(int eventType, Action<T, U> handler)
        {
            m_UIBase.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            m_UIBase.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T, U, V, W>(int eventType, Action<T, U, V, W> handler)
        {
            m_UIBase.AddUIEvent(eventType, handler);
        }

        #endregion
    }
}
