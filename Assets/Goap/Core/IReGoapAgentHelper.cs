using System;

// interface needed only in Unity to use GetComponent and such features for generic agents
namespace ReGoap.Core
{
    /// <summary>
    /// 仅仅是为了在Unity使用与GetComponent类似方式获取通用的参数的接口
    /// </summary>
    public interface IReGoapAgentHelper
    {
        Type[] GetGenericArguments();
    }
}
