#nullable enable
namespace Mvc.SourceGen
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal partial class MvcSourceGenContext : Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext
    {
        static MvcSourceGenContext()
        {
            _types = new System.Reflection.TypeInfo[1]{GetTypeInfo<TodoApp.Controllers.TodosController>()};
        }

        public bool TryGetControllerInfo(System.Type controllerType, out Microsoft.AspNetCore.Mvc.Abstractions.ControllerInfo? controllerInfo)
        {
            controllerInfo = null;
            if (typeof(TodoApp.Controllers.TodosController).IsAssignableFrom(controllerType))
            {
                controllerInfo = TodoAppControllersTodosControllerInfo.Instance;
                return true;
            }

            return controllerInfo != null;
        }

        private readonly static System.Reflection.TypeInfo[] _types;
        public System.Collections.Generic.IEnumerable<System.Reflection.TypeInfo> ControllerTypes => _types;
        private static System.Reflection.TypeInfo GetTypeInfo<
        [DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
        T>() => typeof(T).GetTypeInfo();
    }

    public class TodoAppControllersTodosControllerInfo : Microsoft.AspNetCore.Mvc.Abstractions.ControllerInfo
    {
        private static readonly Microsoft.AspNetCore.Mvc.Abstractions.ControllerActionInfo[] _actions;
        private static readonly System.Reflection.PropertyInfo[] _properties;
        static TodoAppControllersTodosControllerInfo()
        {
            _properties = new System.Reflection.PropertyInfo[0];
            _actions = new Microsoft.AspNetCore.Mvc.Abstractions.ControllerActionInfo[4];
            _actions[0] = new(typeof(TodoApp.Controllers.TodosController).GetMethod("GetAll", new[]{typeof(TodoApp.Controllers.TodoFilterRequest)}), static (target, parameters) => ((TodoApp.Controllers.TodosController)target).GetAll((TodoApp.Controllers.TodoFilterRequest)parameters[0]), new(typeof(TodoApp.Controllers.Todo[]), static (target) => ((System.Threading.Tasks.Task<TodoApp.Controllers.Todo[]>)target).GetAwaiter(), static (target) => ((System.Runtime.CompilerServices.TaskAwaiter<TodoApp.Controllers.Todo[]>)target).IsCompleted, static (target) => ((System.Runtime.CompilerServices.TaskAwaiter<TodoApp.Controllers.Todo[]>)target).GetResult(), static (target, action) => ((System.Runtime.CompilerServices.INotifyCompletion)target).OnCompleted(action), static (target, action) => ((System.Runtime.CompilerServices.ICriticalNotifyCompletion)target).UnsafeOnCompleted(action)));
            _actions[1] = new(typeof(TodoApp.Controllers.TodosController).GetMethod("Get", new[]{typeof(int)}), static (target, parameters) => ((TodoApp.Controllers.TodosController)target).Get((int)parameters[0]), null);
            _actions[2] = new(typeof(TodoApp.Controllers.TodosController).GetMethod("Post", new[]{typeof(TodoApp.Controllers.NewTodo)}), static (target, parameters) => ((TodoApp.Controllers.TodosController)target).Post((TodoApp.Controllers.NewTodo)parameters[0]), null);
            _actions[3] = new(typeof(TodoApp.Controllers.TodosController).GetMethod("Post", new[]{typeof(System.Collections.Generic.List<TodoApp.Controllers.NewTodo>)}), static (target, parameters) => ((TodoApp.Controllers.TodosController)target).Post((System.Collections.Generic.List<TodoApp.Controllers.NewTodo>)parameters[0]), null);
        }

        public static TodoAppControllersTodosControllerInfo Instance { get; } = new();
        public override Microsoft.AspNetCore.Mvc.Abstractions.ControllerActionInfo[] Actions => _actions;
        public override System.Reflection.PropertyInfo[] Properties => _properties;
    }
}