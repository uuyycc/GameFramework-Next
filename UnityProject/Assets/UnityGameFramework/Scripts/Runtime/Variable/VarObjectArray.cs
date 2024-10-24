using GameFramework;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// System.Char 数组变量类。
    /// </summary>
    public sealed class VarObjectArray : Variable<object[]>
    {
        public VarObjectArray()
        {
        }

        public static implicit operator VarObjectArray(object[] value)
        {
            VarObjectArray varValue = ReferencePool.Acquire<VarObjectArray>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator object[](VarObjectArray value)
        {
            return value.Value;
        }
    }
}
