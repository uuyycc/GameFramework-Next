﻿using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace GameLogic
{
    public class UILoadUpdate : UIBase
    {
        [SerializeField] public Button _btn_clear;
        [SerializeField] public Scrollbar _obj_progress;
        [SerializeField] public Text _label_desc;
        [SerializeField] public Text _label_appid;
        [SerializeField] public Text _label_resid;

        public virtual void Start()
        {
            _btn_clear.onClick.AddListener(OnClear);
            _btn_clear.gameObject.SetActive(true);
        }

        public virtual void OnEnable()
        {
            RefreshVersion();
            GameEvent.AddEventListener<float>(StringId.StringToHash("DownProgress"),DownLoad_Progress_Action);
        }

        public override void OnEnter(object param)
        {
            base.OnEnter(param);
            _label_desc.text = param.ToString();
            RefreshVersion();
        }

        public virtual void Update()
        {
        }

        private void RefreshVersion()
        {
            _label_appid.text = string.Format(LoadText.Instance.Label_App_id, Version.GameVersion);
            _label_resid.text = string.Format(LoadText.Instance.Label_Res_id, GameModule.Resource.PackageVersion);
        }

        public virtual void OnContinue(GameObject obj)
        {
            // LoadMgr.Instance.StartDownLoad();
        }

        public virtual void OnStop(GameObject obj)
        {
            // LoadMgr.Instance.StopDownLoad();
        }

        /// <summary>
        /// 清空本地缓存
        /// </summary>
        public virtual void OnClear()
        {
            OnStop(null);
            UILoadTip.ShowMessageBox(LoadText.Instance.Label_Clear_Comfirm, MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Clear,
                () =>
                {
                    GameModule.Resource.ClearUnusedCacheFilesAsync();
                    Application.Quit();
                }, () => { OnContinue(null); });
        }

        /// <summary>
        /// 下载进度完成
        /// </summary>
        /// <param name="type"></param>
        public virtual void DownLoad_Complete_Action(int type)
        {
            Log.Info("DownLoad_Complete");
        }

        /// <summary>
        /// 下载进度更新
        /// </summary>
        /// <param name="progress"></param>
        public virtual void DownLoad_Progress_Action(float progress)
        {
            _obj_progress.gameObject.SetActive(true);

            _obj_progress.size = progress;
        }

        /// <summary>
        /// 解压缩完成回调
        /// </summary>
        /// <param name="type"></param>
        /// <param name="status"></param>
        public virtual void Unpacked_Complete_Action(bool type, GameStatus status)
        {
            _obj_progress.gameObject.SetActive(true);
            _label_desc.text = LoadText.Instance.Label_Load_UnpackComplete;
            if (status == GameStatus.AssetLoad)
            {
            }
            else
            {
                Log.Error("error type");
            }
        }

        /// <summary>
        /// 解压缩进度更新
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="status"></param>
        public virtual void Unpacked_Progress_Action(float progress, GameStatus status)
        {
            _obj_progress.gameObject.SetActive(true);
            if (status == GameStatus.First)
            {
                _label_desc.text = LoadText.Instance.Label_Load_FirstUnpack;
            }
            else
            {
                _label_desc.text = LoadText.Instance.Label_Load_Unpacking;
            }

            _obj_progress.size = progress;
        }

        public virtual void OnDisable()
        {
            GameEvent.RemoveEventListener<float>(StringId.StringToHash("DownProgress"),DownLoad_Progress_Action);
            OnStop(null);
        }
    }
}