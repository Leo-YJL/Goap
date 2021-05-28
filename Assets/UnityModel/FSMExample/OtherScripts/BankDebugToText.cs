using UnityEngine;
using UnityEngine.UI;

namespace ReGoap.Unity.FSMExample.OtherScripts {
    /// <summary>
    /// 用来Debug显示的Text
    /// </summary>
    public class BankDebugToText : MonoBehaviour {
        /// <summary>
        /// 要显示的文本
        /// </summary>
        public Text Text;

        /// <summary>
        /// Bank
        /// </summary>
        private Bank bank;

        void Awake() {
            bank = GetComponent<Bank>();
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        void FixedUpdate() {
            var result = "";
            foreach (var pair in bank.GetResources()) {
                result += string.Format("{0}: {1}\n", pair.Key, pair.Value);
            }
            Text.text = result;
        }
    }
}
