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

                    instance = (T)FindFirstObjectByType(t);
                    if (instance == null)
                    {
                        instance = Create();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// ゲームオブジェクトを生成する
        /// </summary>
        private static T Create()
        {
            var gameObj = new GameObject(typeof(T).Name);
            return gameObj.AddComponent<T>();
        }

        /// <summary>
        /// Start() メソッドは他のコンポーネントが初期化された後で使いたい内容を記述する場所
        /// Instance プロパティは FindFirstObjectByType を利用しており、他コンポーネントの初期化を待ったほうがよい
        /// よって、バリデーションの処理は Awake() ではなく Start() で検証することにした
        /// </summary>
        virtual protected void Start()
        {
            Validate();
        }

        virtual protected void Validate()
        {
            // 他のGameObjectにアタッチされているか調べる。
            // アタッチされている場合は破棄して例外を発生する。
            if (this != Instance)
            {
                Destroy(this);
                throw new SingletonMonoBehaviourException(typeof(T) +
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

    public class SingletonMonoBehaviourException : Exception
    {
        public SingletonMonoBehaviourException(string message) : base(message) { }
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
