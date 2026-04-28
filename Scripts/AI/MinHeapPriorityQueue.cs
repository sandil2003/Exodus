using System.Collections.Generic;

public class MinHeapPriorityQueue
{
    private List<NodePriority> heap = new List<NodePriority>();
    public int Count => heap.Count;

    public void Enqueue(int id, float priority)
    {
        heap.Add(new NodePriority(id, priority));
        // Heapify up logic
    }

    public int Dequeue()
    {
        int rootId = heap[0].id;
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        // Heapify down logic
        return rootId;
    }

    private struct NodePriority
    {
        public int id; public float priority;
        public NodePriority(int i, float p) { id = i; priority = p; }
    }
}