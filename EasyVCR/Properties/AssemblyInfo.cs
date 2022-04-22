using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EasyVCR")]
[assembly: AssemblyDescription("A powerful library for recording and replaying HTTP requests and responses")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("EasyPost")]
[assembly: AssemblyProduct("EasyVCR")]
[assembly: AssemblyCopyright("Copyright © 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Make "private" methods testable.
#if DEBUG
[assembly: InternalsVisibleTo("EasyVCR.Tests")]
#endif
