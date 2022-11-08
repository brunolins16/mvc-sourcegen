namespace Mvc.SourceGen
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal partial class MvcSourceGenContext : ISourceGenControllerTypeProvider
    {
        private readonly static TypeInfo[] _types;
        static MvcSourceGenContext()
        {
            _types = new TypeInfo[2]{GetTypeInfo<Mvc.SourceGen.Web.Controllers.TodosController>(), GetTypeInfo<Mvc.SourceGen.Web.Controllers.PlaygroundController>()};
        }

        public IEnumerable<TypeInfo> ControllerTypes => _types;

        private static TypeInfo GetTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)]T>() 
            => typeof(T).GetTypeInfo();
    }
}