namespace SevenZip;

public struct SevenZipItemNode
{
    private readonly SevenZipItemTree _tree;
    private readonly int _index;

    public SevenZipItemNode(SevenZipItemTree tree, int index)
    {
        _tree = tree;
        _index = index;
    }

    public string Name => _tree.Nodes[_index].Name;
    public uint Id => _tree.Nodes[_index].Id;
    public SevenZipItemType Type => _tree.Nodes[_index].Type;

    public IEnumerable<SevenZipItemNode> Children
    {
        get
        {
            if (Type != SevenZipItemType.Directory)
            {
                throw new InvalidOperationException("Only a directory contains children.");
            }

            foreach (int child in _tree.Nodes[_index].Children!.Values)
            {
                yield return new SevenZipItemNode(
                    tree: _tree,
                    index: child
                );
            }
        }
    }

    public SevenZipItemNode this[string childName]
    {
        get
        {
            if (Type != SevenZipItemType.Directory)
            {
                throw new InvalidOperationException("Only a directory contains children.");
            }

            return new SevenZipItemNode(
                tree: _tree,
                index: _tree.Nodes[_index].Children![childName]
            );
        }
    }
}

public class SevenZipItemTree
{
    private const int ROOT_INDEX = 0;

    private const uint ROOT_ID = 0xFFFFFFFF;
    private const uint UNTRACKED_DIR_ID = 0xFFFFFFFE;

    internal struct Node
    {
        public uint Id;
        public readonly SevenZipItemType Type;
        public readonly string Name;
        public readonly int ParentIndex;
        public readonly Dictionary<string, int>? Children;

        public Node(uint id, SevenZipItemType type, string name, int parent)
        {
            Id = id;
            Type = type;
            Name = name;
            ParentIndex = parent;
            Children = type == SevenZipItemType.Directory ? new Dictionary<string, int>() : null;
        }
    }

    internal readonly List<Node> Nodes;

    public SevenZipItemNode Root => new SevenZipItemNode(
        tree: this,
        index: ROOT_INDEX
    );

    public SevenZipItemTree(int initialCapacity)
    {
        Nodes = new List<Node>(initialCapacity + 1)
        {
            new Node(ROOT_ID, SevenZipItemType.Directory, "", ROOT_INDEX)
        };
    }

    public void Add(uint id, string path, bool isDir)
    {
        string[] pathNodes = path.Split(Path.DirectorySeparatorChar);
        int parentIndex = ROOT_INDEX;
        for (int i = 0; i < pathNodes.Length - 1; i++)
        {
            parentIndex = GetOrAddDirectory(UNTRACKED_DIR_ID, parentIndex, pathNodes[i]);
        }

        string name = pathNodes.Last();
        if (isDir)
        {
            GetOrAddDirectory(id, parentIndex, name);
        }
        else
        {
            var parentNode = Nodes[parentIndex];
            parentNode.Children!.Add(name, Nodes.Count);
            Nodes.Add(new Node(id, isDir ? SevenZipItemType.Directory : SevenZipItemType.File, name, parentIndex));
        }
    }

    private int GetOrAddDirectory(uint id, int parentIndex, string name)
    {
        var parentNode = Nodes[parentIndex];
        if (parentNode.Children!.TryGetValue(name, out var existingNodeIndex))
        {
            if (id != UNTRACKED_DIR_ID)
            {
                // Update ID
                var node = Nodes[existingNodeIndex];
                node.Id = id;
                Nodes[existingNodeIndex] = node;
            }
            return existingNodeIndex;
        }

        parentNode.Children!.Add(name, Nodes.Count);
        int index = Nodes.Count;
        Nodes.Add(new Node(id, SevenZipItemType.Directory, name, parentIndex));
        return index;
    }
}
