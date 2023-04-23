## 7-Zip.NET

Cross platform .NET wrapper of the 7-Zip dynamic-link library.

Due to license limitation, you need place `7z.dll` or `7z.so` under library search path.

### Example

```csharp
using SevenZip;

var path = args[0];
var extractDest = args[1];

using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

var arc = new SevenZipInArchive(path, stream);

arc.ExtractAll(NAskMode.kExtract, new TestExtractCallback(arc, extractDest));
```
