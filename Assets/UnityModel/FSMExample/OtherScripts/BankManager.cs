using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts {
    /// <summary>
    /// 银行管理者
    /// </summary>
    public class BankManager : MonoBehaviour {
        /// <summary>
        /// 单例
        /// </summary>
        public static BankManager Instance;
        /// <summary>
        /// 所有银行
        /// </summary>
        public Bank[] Banks;
        /// <summary>
        /// 当前的值
        /// </summary>
        public int currentIndex;

        protected virtual void Awake() {
            if (Instance != null)
                throw new UnityException("[BankManager] Can have only one instance per scene.");
            Instance = this;
        }
        /// <summary>
        /// 获取当前银行
        /// </summary>
        public Bank GetBank() {
            var result = Banks[currentIndex];
            currentIndex = currentIndex++ % Banks.Length;
            return result;
        }
        /// <summary>
        /// 获取银行数目
        /// </summary>
        /// <returns></returns>
        public int GetBanksCount() {
            return Banks.Length;
        }
    }
}