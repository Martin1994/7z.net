using SevenZip.Native;
using System;
using static SevenZip.SevenZipItemTree;

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
    public uint Directories => _tree.Nodes[_index].Directories;
    public uint Files => _tree.Nodes[_index].Files;

    public bool IsRoot => _index == ROOT_INDEX;
    public SevenZipItemNode Parent => new SevenZipItemNode(tree: _tree, index: _tree.Nodes[_index].ParentIndex);

    public bool HasDetail
    {
        get
        {
            uint id = Id;
            if (id == ROOT_ID || id == UNTRACKED_DIR_ID)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public SevenZipItem Detail
    {
        get
        {
            uint id = Id;
            if (id == ROOT_ID || id == UNTRACKED_DIR_ID)
            {
                throw new InvalidOperationException($"Item {Name} does not have detailed information in the archive.");
            }
            return _tree.Archive[id];
        }
    }

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

    public int ChildrenCount
    {
        get
        {
            if (Type != SevenZipItemType.Directory)
            {
                throw new InvalidOperationException("Only a directory contains children.");
            }

            return _tree.Nodes[_index].Children!.Count;
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
    internal const int ROOT_INDEX = 0;

    public const uint ROOT_ID = 0xFFFFFFFF;
    public const uint UNTRACKED_DIR_ID = 0xFFFFFFFE;

    internal struct Node
    {
        public uint Id;
        public readonly SevenZipItemType Type;
        public readonly string Name;
        public readonly int ParentIndex;
        public readonly Dictionary<string, int>? Children;
        public uint Directories = 0;
        public uint Files = 0;

        public Node(uint id, SevenZipItemType type, string name, int parent)
        {
            Id = id;
            Type = type;
            Name = name;
            ParentIndex = parent;
            Children = type == SevenZipItemType.Directory ? new Dictionary<string, int>() : null;
        }
    }

    public SevenZipInArchive Archive { get; init; }

    internal readonly Node[] Nodes;

    public SevenZipItemNode Root => new SevenZipItemNode(
        tree: this,
        index: ROOT_INDEX
    );

    public SevenZipItemTree(SevenZipInArchive arc)
    {
        Archive = arc;
        ref IInArchive native = ref arc.Native;

        PROPVARIANT prop;
        uint num = arc.Count;
        if (num >= 0x80000000)
        {
            throw new IndexOutOfRangeException($"The archive contains too many items: {num}");
        }

        List<Node> nodes = new List<Node>((int)num + 1)
        {
            new Node(ROOT_ID, SevenZipItemType.Directory, "", ROOT_INDEX)
        };

        for (uint i = 0; i < num; i++)
        {
            native.GetProperty(i, PROPID.kpidIsDeleted, out prop);
            bool deleted = prop.ReadBool(false);
            if (deleted)
            {
                continue;
            }

            native.GetProperty(i, PROPID.kpidPath, out prop);
            string path = prop.ReadOptionalString() ?? "";
            native.GetProperty(i, PROPID.kpidIsDir, out prop);
            bool isDir = prop.ReadBool();

            Add(nodes, i, path, isDir);
        }

        Nodes = new Node[nodes.Count];
        for (int i = 0; i < Nodes.Length; i++)
        {
            Nodes[i] = nodes[i];
        }
        TrackGrandChildrenCount(ref Nodes[0]);
    }

    private static void Add(List<Node> nodes, uint id, string path, bool isDir)
    {
        string[] pathNodes = path.Split(Path.DirectorySeparatorChar);
        int parentIndex = ROOT_INDEX;
        for (int i = 0; i < pathNodes.Length - 1; i++)
        {
            parentIndex = GetOrAddDirectory(nodes, UNTRACKED_DIR_ID, parentIndex, pathNodes[i]);
        }

        string name = pathNodes.Last();
        if (isDir)
        {
            GetOrAddDirectory(nodes, id, parentIndex, name);
        }
        else
        {
            var parentNode = nodes[parentIndex];
            parentNode.Children!.Add(name, nodes.Count);
            nodes.Add(new Node(id, isDir ? SevenZipItemType.Directory : SevenZipItemType.File, name, parentIndex));
        }
    }

    private static int GetOrAddDirectory(List<Node> nodes, uint id, int parentIndex, string name)
    {
        var parentNode = nodes[parentIndex];
        if (parentNode.Children!.TryGetValue(name, out var existingNodeIndex))
        {
            if (id != UNTRACKED_DIR_ID)
            {
                // Update ID
                // This means the archive tracks a directory item, but appears after its children
                var node = nodes[existingNodeIndex];
                node.Id = id;
                nodes[existingNodeIndex] = node;
            }
            return existingNodeIndex;
        }

        // Need to create a new node record
        parentNode.Children!.Add(name, nodes.Count);
        int index = nodes.Count;
        Node newNode = new Node(id, SevenZipItemType.Directory, name, parentIndex);
        nodes.Add(newNode);
        return index;
    }

    private void TrackGrandChildrenCount(ref Node node)
    {
        var children = node.Children;
        if (children == null)
        {
            return;
        }

        foreach (uint index in children.Values)
        {
            ref Node child = ref Nodes[index];
            TrackGrandChildrenCount(ref child);
            if (child.Type == SevenZipItemType.Directory)
            {
                node.Files += child.Files;
                node.Directories += child.Directories + 1;
            }
            else
            {
                node.Files++;
            }
        }
    }
}
