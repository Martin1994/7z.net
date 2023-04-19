namespace SevenZip;

public struct SevenZipItemNode
{
    public uint Id { get; init; } // Used by a 7z archive
    public SevenZipItemType Type { get; init; }
    public string Name { get; init; }
    public int Index { get; init; } // Used by the file tree
    public int ParentIndex { get; init; } // Used by the file tree
}

public class SevenZipItemTree
{
    private const int ROOT_INDEX = 0;

    private const uint ROOT_ID = 0xFFFFFFFF;
    private const uint UNTRACKED_DIR_ID = 0xFFFFFFFE;

    private struct Node
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

    private readonly List<Node> _nodes;

    public SevenZipItemTree(int initialCapacity)
    {
        _nodes = new List<Node>(initialCapacity + 1)
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
            var parentNode = _nodes[parentIndex];
            parentNode.Children!.Add(name, _nodes.Count);
            _nodes.Add(new Node(id, isDir ? SevenZipItemType.Directory : SevenZipItemType.File, name, parentIndex));
        }
    }

    private int GetOrAddDirectory(uint id, int parentIndex, string name)
    {
        var parentNode = _nodes[parentIndex];
        if (parentNode.Children!.TryGetValue(name, out var existingNodeIndex))
        {
            if (id != UNTRACKED_DIR_ID)
            {
                // Update ID
                var node = _nodes[existingNodeIndex];
                node.Id = id;
                _nodes[existingNodeIndex] = node;
            }
            return existingNodeIndex;
        }
        
        parentNode.Children!.Add(name, _nodes.Count);
        int index = _nodes.Count;
        _nodes.Add(new Node(id, SevenZipItemType.Directory, name, parentIndex));
        return index;
    }

    public SevenZipItemNode[] List(int index = ROOT_INDEX)
    {
        if (index < 0 || index > _nodes.Count)
        {
            throw new IndexOutOfRangeException($"Invalid file tree index: {index}. Expected in range [0, {_nodes.Count - 1}]");
        }

        Node node = _nodes[index];

        if (node.Type != SevenZipItemType.Directory)
        {
            throw new ArgumentException($"{node.Name} is file, not a directory.");
        }

        return node.Children!.Values.Select(childIndex => {
            var child = _nodes[childIndex];
            return new SevenZipItemNode()
            {
                Id = child.Id,
                Type = child.Type,
                Name = child.Name,
                Index = childIndex,
                ParentIndex = child.ParentIndex
            };
        }).ToArray();
    }
}

