using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Ok here")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "No need in .net core apps")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Do not need to comment as they are used only for [de]serialization")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Scope = "namespaceanddescendants", Justification = "Keeping all related classes in one file. These files are used only for for [de]serialization")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Ok here")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validation is performed by Guard class")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Ok here")]
[assembly: SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Ok here, type is not used for comparison")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Ok here")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "Ok here")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "Ok here")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1413:Use trailing comma in multi-line initializers", Justification = "Ok")]