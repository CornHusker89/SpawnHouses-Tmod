using System.Collections.Generic;

namespace SpawnHouses.Structures;

public delegate void TreeVisitor<T>(T nodeData);

public class NodeTree<T>
{
    public T data;
    protected LinkedList<NodeTree<T>> _children;

    public NodeTree(T data)
    {
        this.data = data;
        _children = new LinkedList<NodeTree<T>>();
    }

    public void AddChild(T data)
    {
        _children.AddFirst(new NodeTree<T>(data));
    }

    public NodeTree<T> GetChild(int i)
    {
        foreach (NodeTree<T> n in _children)
            if (--i == 0)
                return n;
        
        return null;
    }

    public void Traverse(NodeTree<T> node, TreeVisitor<T> visitor)
    {
        visitor(node.data);
        foreach (NodeTree<T> child in node._children)
            Traverse(child, visitor);
    }
}
