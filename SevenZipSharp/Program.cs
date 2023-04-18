using SevenZip;

Console.WriteLine("Started");

var path = args[0];

var stream = new FileStream(path, FileMode.Open);

var arc = new SevenZipInArchive(path, stream);

Console.WriteLine("Items: {0}", arc.Count);

PrintTree(arc.FileTree);

Console.WriteLine("Ended");

void PrintTree(SevenZipFileTree tree)
{
    PrintNodes(tree, tree.List(), "|-");
}

void PrintNodes(SevenZipFileTree tree, SevenZipFileNode[] nodes, string prefix)
{
    foreach (var node in nodes.OrderBy(n => n.Name))
    {
        if (node.Type == SevenZipFileNodeType.File)
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
