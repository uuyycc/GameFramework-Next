using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using YooAsset;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public class ResourceComponent : GameFrameworkComponent
    {
        #region Propreties

        private const int DefaultPriority = 0;

        /// <summary>
        /// 当前最新的包裹版本。
        /// </summary>
        public string PackageVersion { set; get; }

        private IResourceManager m_ResourceManager;
        private bool m_ForceUnloadUnusedAssets = false;
        private bool m_PreorderUnloadUnusedAssets = false;
        private bool m_PerformGCCollect = false;
        private AsyncOperation m_AsyncOperation = null;
        private float m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;

        [SerializeField] private float m_MinUnloadUnusedAssetsInterval = 60f;

        [SerializeField] private float m_MaxUnloadUnusedAssetsInterval = 300f;

        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string PackageName = "DefaultPackage";

        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        /// <summary>
        /// 下载文件校验等级。
        /// </summary>
        public EVerifyLevel VerifyLevel = EVerifyLevel.Middle;

        [SerializeField] private ReadWritePathType m_ReadWritePathType = ReadWritePathType.Unspecified;

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        [SerializeField] public long Milliseconds = 30;

        public int m_DownloadingMaxNum = 2;

        /// <summary>
        /// 获取或设置同时最大下载数目。
        /// </summary>
        public int DownloadingMaxNum
        {
            get { return m_DownloadingMaxNum; }
            set { m_DownloadingMaxNum = value; }
        }

        public int m_FailedTryAgain = 3;

        public int FailedTryAgain
        {
            get { return m_FailedTryAgain; }
            set { m_FailedTryAgain = value; }
        }

        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get { return m_ResourceManager.ApplicableGameVersion; }
        }

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get { return m_ResourceManager.InternalResourceVersion; }
        }

        /// <summary>
        /// 获取资源读写路径类型。
        /// </summary>
        public ReadWritePathType ReadWritePathType
        {
            get { return m_ReadWritePathType; }
        }

        /// <summary>
        /// 获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        public float MinUnloadUnusedAssetsInterval
        {
            get { return m_MinUnloadUnusedAssetsInterval; }
            set { m_MinUnloadUnusedAssetsInterval = value; }
        }

        /// <summary>
        /// 获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        public float MaxUnloadUnusedAssetsInterval
        {
            get { return m_MaxUnloadUnusedAssetsInterval; }
            set { m_MaxUnloadUnusedAssetsInterval = value; }
        }

        /// <summary>
        /// 获取无用资源释放的等待时长，以秒为单位。
        /// </summary>
        public float LastUnloadUnusedAssetsOperationElapseSeconds
        {
            get { return m_LastUnloadUnusedAssetsOperationElapseSeconds; }
        }

        /// <summary>
        /// 获取资源只读路径。
        /// </summary>
        public string ReadOnlyPath
        {
            get { return m_ResourceManager.ReadOnlyPath; }
        }

        /// <summary>
        /// 获取资源读写路径。
        /// </summary>
        public string ReadWritePath
        {
            get { return m_ResourceManager.ReadWritePath; }
        }

        #endregion

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("YooAssetsManager component is invalid.");
                return;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
#if !UNITY_EDITOR
                PlayMode = EPlayMode.OfflinePlayMode;
#endif
            }

            m_ResourceManager.SetReadOnlyPath(Application.streamingAssetsPath);
            if (m_ReadWritePathType == ReadWritePathType.TemporaryCache)
            {
                m_ResourceManager.SetReadWritePath(Application.temporaryCachePath);
            }
            else
            {
                if (m_ReadWritePathType == ReadWritePathType.Unspecified)
                {
                    m_ReadWritePathType = ReadWritePathType.PersistentData;
                }

                m_ResourceManager.SetReadWritePath(Application.persistentDataPath);
            }

            m_ResourceManager.DefaultPackageName = PackageName;
            m_ResourceManager.PlayMode = PlayMode;
            m_ResourceManager.VerifyLevel = VerifyLevel;
            m_ResourceManager.Milliseconds = Milliseconds;
            m_ResourceManager.InstanceRoot = transform;
            m_ResourceManager.HostServerURL = SettingsUtils.GetResDownLoadPath();
            m_ResourceManager.Initialize();
            Log.Info($"AssetsComponent Run Mode：{PlayMode}");
        }

        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <returns></returns>
        public void InitPackage()
        {
            m_ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("YooAssetsManager component is invalid.");
                return;
            }

            m_ResourceManager.InitPackage(PackageName);
        }

        #region 加载资源

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAssetAsync(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData = null)
        {
            LoadAssetAsync(assetName, assetType, DefaultPriority, loadAssetCallbacks, userData);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAssetAsync(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return;
            }

            m_ResourceManager.LoadAssetAsync(assetName, assetType, priority, loadAssetCallbacks, userData);
        }

        #endregion

        #region 卸载资源

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (asset == null)
            {
                throw new GameFrameworkException("Asset is invalid.");
            }
        }

        #endregion

        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            m_ForceUnloadUnusedAssets = true;
            if (performGCCollect)
            {
                m_PerformGCCollect = true;
            }
        }

        public void ClearSandbox()
        {
        }

        private void Update()
        {
            m_LastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (m_AsyncOperation == null && (m_ForceUnloadUnusedAssets || m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MaxUnloadUnusedAssetsInterval ||
                                             m_PreorderUnloadUnusedAssets && m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MinUnloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                m_ForceUnloadUnusedAssets = false;
                m_PreorderUnloadUnusedAssets = false;
                m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                m_AsyncOperation = Resources.UnloadUnusedAssets();
            }

            if (m_AsyncOperation is { isDone: true })
            {
                m_ResourceManager.UnloadUnusedAssets();
                m_AsyncOperation = null;
                if (m_PerformGCCollect)
                {
                    Log.Info("GC.Collect...");
                    m_PerformGCCollect = false;
                    GC.Collect();
                }
            }
        }
    }
}