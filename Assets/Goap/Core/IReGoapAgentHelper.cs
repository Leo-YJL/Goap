
using System;

namespace ReGoap.Core {
    /// <summary>
    /// 仅仅是为了在unity使用与GetComponet类似方法获取通用的参数的接口
    /// </summary>
    public interface IReGoapAgentHelper  {

        Type[] GetGenericArguments();
    }
}