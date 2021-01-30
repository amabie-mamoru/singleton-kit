using System;
using UnityEngine;

namespace com.amabie.SingletonKit {
    /// <summary>
    /// MonoBehaviour 用 Singleton
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Type t = typeof(T);

                    instance = (T)FindObjectOfType(t);
                    if (instance == null)
                    {
                        throw new Exception(t + " をアタッチしている GameObject がありません。");
                    }
                }

                return instance;
            }
        }

        virtual protected void Awake()
        {
            // 他のGameObjectにアタッチされているか調べる。
            // アタッチされている場合は破棄して例外を発生する。
            if (this != Instance)
            {
                Destroy(this);
                throw new Exception(typeof(T) +
                    " は既に他の GameObject にアタッチされているため、コンポーネントを破棄します。" +
                    " アタッチされている GameObject は " + Instance.gameObject.name + " です。");
            }
        }

        /// <summary>
        /// GameObject をシーン跨ぎでも破棄しないように永久化する
        /// </summary>
        public void Permanent()
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public abstract class Singleton<T> where T : class, new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}