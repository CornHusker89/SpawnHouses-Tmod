using System.Collections.Generic;

namespace SpawnHouses.Structures;

public delegate void TreeVisitor<T>(T nodeData, LinkedList<NodeTree<T>> children);

public class NodeTree<T>
{
    public T data;
    public LinkedList<NodeTree<T>> Children;

    public NodeTree(T data)
    {
        this.data = data;
        Children = new LinkedList<NodeTree<T>>();
    }

    public void AddChild(T data)
    {
        Children.AddFirst(new NodeTree<T>(data));
    }

    public NodeTree<T> GetChild(int i)
    {
        foreach (NodeTree<T> n in Children)
            if (--i == 0)
                return n;
        
        return null;
    }

    public void Traverse(NodeTree<T> node, TreeVisitor<T> visitor)
    {
        visitor(node.data, node.Children);
        foreach (NodeTree<T> child in node.Children)
            Traverse(child, visitor);
    }
}
