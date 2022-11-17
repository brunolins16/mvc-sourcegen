namespace Mvc.SourceGen
{
    using System.Reflection;
    using static Microsoft.AspNetCore.Mvc.ApplicationModels.ActionModel;

    public abstract class ControllerInfo
    {
        public abstract ControllerActionInfo[] Actions { get; }
        public abstract PropertyInfo[] Properties { get; }
    }

    public struct ControllerActionInfo
    {
        public ControllerActionInfo(MethodInfo method, Func<object, object?[]?, object> methodInvoker)
        {
            Method = method;
            MethodInvoker = methodInvoker;
        }

        public ControllerActionInfo(MethodInfo method, Func<object, object?[]?, object> methodInvoker, ActionAwaitableInfo? methodAwaitableInfo)
            : this(method, methodInvoker)
        {
            MethodAwaitableInfo = methodAwaitableInfo;
        }

        public MethodInfo Method { get; set; }
        public Func<object, object?[]?, object> MethodInvoker { get; set; }
        public ActionAwaitableInfo? MethodAwaitableInfo { get; set; }
    }

}
