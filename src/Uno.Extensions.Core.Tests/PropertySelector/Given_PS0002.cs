﻿using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Extensions.Core.Tests.Utils;

namespace Uno.Extensions.Core.Tests.PropertySelector;

[TestClass]
public class Given_PS0002
{
	[TestMethod]
	public async Task When_UsingExternalVariable()
	{
		var compilation = GenerationTestHelper.CreateCompilationWithAnalyzers($@"
			using Uno.Extensions.Edition;
			using System.Runtime.CompilerServices;

			namespace TestNamespace;

			public record Entity(string Value);

			public class SUTClass
			{{
				public void Test()
				{{
					var external = new Entity(""hello world"");
					SUTMethod(e => external.Value);
				}}

				public void SUTMethod(PropertySelector<Entity, string> selector, [CallerFilePath] string path = """", [CallerLineNumber] int line = -1)
				{{
				}}
			}}
			");

		var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
		diagnostics.Length.Should().Be(2);

		var constructableDiag = diagnostics[0];
		constructableDiag.Id.Should().Be("PS0006");
		constructableDiag.Location.GetLineSpan().StartLinePosition.Line.Should().Be(13);
		constructableDiag.Location.GetLineSpan().StartLinePosition.Character.Should().Be(20);

		var pathDiag = diagnostics[1];
		pathDiag.Id.Should().Be("PS0002");
		pathDiag.Location.GetLineSpan().StartLinePosition.Line.Should().Be(13);
		pathDiag.Location.GetLineSpan().StartLinePosition.Character.Should().Be(20);

		var expectedCode = @"//----------------------
// <auto-generated>
//	Generated by the PropertySelectorsGenerationTool v1. DO NOT EDIT!
//	Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//----------------------
#pragma warning disable

namespace Tests.__PropertySelectors
{
	/// <summary>
	/// Auto registration class for PropertySelector used in <global namespace>.
	/// </summary>
	[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
	internal static class _14_5
	{
		/// <summary>
		/// Register the value providers for the PropertySelectors used to invoke 'SUTMethod'
		/// in  on line 14.
		/// </summary>
		/// <remarks>
		/// This method is flagged with the [ModuleInitializerAttribute] which means that it will be invoked by the runtime when the module is being loaded.
		/// You should not have to use it at any time.
		/// </remarks>
		[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
		[global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
		internal static void Register()
		{
			global::Uno.Extensions.Edition.PropertySelectors.Register(
				@""selector14"",
				new global::Uno.Extensions.Edition.ValueAccessor<TestNamespace.Entity, string>(
				"".Value"",
				entity => entity.Value,
				(current, updated_Value) => (current /* Rule PS006 failed. */) with { Value = updated_Value }));
		}
	}
}";
		var (result1, result2) = GenerationTestHelper.RunGeneratorTwice(compilation.Compilation);
		GenerationTestHelper.AssertGeneratorResult(result1, expectedCode, IncrementalStepRunReason.New);
		GenerationTestHelper.AssertGeneratorResult(result2, expectedCode, IncrementalStepRunReason.Cached);
	}
}
