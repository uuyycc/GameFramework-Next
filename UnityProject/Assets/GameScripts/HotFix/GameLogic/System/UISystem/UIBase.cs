using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameLogic
{
    /// <summary>
    /// UI基类。
    /// </summary>
    public class UIBase
    {
        /// <summary>
        /// 所属UI父节点。
        /// </summary>
        protected UIBase parent = null;

        /// <summary>
        /// UI父节点。
        /// </summary>
        public UIBase Parent => parent;

        /// <summary>
        /// 自定义数据集。
        /// </summary>
        public object UserData { protected set; get; }

        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public virtual GameObject gameObject { protected set; get; }

        /// <summary>
        /// 窗口位置组件。
        /// </summary>
        public virtual Transform transform { protected set; get; }

        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public virtual RectTransform rectTransform { protected set; get; }

        /// <summary>
        /// UI类型。
        /// </summary>
        public virtual UIType Type => UIType.None;

        /// <summary>
        /// 资源是否准备完毕。
        /// </summary>
        public bool IsPrepare { protected set; get; }

        /// <summary>
        /// UI子组件列表。
        /// </summary>
        public List<UIWidget> ListChild = new List<UIWidget>();

        /// <summary>
        /// 存在Update更新的UI子组件列表。
        /// </summary>
        protected List<UIWidget> m_listUpdateChild = null;

        /// <summary>
        /// 是否持有Update行为。
        /// </summary>
        protected bool m_updateListValid = false;

        protected UIBaseLogic m_UILogic;
        private bool m_HasUILogic;

        /// <summary>
        /// 代码自动生成绑定。
        /// </summary>
        protected virtual void ScriptGenerator()
        {
        }

        /// <summary>
        /// 绑定UI成员元素。
        /// </summary>
        protected virtual void BindMemberProperty()
        {
        }

        /// <summary>
        /// 注册事件。
        /// </summary>
        protected virtual void RegisterEvent()
        {
        }

        /// <summary>
        /// 窗口创建。
        /// </summary>
        protected virtual void OnCreate()
        {
            m_UILogic = gameObject.GetComponent<UIBaseLogic>();
            if (m_UILogic != null)
            {
                m_UILogic.OnInit(this);
                m_HasUILogic = true;
            }
            else
            {
                m_HasUILogic = false;
            }
        }

        /// <summary>
        /// 窗口刷新
        /// </summary>
        protected virtual void OnRefresh()
        {
            if (m_HasUILogic)
            {
                m_UILogic.OnOpen();
            }
        }

        /// <summary>
        /// 是否需要Update
        /// </summary>
        protected bool HasOverrideUpdate = true;

        /// <summary>
        /// 窗口更新
        /// </summary>
        protected virtual void OnUpdate()
        {
            HasOverrideUpdate = false;

            if (m_HasUILogic)
            {
                m_UILogic.OnUpdate();
            }
        }

        internal void CallDestroy()
        {
            OnDestroy();
        }

        /// <summary>
        /// 窗口销毁
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_HasUILogic)
            {
                m_UILogic.OnClose();
            }
        }

        /// <summary>
        /// 当触发窗口的层级排序。
        /// </summary>
        protected virtual void OnSortDepth(int depth)
        {
            if (m_HasUILogic)
            {
                m_UILogic.OnSortDepth(depth);
            }
        }

        /// <summary>
        /// 当因为全屏遮挡触或者窗口可见性触发窗口的显隐。
        /// </summary>
        protected virtual void OnSetVisible(bool visible)
        {
        }

        internal void SetUpdateDirty()
        {
            m_updateListValid = false;
            if (Parent != null)
            {
                Parent.SetUpdateDirty();
            }
        }

        #region FindChildComponent

        public Transform FindChild(string path)
        {
            return DUnityUtil.FindChild(rectTransform, path);
        }

        public Transform FindChild(Transform trans, string path)
        {
            return DUnityUtil.FindChild(trans, path);
        }

        public T FindChildComponent<T>(string path) where T : Component
        {
            return DUnityUtil.FindChildComponent<T>(rectTransform, path);
        }

        public T FindChildComponent<T>(Transform trans, string path) where T : Component
        {
            return DUnityUtil.FindChildComponent<T>(trans, path);
        }

        #endregion

        #region UIEvent

        private GameEventMgr _eventMgr;

        protected GameEventMgr EventMgr
        {
            get
            {
                if (_eventMgr == null)
                {
                    _eventMgr = ReferencePool.Acquire<GameEventMgr>();
                }

                return _eventMgr;
            }
        }

        public void AddUIEvent(int eventType, Action handler)
        {
            EventMgr.AddEvent(eventType, handler);
        }

        public void AddUIEvent<T>(int eventType, Action<T> handler)
        {
            EventMgr.AddEvent(eventType, handler);
        }

        public void AddUIEvent<T, U>(int eventType, Action<T, U> handler)
        {
            EventMgr.AddEvent(eventType, handler);
        }

        public void AddUIEvent<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            EventMgr.AddEvent(eventType, handler);
        }

        public void AddUIEvent<T, U, V, W>(int eventType, Action<T, U, V, W> handler)
        {
            EventMgr.AddEvent(eventType, handler);
        }

        protected void RemoveAllUIEvent()
        {
            if (_eventMgr != null)
            {
                ReferencePool.Release(_eventMgr);
            }
        }

        #endregion

        #region UIWidget

        /// <summary>
        /// 创建UIWidget通过父UI位置节点。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="goPath">父UI位置节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(string goPath, bool visible = true) where T : UIWidget, new()
        {
            var goRootTrans = FindChild(goPath);

            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }


        /// <summary>
        /// 创建UIWidget通过父UI位置节点。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="parentTrans"></param>
        /// <param name="goPath">父UI位置节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(Transform parentTrans, string goPath, bool visible = true) where T : UIWidget, new()
        {
            var goRootTrans = FindChild(parentTrans, goPath);
            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }

        /// <summary>
        /// 创建UIWidget通过游戏物体。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="goRoot">游戏物体。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(GameObject goRoot, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (widget.Create(this, goRoot, visible))
            {
                return widget;
            }

            return null;
        }

        /// <summary>
        /// 创建UIWidget通过资源定位地址。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByPath<T>(Transform parentTrans, string assetLocation, bool visible = true) where T : UIWidget, new()
        {
            GameObject goInst = GameModule.Resource.LoadGameObject(assetLocation, parent: parentTrans);
            return CreateWidget<T>(goInst, visible);
        }

        /// <summary>
        /// 创建UIWidget通过资源定位地址。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public async UniTask<T> CreateWidgetByPathAsync<T>(Transform parentTrans, string assetLocation, bool visible = true) where T : UIWidget, new()
        {
            GameObject goInst = await GameModule.Resource.LoadGameObjectAsync(assetLocation, parentTrans, gameObject.GetCancellationTokenOnDestroy());
            return CreateWidget<T>(goInst, visible);
        }

        /// <summary>
        /// 根据prefab或者模版来创建新的 widget。
        /// </summary>
        /// <param name="goPrefab">资源创建副本。</param>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByPrefab<T>(GameObject goPrefab, Transform parentTrans = null, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (!widget.CreateByPrefab(this, goPrefab, parentTrans, visible))
            {
                return null;
            }

            return widget;
        }

        /// <summary>
        /// 通过UI类型来创建widget。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByType<T>(Transform parentTrans, bool visible = true) where T : UIWidget, new()
        {
            return CreateWidgetByPath<T>(parentTrans, typeof(T).Name, visible);
        }

        /// <summary>
        /// 通过UI类型来创建widget。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public async UniTask<T> CreateWidgetByTypeAsync<T>(Transform parentTrans, bool visible = true) where T : UIWidget, new()
        {
            return await CreateWidgetByPathAsync<T>(parentTrans, typeof(T).Name, visible);
        }

        /// <summary>
        /// 调整图标数量。
        /// </summary>
        /// <remarks>常用于Item创建。</remarks>
        /// <param name="itemList">存放Item的列表。</param>
        /// <param name="number">创建数目。</param>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="prefab">资产副本。</param>
        /// <param name="assetPath">资产地址。</param>
        /// <typeparam name="T">图标类型。</typeparam>
        public void AdjustItemNum<T>(List<T> itemList, int number, Transform parentTrans, GameObject prefab = null, string assetPath = "")
            where T : UIWidget, new()
        {
            if (itemList == null)
            {
                itemList = new List<T>();
            }

            if (itemList.Count < number)
            {
                int needNum = number - itemList.Count;
                for (int itemIdx = 0; itemIdx < needNum; itemIdx++)
                {
                    T tmpT = prefab == null ? CreateWidgetByType<T>(parentTrans) : CreateWidgetByPrefab<T>(prefab, parentTrans);
                    itemList.Add(tmpT);
                }
            }
            else if (itemList.Count > number)
            {
                RemoveUnUseItem<T>(itemList, number);
            }
        }

        /// <summary>
        /// 异步调整Item数量。
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="tarNum"></param>
        /// <param name="parentTrans"></param>
        /// <param name="prefab"></param>
        /// <param name="assetPath"></param>
        /// <param name="maxNumPerFrame"></param>
        /// <param name="updateAction"></param>
        /// <typeparam name="T"></typeparam>
        public void AsyncAdjustItemNum<T>(List<T> itemList, int tarNum, Transform parentTrans, GameObject prefab = null,
            string assetPath = "", int maxNumPerFrame = 5,
            Action<T, int> updateAction = null) where T : UIWidget, new()
        {
            AsyncAdjustItemNumInternal(itemList, tarNum, parentTrans, maxNumPerFrame, updateAction, prefab, assetPath).Forget();
        }

        /// <summary>
        /// 异步创建接口。
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="tarNum"></param>
        /// <param name="parentTrans"></param>
        /// <param name="maxNumPerFrame"></param>
        /// <param name="updateAction"></param>
        /// <param name="prefab"></param>
        /// <param name="assetPath"></param>
        /// <typeparam name="T"></typeparam>
        private async UniTaskVoid AsyncAdjustItemNumInternal<T>(List<T> itemList, int tarNum, Transform parentTrans, int maxNumPerFrame,
            Action<T, int> updateAction, GameObject prefab, string assetPath) where T : UIWidget, new()
        {
            if (itemList == null)
            {
                itemList = new List<T>();
            }

            int createCnt = 0;

            for (int i = 0; i < tarNum; i++)
            {
                T tmpT = null;
                if (i < itemList.Count)
                {
                    tmpT = itemList[i];
                }
                else
                {
                    if (prefab == null)
                    {
                        tmpT = await CreateWidgetByPathAsync<T>(parentTrans, assetPath);
                    }
                    else
                    {
                        tmpT = CreateWidgetByPrefab<T>(prefab, parentTrans);
                    }

                    itemList.Add(tmpT);
                }

                int index = i;
                if (updateAction != null)
                {
                    updateAction(tmpT, index);
                }

                createCnt++;
                if (createCnt >= maxNumPerFrame)
                {
                    createCnt = 0;
                    await UniTask.Yield();
                }
            }

            if (itemList.Count > tarNum)
            {
                RemoveUnUseItem(itemList, tarNum);
            }
        }

        private void RemoveUnUseItem<T>(List<T> itemList, int tarNum) where T : UIWidget
        {
            var removeList = new List<T>();
            for (int itemIdx = 0; itemIdx < itemList.Count; itemIdx++)
            {
                var item = itemList[itemIdx];
                if (itemIdx >= tarNum)
                {
                    removeList.Add(item);
                }
            }

            for (var index = 0; index < removeList.Count; index++)
            {
                var item = removeList[index];
                itemList.Remove(item);
                item.OnDestroy();
                item.OnDestroyWidget();
                ListChild.Remove(item);
                if (item.gameObject != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }

        #endregion
    }
}