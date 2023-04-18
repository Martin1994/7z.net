namespace SevenZip;

public struct SevenZipFileNode
{
    public int Id { get; init; } // Used by a 7z archive
    public SevenZipFileNodeType Type { get; init; }
    public string Name { get; init; }
    public int Index { get; init; } // Used by the file tree
    public int ParentIndex { get; init; } // Used by the file tree
}

public enum SevenZipFileNodeType
{
    File,
    Directory
}

public class SevenZipFileTree
{
    private const int ROOT_INDEX = 0;

    private const int ROOT_ID = -1;
    private const int UNTRACKED_DIR_ID = -2;

    private struct Node
    {
        public readonly int Id;
        public readonly SevenZipFileNodeType Type;
        public readonly string Name;
        public readonly int ParentIndex;
        public readonly Dictionary<string, int>? Children;

        public Node(int id, SevenZipFileNodeType type, string name, int parent)
        {
            Id = id;
            Type = type;
            Name = name;
            ParentIndex = parent;
            Children = type == SevenZipFileNodeType.Directory ? new Dictionary<string, int>() : null;
        }
    }

    private readonly List<Node> _nodes;

    public SevenZipFileTree(int initialCapacity)
    {
        _nodes = new List<Node>(initialCapacity + 1)
        {
            new Node(ROOT_ID, SevenZipFileNodeType.Directory, "", ROOT_ID)
        };
    }

    public void Add(int id, string path, bool isDir)
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
            _nodes.Add(new Node(id, isDir ? SevenZipFileNodeType.Directory : SevenZipFileNodeType.File, name, parentIndex));
        }
    }

    private int GetOrAddDirectory(int id, int parentIndex, string name)
    {
        if (parentIndex >= 0)
        {
            var parentNode = _nodes[parentIndex];
            if (parentNode.Children!.TryGetValue(name, out var existingNodeIndex))
            {
                if (id >= 0)
                {
                    // Update ID
                    _nodes[existingNodeIndex] = new Node(id, SevenZipFileNodeType.Directory, name, parentIndex);
                }
                return existingNodeIndex;
            }
            
            parentNode.Children!.Add(name, _nodes.Count);
        }

        int index = _nodes.Count;
        _nodes.Add(new Node(id, SevenZipFileNodeType.Directory, name, parentIndex));
        return index;
    }

    public SevenZipFileNode[] List(int index = ROOT_INDEX)
    {
        if (index < 0 || index > _nodes.Count)
        {
            throw new IndexOutOfRangeException($"Invalid file tree index: {index}. Expected in range [0, {_nodes.Count - 1}]");
        }

        Node node = _nodes[index];

        if (node.Type != SevenZipFileNodeType.Directory)
        {
            throw new ArgumentException($"{node.Name} is file, not a directory.");
        }

        return node.Children!.Values.Select(childIndex => {
            var child = _nodes[childIndex];
            return new SevenZipFileNode()
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

