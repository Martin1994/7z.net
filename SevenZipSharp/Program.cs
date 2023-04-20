using System.Runtime.InteropServices;
using SevenZip;
using SevenZip.Native;

Console.WriteLine("Started");

var path = args[0];
var extractDest = args[1];

using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

var arc = new SevenZipInArchive(path, stream);

Console.WriteLine("Items: {0}", arc.Count);

PrintTree(arc.FileTree);

arc.ExtractAll(NAskMode.kExtract, new TestExtractCallback(arc, extractDest));

Console.WriteLine("Ended");

void PrintTree(SevenZipItemTree tree)
{
    PrintNodes(tree, tree.List(), "|-");
}

void PrintNodes(SevenZipItemTree tree, SevenZipItemNode[] nodes, string prefix)
{
    foreach (var node in nodes.OrderBy(n => n.Name))
    {
        if (node.Type == SevenZipItemType.File)
        {
            Console.WriteLine("{0}- {1}", prefix, node.Name);
        }
        else
        {
            Console.WriteLine("{0}+ {1}", prefix, node.Name);
            PrintNodes(tree, tree.List(node.Index), prefix.Replace('-', ' ') + "|-");
        }
    }
}

class TestExtractCallback : IManagedArchiveExtractCallback
{
    private readonly SevenZipInArchive _arc;
    private readonly string _outPath;

    private readonly HashSet<string> _createdDir = new();

    private ulong _total;
    private Action? _onOperationComplete;

    public TestExtractCallback(SevenZipInArchive arc, string outPath)
    {
        _arc = arc;
        _outPath = outPath;
        _total = arc.PhysicalSize;
    }

    public Stream? GetStream(uint index, NAskMode askExtractMode)
    {
        if (askExtractMode != NAskMode.kExtract)
        {
            return null;
        }

        var item = _arc[index];
        string relativePath = item.Path;
        string destinationPath = Path.Join(_outPath, item.Path);

        if (item.Type == SevenZipItemType.Directory)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Directory.CreateDirectory(destinationPath);
            }
            else
            {
                Directory.CreateDirectory(destinationPath, item.UnixFileMode);
            }
            _createdDir.Add(destinationPath);

            return null;
        }

        string? parentDir = Path.GetDirectoryName(destinationPath);
        if (parentDir != null && !_createdDir.Contains(parentDir))
        {
            Directory.CreateDirectory(parentDir);
            _createdDir.Add(parentDir);
        }

        var fileStream = new FileStream(destinationPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        _onOperationComplete = () =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.SetAttributes(fileStream.SafeFileHandle, item.WindowsFileAttributes);
            }
            else
            {
                File.SetUnixFileMode(fileStream.SafeFileHandle, item.UnixFileMode);
            }
        };

        Console.WriteLine("Extract {0}", _arc[index].Path);
        return fileStream;
    }

    public void PrepareOperation(NAskMode askExtractMode)
    {
        Console.WriteLine("PrepareOperation {0}", askExtractMode);
    }

    public void SetCompleted(in ulong size)
    {
        Console.WriteLine("Completed {0}/{1}", size, _total);
    }

    public void SetOperationResult(NOperationResult opRes)
    {
        Console.WriteLine("Result: {0}", Enum.GetName(opRes));
        if (_onOperationComplete != null)
        {
            _onOperationComplete();
            _onOperationComplete = null;
        }
    }

    public void SetTotal(ulong size)
    {
        _total = size;
        Console.WriteLine("Set total {0}", _total);
    }
}
