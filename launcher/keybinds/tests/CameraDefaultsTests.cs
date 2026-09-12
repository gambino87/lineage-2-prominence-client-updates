using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LocalL2Keys;
class CameraDefaultsTests {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(string[] args){
  var original=File.ReadAllBytes(args[0]);var result=CameraDefaults.Updated(original);Check(result!=null,"Stock reset must be detected");
  var encoding=Encoding.GetEncoding(28591);string before=encoding.GetString(PortableCrypt.Decode(original)),after=encoding.GetString(PortableCrypt.Decode(result));
  string right=Regex.Match(after,@"^RightMouse=.*$",RegexOptions.Multiline).Value;
  Check(right.Contains("CameraRotationModeOn") && right.Contains("CameraRotationModeOff") && !right.Contains("FixedDefaultCamera"),"Keep rotation, remove reset");
  Check(Regex.Replace(before,@"^RightMouse=.*$","",RegexOptions.Multiline)==Regex.Replace(after,@"^RightMouse=.*$","",RegexOptions.Multiline),"Preserve every other setting");
  Check(CameraDefaults.Updated(result)==null,"Fix must be idempotent");
  var fixture=Path.Combine(args[1],Guid.NewGuid().ToString("N"));Directory.CreateDirectory(fixture);string target=Path.Combine(fixture,"user.ini"),backup=Path.Combine(fixture,"backup.ini");File.WriteAllBytes(target,original);
  if(System.Diagnostics.Process.GetProcessesByName("L2").Length==0){CameraDefaults.Apply(target,backup);Check(File.ReadAllBytes(backup).SequenceEqual(original),"Backup must preserve original bytes");Check(!CameraDefaults.Needs(target),"Saved fix must pass check");}
  Console.WriteLine("Camera default checks passed; unrelated settings preserved.");
 }
}
