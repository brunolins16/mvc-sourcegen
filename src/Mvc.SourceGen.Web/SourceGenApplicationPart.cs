using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Mvc.SourceGen.Web.Controllers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mvc.SourceGen.Web;

public partial class SourceGenApplicationPart : ApplicationPart
{
    /// <summary>
    /// Gets the name of the <see cref="ApplicationPart"/>.
    /// </summary>
    public override string Name => "Mvc.SourceGen"!;
}