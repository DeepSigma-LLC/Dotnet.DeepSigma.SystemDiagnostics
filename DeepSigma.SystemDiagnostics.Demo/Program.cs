using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Formatting;

var snapshot = SystemDiagnostics.GetSnapshot();
Console.WriteLine(snapshot.ToReadableString());
