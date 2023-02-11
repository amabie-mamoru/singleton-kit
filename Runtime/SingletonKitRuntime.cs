using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.amabie.SingletonKit {
    /// <summary>
    /// MonoBehaviour 用 Singleton(厳密にはシングルトンではない)
    /// Unique であることを他のクラスを使って保証する
    /// 
    /// SingletonMonoBehaviour は厳密にシングルトンであるため、初期化時に使うことを前提とすればチームで利用する場合はいまだに検討の余地はある
    /// 一方で、内部で利用されている FindObjectOfType は GameObject を全操作するためシーンのオブジェクトが増えるたびに低速になる
    /// see: https://baba-s.hatenablog.com/entry/2014/07/09/093240
    /// また、公式でも this function is very slow と謳っている
    /// see: https://docs.unity3d.com/ja/2018.4/ScriptReference/Object.FindObjectOfType.html
    /// よって、Uniqueであることを他のクラスを使いながら保証する
    /// シングルトンではないので、
    /// </summary>
    public abstract class UniqueMonoBehaviour<T> : UniqueBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Type t = typeof(T);
                    var gameObj = new GameObject(t.Name);
                    instance = gameObj.AddComponent<T>();
                }
                return instance;
            }
        }

        /// <summary>
        /// インスタンス生成後戻り値が欲しくない場合に利用する
        /// </summary>
        public virtual void Create() { }

        /// <summary>
        /// GameObject をシーン跨ぎでも破棄しないように永久化する
        /// </summary>
        public void Permanent()
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public class UniqueBehaviour : MonoBehaviour
    {
    }

    /// <summary>
    /// UniqueMonoBehaviour を検査するための機構
    /// シーンの Awake 実行時に生成しておくと、シーン中の UniqueMonoBehaviour を全操作してシングルトンであることを擬似的に保証する
    /// そのため、Update などシーンの途中で UniqueMonoBehaviour が生成された場合はユニークであることを保証できない
    /// また、全操作するためめちゃくちゃ遅い
    /// 使う場合は #if UNITY_EDITOR などでチェックしたい場合のみに限定して利用する
    /// </summary>
    public class UniqueMonoBehaviourValidator : UniqueMonoBehaviour<UniqueMonoBehaviourValidator>
    {
        private List<Type> types;
        /// <summary>
        /// Awake で UniqueMonoBehaviour が生成される前提で検査は Start で行う
        /// </summary>
        protected void Start()
        {
            types = new List<Type>();
            Validate();
        }

        private void Validate()
        {
            var objects = FindObjectsOfType<UniqueBehaviour>();
            foreach(var obj in objects)
            {
                Type objType = obj.GetType();
                if (types.Contains(objType))
                {
                    throw new SingletonKitException(objType + "が重複しています。スクリプトとシーンヒエラルキーを合わせて2つ以上定義されているため、どちらかを削除してください。");
                }
                types.Add(objType);
            }
        }
    }

    /// <summary>
    /// MonoBehaviour 用 Singleton
    /// </summary>
    [Obsolete("速度に問題があります。UniqueMonoBehaviourの利用を検討してください。")]
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
                        throw new SingletonKitException(t + " をアタッチしている GameObject がありません。");
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            // 他のGameObjectにアタッチされているか調べる。
            // アタッチされている場合は破棄して例外を発生する。
            if (this != Instance)
            {
                Destroy(this);
                throw new SingletonKitException(typeof(T) +
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

    public class SingletonKitException : Exception
    {
        public SingletonKitException(string message) : base (message) { }
    }
}