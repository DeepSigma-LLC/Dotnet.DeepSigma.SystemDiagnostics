using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Formatting;

var snapshot = SystemDiagnostics.GetSnapshot();
Console.WriteLine(snapshot.ToReadableString());
Console.WriteLine(SystemDiagnostics.GetBatteries().FormatBatteries());
