using SevenZip;
using SevenZip.Native;

Console.WriteLine("Started");

var path = args[0];

using var stream = new FileStream(path, FileMode.Open);

var arc = new SevenZipInArchive(path, stream);

Console.WriteLine("Items: {0}", arc.Count);

PrintTree(arc.FileTree);

arc.ExtractAll(NAskMode.kTest, new TestExtractCallback(arc));

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
    private ulong _total;

    public TestExtractCallback(SevenZipInArchive arc)
    {
        _arc = arc;
        _total = arc.PhysicalSize;
    }

    public unsafe void GetStream(uint index, out void* outStream, NAskMode askExtractMode)
    {
        if (askExtractMode != NAskMode.kTest)
        {
            throw new NotImplementedException();
        }
        outStream = null;
        Console.WriteLine("Test {0}", _arc[index].Path);
    }

    public void PrepareOperation(NAskMode askExtractMode)
    {
        Console.WriteLine("PrepareOperation");
    }

    public void SetCompleted(in ulong size)
    {
        Console.WriteLine("Completed {0}/{1}", size, _total);
    }

    public void SetOperationResult(NOperationResult opRes)
    {
        Console.WriteLine("Result: {0}", Enum.GetName(opRes));
    }

    public void SetTotal(ulong size)
    {
        _total = size;
        Console.WriteLine("Set total {0}", _total);
    }
}
