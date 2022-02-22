#nullable enable

using System;
using System.Collections;
using UnityEngine;

namespace Edanoue.EH.Scene
{
    public class MultiSceneLoadingToken
    {
        private SceneLoadingToken[] m_sceneLoadingTokens;

        public MultiSceneLoadingToken(SceneLoadingToken[] sceneLoadingTokens)
        {
            m_sceneLoadingTokens = sceneLoadingTokens;
        }

        internal void Update()
        {
            foreach (var token in m_sceneLoadingTokens)
            {
                token.Update();
            }
        }

        internal IEnumerator PreLoad()
        {
            while (!IsPreLoaded)
            {
                Update();
                yield return null;
            }
        }

        public bool IsPreLoaded
        {
            get
            {
                foreach (var token in m_sceneLoadingTokens)
                {
                    if (!token.IsPreLoaded)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool IsCompleted
        {
            get
            {
                foreach (var token in m_sceneLoadingTokens)
                {
                    if (!token.IsCompleted)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void Complete()
        {
            foreach (var token in m_sceneLoadingTokens)
            {
                token.Complete();
            }
        }
    }

    /// <summary>
    /// シーンロードの 進行状態を保持するクラス
    /// </summary>
    public class SceneLoadingToken
    {
        private float m_progress = -1f;
        private readonly AsyncOperation m_asyncOperation;
        private bool m_calledEventLoaded = false;

        internal SceneLoadingToken(AsyncOperation asyncOperation)
        {
            m_asyncOperation = asyncOperation;
            m_asyncOperation.completed += OnCompletedAsyncOperation;
        }

        #region API

        public AsyncOperation AsyncOperation => m_asyncOperation;
        public float Progress => m_progress;
        public bool IsPreLoaded => m_progress >= 0.9f;
        public bool IsCompleted => m_asyncOperation.isDone;

        internal void Update()
        {
            if (m_progress < m_asyncOperation.progress)
            {
                m_progress = m_asyncOperation.progress;
                ChangedProgress?.Invoke(m_progress);

                if (!m_calledEventLoaded && IsPreLoaded)
                {
                    PreLoaded?.Invoke();
                    m_calledEventLoaded = true;
                }
            }
        }

        public void Complete()
        {
            if (IsPreLoaded && !IsCompleted)
            {
                m_asyncOperation.allowSceneActivation = true;
            }
        }

        #endregion

        #region events

        public event Action<float>? ChangedProgress;
        public event Action? PreLoaded;
        public event Action? Completed;

        #endregion

        private void OnCompletedAsyncOperation(AsyncOperation _)
        {
            Completed?.Invoke();
        }
    }

}
